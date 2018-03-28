using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using ZMBA.Exceptions;
using static System.Runtime.CompilerServices.MethodImplOptions;


namespace ZMBA {

   public static class RuntimeCompiler {
      private static readonly Type TypeOfDelegate = typeof(Delegate);

      public delegate void CopyIntoDelegate<T, S>(T location, S src);
      public delegate T DataReaderDelegate<T>(IDataReader reader) where T : new();

      private static class RCCache_Ctors<T> {
         public static object CtorLock = new object();
         public static Func<T> DefaultConstructor;
         public static T MultiArgConstructor;
      }

      private static class RCCache_Copiers<T, S> {
         public static object Lock = new object();
         public static CopyIntoDelegate<T, S> ShallowFieldCopier;
         public static CopyIntoDelegate<T, S> ShallowPropertyCopier;
      }

      #region Copiers
      
      /// <summary>
      /// Compiles a function that copies fields with the same name and type from one object to the other. 
      /// Be sure to only compile a single function for a set of types and re-use it, because this is slow and the function does not get garbage collected, but extremely fast after compiled.
      /// </summary>
      public static CopyIntoDelegate<T, S> GetShallowFieldCopier<T, S>() {
         if (RCCache_Copiers<T, S>.ShallowFieldCopier is null) {
            lock (RCCache_Copiers<T, S>.Lock) {
               if (RCCache_Copiers<T, S>.ShallowFieldCopier != null) {
                  return RCCache_Copiers<T, S>.ShallowFieldCopier;
               }
               RCCache_Copiers<T, S>.ShallowFieldCopier = (CopyIntoDelegate<T, S>)CreateDelegate();
            }
         }
         return RCCache_Copiers<T, S>.ShallowFieldCopier;

         Delegate CreateDelegate() {
            Type targetType = typeof(T);
            Type srcType = typeof(S);

            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            FieldInfo[] targetFields = targetType.GetFields(flags);
            FieldInfo[] srcFields = targetType == srcType ? targetFields : srcType.GetFields(flags);

            DynamicMethod dynmethod = new System.Reflection.Emit.DynamicMethod("ShallowFieldCopy", typeof(void), new Type[2]{ targetType, srcType }, true);
            ILGenerator gen = dynmethod.GetILGenerator();

            foreach (FieldInfo targetField in targetFields) {
               FieldInfo srcField = targetType == srcType ? targetField : srcFields.FirstOrDefault(f=>f.Name.EqIgCase(targetField.Name));
               if (srcField is null) { continue; }

               if (targetField.FieldType == srcField.FieldType) {
                  gen.Emit(OpCodes.Ldarg_0);
                  gen.Emit(OpCodes.Ldarg_1);
                  gen.Emit(OpCodes.Ldfld, srcField);
                  gen.Emit(OpCodes.Stfld, targetField);
               } else {
                  TypeConverter converter = targetField.FieldType.GetTypeConverter();
                  FieldInfo converterField = GetStaticConverterFieldByConverter(converter);
                  if (converterField != null && converter.CanConvertFrom(srcField.FieldType)) {
                     gen.Emit(OpCodes.Ldarg_0);
                     gen.Emit(OpCodes.Ldsfld, converterField);
                     gen.Emit(OpCodes.Ldarg_1);
                     gen.Emit(OpCodes.Ldfld, srcField);
                     gen.Emit(OpCodes.Box, srcField.FieldType);
                     gen.Emit(OpCodes.Callvirt, converter.GetType().GetMethod(nameof(TypeConverter.ConvertFrom), new Type[] { typeof(object) }));
                     gen.Emit(OpCodes.Unbox_Any, targetField.FieldType);
                     gen.Emit(OpCodes.Stfld, targetField);
                  }
               }
            }
            gen.Emit(OpCodes.Ret);
            return dynmethod.CreateDelegate(typeof(CopyIntoDelegate<T, S>));
         }
      }


