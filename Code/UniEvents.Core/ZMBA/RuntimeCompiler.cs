using System;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using System.Linq.Expressions;



namespace ZMBA {

	public static class RuntimeCompiler {
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

		//private static class RCCache_DB<T> where T : new() {
		//	public static object Lock = new object();
		//	public static DataReaderDelegate<T> DBDataReader;
		//}


		/// <summary>
		/// Compiles a function that copies fields with the same name and type from one object to the other. 
		/// Be sure to only compile a single function for a set of types and re-use it, because this is slow and the function does not get garbage collected, but extremely fast after compiled.
		/// </summary>
		public static CopyIntoDelegate<T, S> CompileShallowFieldCopier<T, S>() {
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


		public static CopyIntoDelegate<T, S> CompileShallowPropertyCopier<T, S>() {
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


		//public static DataReaderDelegate<T> CompileDBDataReader<T>() where T : new() {
		//	if (RCCache_DB<T>.DBDataReader is null) {
		//		lock (RCCache_DB<T>.Lock) {
		//			if (RCCache_DB<T>.DBDataReader != null) {
		//				return RCCache_DB<T>.DBDataReader;
		//			}
		//			RCCache_DB<T>.DBDataReader = (DataReaderDelegate<T>)CreateDelegate();
		//		}
		//	}
		//	return RCCache_DB<T>.DBDataReader;

		//	Delegate CreateDelegate() {

		//	}
		//}



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

		public static Func<T> CompileGenericDefaultConstructor<T>() {
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

		public static TDelegate CompileMultiArgumentConstructor<TDelegate>() where TDelegate : class {
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
				BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance;
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


	}

}
