using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Reflection;
using Newtonsoft.Json;
using CmpOp = System.Globalization.CompareOptions;
using static System.Runtime.CompilerServices.MethodImplOptions;


namespace ZMBA {

   public static partial class Common {


      #region ************************************** Collections ******************************************************

      [MethodImpl(AggressiveInlining)] public static bool IsEmpty<T>(this T[] arr) => arr is null || arr.Length == 0;
      [MethodImpl(AggressiveInlining)] public static bool IsEmpty<T>(this ICollection<T> arr) => arr is null || arr.Count == 0;

      [MethodImpl(AggressiveInlining)]
      public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue @default = default) {
         TValue value;
         return dict.TryGetValue(key, out value) ? value : @default;
      }

      [MethodImpl(AggressiveInlining)]
      public static TValue GetValueOrDefaultR<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dict, TKey key, TValue @default = default) {
         TValue value;
         return dict.TryGetValue(key, out value) ? value : @default;
      }

      [MethodImpl(AggressiveInlining)] public static bool Includes<T>(this T[] arr, T value) => Array.IndexOf(arr, value) >= 0;

      #endregion



      #region ************************************** Type ******************************************************

      [MethodImpl(AggressiveInlining)] public static bool IsDefault<T>(this T input) => EqualityComparer<T>.Default.Equals(input, default(T));

      [MethodImpl(AggressiveInlining)] public static T UnBox<T>(this T? input, T @default = default(T)) where T : struct => input ?? @default;

      [MethodImpl(AggressiveInlining)] public static bool IsNullable(this Type type) => type.IsGenericType && !type.IsGenericTypeDefinition && type.GetGenericTypeDefinition() == typeof(Nullable<>);

      [MethodImpl(AggressiveInlining)] public static bool IsNumericType(this Type oType) => (uint)(Type.GetTypeCode(oType) - 5) <= 10U;

      [MethodImpl(AggressiveInlining)] public static TypeConverter GetTypeConverter(this Type src) => TypeConversion.TypeConverterLookup.GetValueOrDefault(src);


      internal static bool IsCastableTo(this Type src, Type target) => target.IsAssignableFrom(src) || src.GetMethods(BindingFlags.Public | BindingFlags.Static).Any(x => x.ReturnType == target && (x.Name == "op_Implicit" || x.Name == "op_Explicit"));

      public static Type TryGetGenericTypeDefinition(this Type src) => src.IsGenericType ? src.GetGenericTypeDefinition() : null;



      #endregion



      #region ************************************** Reflection ******************************************************


      [MethodImpl(AggressiveInlining)]
      public static ConstructorInfo GetConstructor(this Type type, BindingFlags flags, Type[] argTypes = null) {
         return type.GetConstructor(flags, null, argTypes, null);
      }

      [MethodImpl(AggressiveInlining)]
      public static MethodInfo GetMethod(this Type type, string name, BindingFlags flags, Type[] argTypes = null) {
         return type.GetMethod(name, flags, null, argTypes, null);
      }

      [MethodImpl(AggressiveInlining)]
      public static PropertyInfo GetProperty(this Type type, string name, BindingFlags flags, Type returnType, Type[] argTypes = null) {
         return type.GetProperty(name, flags, null, returnType, argTypes, null);
      }

      #endregion


   }

}