      public static CopyIntoDelegate<T, S> GetShallowPropertyCopier<T, S>() {
         if (RCCache_Copiers<T, S>.ShallowPropertyCopier is null) {
            lock (RCCache_Copiers<T, S>.Lock) {
               if (RCCache_Copiers<T, S>.ShallowPropertyCopier != null) {
                  return RCCache_Copiers<T, S>.ShallowPropertyCopier;
               }
               RCCache_Copiers<T, S>.ShallowPropertyCopier = (CopyIntoDelegate<T, S>)CreateDelegate();
            }
         }
         return RCCache_Copiers<T, S>.ShallowPropertyCopier;

         Delegate CreateDelegate() {
            Type targetType = typeof(T);
            Type srcType = typeof(S);

            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            PropertyInfo[] targetProps = targetType.GetProperties(flags);
            PropertyInfo[] srcProps = targetType == srcType ? targetProps : srcType.GetProperties(flags);

            DynamicMethod dynmethod = new DynamicMethod("ShallowPropCopy", typeof(void), new Type[2]{ targetType, srcType }, true);
            ILGenerator gen = dynmethod.GetILGenerator();

            foreach (PropertyInfo targetProp in targetProps) {
               if (!targetProp.CanWrite) { continue; }
               PropertyInfo srcProp = targetType == srcType ? targetProp : srcProps.FirstOrDefault(f=>f.Name.EqIgCase(targetProp.Name));
               if (srcProp is null || !srcProp.CanRead) { continue; }

               if (targetProp.PropertyType == srcProp.PropertyType) {
                  gen.Emit(OpCodes.Ldarg_0);
                  gen.Emit(OpCodes.Ldarg_1);
                  gen.Emit(OpCodes.Callvirt, srcProp.GetMethod);
                  gen.Emit(OpCodes.Callvirt, targetProp.SetMethod);
               } else {
                  TypeConverter converter = targetProp.PropertyType.GetTypeConverter();
                  FieldInfo converterField = GetStaticConverterFieldByConverter(converter);
                  if (converterField != null && converter.CanConvertFrom(srcProp.PropertyType)) {
                     gen.Emit(OpCodes.Ldarg_0);
                     gen.Emit(OpCodes.Ldsfld, converterField);
                     gen.Emit(OpCodes.Ldarg_1);
                     gen.Emit(OpCodes.Callvirt, srcProp.GetMethod);
                     gen.Emit(OpCodes.Box, srcProp.PropertyType);
                     gen.Emit(OpCodes.Callvirt, converter.GetType().GetMethod(nameof(TypeConverter.ConvertFrom), new Type[] { typeof(object) }));
                     gen.Emit(OpCodes.Unbox_Any, targetProp.PropertyType);
                     gen.Emit(OpCodes.Callvirt, targetProp.SetMethod);
                  }
               }
            }

            gen.Emit(OpCodes.Ret);
            return dynmethod.CreateDelegate(typeof(CopyIntoDelegate<T, S>));
         }
      }


      #endregion Copiers


      #region Constructors

      public static Func<T> GetDefaultConstructor<T>() {
         if (RCCache_Ctors<T>.DefaultConstructor is null) {
            lock (RCCache_Ctors<T>.CtorLock) {
               if (RCCache_Ctors<T>.DefaultConstructor != null) {
                  return RCCache_Ctors<T>.DefaultConstructor;
               }
               ConstructorInfo ctor = typeof(T).GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
               NewExpression calleeExp = Expression.New(ctor);
               LambdaExpression lambda = Expression.Lambda(typeof(Func<T>), calleeExp);
               RCCache_Ctors<T>.DefaultConstructor = (Func<T>)(object)lambda.Compile();
            }
         }
         return RCCache_Ctors<T>.DefaultConstructor;
      }

      public static TDelegate GetConstructor<TDelegate>() where TDelegate : class {
         if (RCCache_Ctors<TDelegate>.MultiArgConstructor is null) {
            lock (RCCache_Ctors<TDelegate>.CtorLock) {
               if (RCCache_Ctors<TDelegate>.MultiArgConstructor != null) {
                  return RCCache_Ctors<TDelegate>.MultiArgConstructor;
               }
               RCCache_Ctors<TDelegate>.MultiArgConstructor = (TDelegate)CreateDelegate();
            }
         }
         return RCCache_Ctors<TDelegate>.MultiArgConstructor;

         object CreateDelegate() {
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            Type delegateType = typeof(TDelegate);
            MethodInfo invoker = delegateType.GetMethod("Invoke");
            ParameterInfo[] invokerParams = invoker.GetParameters();
            Type classType = invoker.ReturnType;

            ParameterExpression[] args = new ParameterExpression[invokerParams.Length];
            Type[] argTypes = new Type[args.Length];

            for (int i = 0; i < invokerParams.Length; i++) {
               args[i] = Expression.Parameter(argTypes[i] = invokerParams[i].ParameterType);
            }

            ConstructorInfo calleeInfo = classType.GetConstructor(flags, null, argTypes, null);
            NewExpression calleeExp = Expression.New(calleeInfo, args);
            LambdaExpression lambda = Expression.Lambda(delegateType, calleeExp, args);

            return lambda.Compile();
         }
      }


      #endregion Constructors


      #region "Functions"


      /// <summary>
      /// Compiles a function to call a private or internal instance method with no performance cost after creation
      /// </summary>
      /// <typeparam name="TDelegate">Func<TRet>: A delegate where the first param is the instance and the final param is the result, if there is one.</typeparam>
      public static TDelegate CompileFunctionCaller<TDelegate>(MethodInfo calleeInfo) {
         if (!TypeOfDelegate.IsAssignableFrom(typeof(TDelegate))) { throw new UnsupportedArgumentTypeException<TDelegate>(param: nameof(TDelegate)); }

         Type delegateType = typeof(TDelegate);
         ParameterInfo[] callerArgs = delegateType.GetMethod("Invoke").GetParameters();
         ParameterInfo[] calleeArgs = calleeInfo.GetParameters();

         Type[] callerArgTypes = new Type[callerArgs.Length];
         Type[] calleeArgTypes = new Type[calleeArgs.Length];
         ParameterExpression[] callerParams = new ParameterExpression[callerArgs.Length];
         Expression[] calleeParams = new Expression[calleeArgs.Length];
         Type instanceType = callerArgTypes[0] = callerArgs[0].ParameterType;
         Expression instanceParam = callerParams[0] = Expression.Parameter(instanceType);

         if (instanceType != calleeInfo.DeclaringType) {
            instanceParam = Expression.Convert(instanceParam, calleeInfo.DeclaringType);
         }

         for (int idx = 1; idx < callerArgs.Length; idx++) {
            int ee = idx-1;   //call[ee] index
            calleeArgTypes[ee] = callerArgTypes[idx] = callerArgs[idx].ParameterType;
            calleeParams[ee] = callerParams[idx] = Expression.Parameter(callerArgs[idx].IsOut ? callerArgTypes[idx].MakeByRefType() : callerArgTypes[idx]);
            if (calleeArgs[ee].ParameterType != calleeArgTypes[ee]) {
               calleeArgTypes[ee] = calleeArgs[ee].IsOut ? calleeArgs[ee].ParameterType.MakeByRefType() : calleeArgs[ee].ParameterType;
               calleeParams[ee] = Expression.Convert(calleeParams[ee], calleeArgTypes[ee]);
            }
         }

         MethodCallExpression calleeExp = Expression.Call(instanceParam, calleeInfo, calleeParams);
         LambdaExpression lambda = Expression.Lambda(delegateType, calleeExp, callerParams);
         return (TDelegate)(object)lambda.Compile();
      }

      public static TDelegate CompileFunctionCaller<TDelegate>(Type type, string name, Type[] args) {
         BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.InvokeMethod;
         MethodInfo calleeInfo = type.GetMethod(name, flags, args) ?? type.GetMethod(name, flags | BindingFlags.FlattenHierarchy, args);
         return CompileFunctionCaller<TDelegate>(calleeInfo);
      }

      public static TDelegate CompileFunctionCaller<TDelegate>(string fullyQualifiedTypeName, string name, Type[] args) {
         return CompileFunctionCaller<TDelegate>(Type.GetType(fullyQualifiedTypeName, true, true), name, args);
      }

      /// <summary>
      /// Compiles a function to call a private or internal static method with no performance cost after creation
      /// </summary>
      /// <typeparam name="TDelegate">Func<TRet>: A delegate where the first param is the instance and the final param is the result, if there is one.</typeparam>
      public static TDelegate CompileStaticFunctionCaller<TDelegate>(MethodInfo calleeInfo) {
         if (!TypeOfDelegate.IsAssignableFrom(typeof(TDelegate))) { throw new UnsupportedArgumentTypeException<TDelegate>(param: nameof(TDelegate)); }

         Type delegateType = typeof(TDelegate);
         ParameterInfo[] callerArgs = delegateType.GetMethod("Invoke").GetParameters();
         ParameterInfo[] calleeArgs = calleeInfo.GetParameters();

         Type[] callerArgTypes = new Type[callerArgs.Length];
         Type[] calleeArgTypes = new Type[calleeArgs.Length];
         ParameterExpression[] callerParams = new ParameterExpression[callerArgs.Length];
         Expression[] calleeParams = new Expression[calleeArgs.Length];

         for (int idx = 0; idx < callerArgs.Length; idx++) {
            calleeArgTypes[idx] = callerArgTypes[idx] = callerArgs[idx].ParameterType;
            calleeParams[idx] = callerParams[idx] = Expression.Parameter(callerArgs[idx].IsOut ? callerArgTypes[idx].MakeByRefType() : callerArgTypes[idx]);
            if (calleeArgs[idx].ParameterType != calleeArgTypes[idx]) {
               calleeArgTypes[idx] = calleeArgs[idx].IsOut ? calleeArgs[idx].ParameterType.MakeByRefType() : calleeArgs[idx].ParameterType;
               calleeParams[idx] = Expression.Convert(calleeParams[idx], calleeArgTypes[idx]);
            }
         }

         MethodCallExpression calleeExp = Expression.Call(null, calleeInfo, calleeParams);
         LambdaExpression lambda = Expression.Lambda(delegateType, calleeExp, callerParams);
         return (TDelegate)(object)lambda.Compile();
      }

      public static TDelegate CompileStaticFunctionCaller<TDelegate>(Type type, string name, Type[] args) {
         BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.InvokeMethod;
         MethodInfo calleeInfo = type.GetMethod(name, flags, args) ?? type.GetMethod(name, flags | BindingFlags.FlattenHierarchy, args);
         return CompileStaticFunctionCaller<TDelegate>(calleeInfo);
      }

      public static TDelegate CompileStaticFunctionCaller<TDelegate>(string fullyQualifiedTypeName, string name, Type[] args) {
         return CompileStaticFunctionCaller<TDelegate>(Type.GetType(fullyQualifiedTypeName, true, true), name, args);
      }

      #endregion


      private static FieldInfo GetStaticConverterFieldByConverter(TypeConverter converter) {
         Type type = typeof(TypeConversion);
         if (converter == TypeConversion.StringConverter) { return type.GetField(nameof(TypeConversion.StringConverter)); }
         if (converter == TypeConversion.CharConverter) { return type.GetField(nameof(TypeConversion.CharConverter)); }
         if (converter == TypeConversion.BoolConverter) { return type.GetField(nameof(TypeConversion.BoolConverter)); }
         if (converter == TypeConversion.SByteConverter) { return type.GetField(nameof(TypeConversion.SByteConverter)); }
         if (converter == TypeConversion.Int16Converter) { return type.GetField(nameof(TypeConversion.Int16Converter)); }
         if (converter == TypeConversion.Int32Converter) { return type.GetField(nameof(TypeConversion.Int32Converter)); }
         if (converter == TypeConversion.Int64Converter) { return type.GetField(nameof(TypeConversion.Int64Converter)); }
         if (converter == TypeConversion.ByteConverter) { return type.GetField(nameof(TypeConversion.ByteConverter)); }
         if (converter == TypeConversion.UInt16Converter) { return type.GetField(nameof(TypeConversion.UInt16Converter)); }
         if (converter == TypeConversion.UInt32Converter) { return type.GetField(nameof(TypeConversion.UInt32Converter)); }
         if (converter == TypeConversion.UInt64Converter) { return type.GetField(nameof(TypeConversion.UInt64Converter)); }
         if (converter == TypeConversion.Float32Converter) { return type.GetField(nameof(TypeConversion.Float32Converter)); }
         if (converter == TypeConversion.Float64Converter) { return type.GetField(nameof(TypeConversion.Float64Converter)); }
         if (converter == TypeConversion.DecimalConverter) { return type.GetField(nameof(TypeConversion.DecimalConverter)); }
         if (converter == TypeConversion.DateTimeConverter) { return type.GetField(nameof(TypeConversion.DateTimeConverter)); }
         if (converter == TypeConversion.TimeSpanConverter) { return type.GetField(nameof(TypeConversion.TimeSpanConverter)); }
         if (converter == TypeConversion.GuidConverter) { return type.GetField(nameof(TypeConversion.GuidConverter)); }
         return null;
      }


   }

}
