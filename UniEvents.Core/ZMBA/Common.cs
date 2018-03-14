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
using System.Runtime.CompilerServices;
using System.Reflection;

using CmpOp = System.Globalization.CompareOptions;
using static System.Runtime.CompilerServices.MethodImplOptions;


namespace ZMBA {

	public static partial class Common {

		#region ************************************** Math ******************************************************

		//public static bool IsBetween(this char value, char min, char max) => value > min && value < max;
		//public static bool IsBetween(this SByte value, SByte min, SByte max) => value > min && value < max;
		//public static bool IsBetween(this Int16 value, Int16 min, Int16 max) => value > min && value < max;
		//public static bool IsBetween(this Int32 value, Int32 min, Int32 max) => value > min && value < max;
		//public static bool IsBetween(this Int64 value, Int64 min, Int64 max) => value > min && value < max;
		//public static bool IsBetween(this Byte value, Byte min, Byte max) => value > min && value < max;
		//public static bool IsBetween(this UInt16 value, UInt16 min, UInt16 max) => value > min && value < max;
		//public static bool IsBetween(this UInt32 value, UInt32 min, UInt32 max) => value > min && value < max;
		//public static bool IsBetween(this UInt64 value, UInt64 min, UInt64 max) => value > min && value < max;
		//public static bool IsBetween(this Single value, Single min, Single max) => value > min && value < max;
		//public static bool IsBetween(this Double value, Double min, Double max) => value > min && value < max;
		//public static bool IsBetween(this Decimal value, Decimal min, Decimal max) => value > min && value < max;
		//public static bool IsBetween(this DateTime value, DateTime min, DateTime max) => value > min && value < max;


		#endregion



		#region ************************************** String ******************************************************

		[MethodImpl(AggressiveInlining)] public static bool IsNullOrWhitespace(this string str) => String.IsNullOrWhiteSpace(str);

		[MethodImpl(AggressiveInlining)] public static bool IsEmpty(this string str) => String.IsNullOrEmpty(str);

		[MethodImpl(AggressiveInlining)] public static bool Eq(this string str, string other) => String.Equals(str, other, StringComparison.Ordinal);

		[MethodImpl(AggressiveInlining)] public static bool EqIgCase(this string str, string other) => String.Equals(str, other, StringComparison.OrdinalIgnoreCase);

		[MethodImpl(AggressiveInlining)] public static bool EqIgCaseSym(this string str, string other) => 0 == CultureInfo.InvariantCulture.CompareInfo.Compare(str, other, CmpOp.IgnoreWidth | CmpOp.IgnoreNonSpace | CmpOp.IgnoreKanaType | CmpOp.IgnoreSymbols | CmpOp.IgnoreCase);

		[MethodImpl(AggressiveInlining)] public static bool EqIgSym(this string str, string other) => 0 == CultureInfo.InvariantCulture.CompareInfo.Compare(str, other, CmpOp.IgnoreWidth | CmpOp.IgnoreNonSpace | CmpOp.IgnoreKanaType | CmpOp.IgnoreSymbols);


		public static string ReplaceIgCase(this string sInput, string oldValue, string newValue) {
			if (!string.IsNullOrEmpty(sInput) && !string.IsNullOrEmpty(oldValue)) {
				int idxLeft = sInput.IndexOf(oldValue, 0, StringComparison.OrdinalIgnoreCase);
				//Don't build a new string if it doesn't even contain the value
				if (idxLeft >= 0) {
					if (newValue == null)
						newValue = string.Empty;
					var sb = new StringBuilder(sInput.Length + Math.Max(0, newValue.Length - oldValue.Length) + 16);
					int pos = 0;
					while (pos < sInput.Length) {
						if (idxLeft == -1) {
							sb.Append(sInput.Substring(pos));
							break;
						} else {
							sb.Append(sInput.Substring(pos, idxLeft - pos));
							sb.Append(newValue);
							pos = idxLeft + oldValue.Length + 1;
						}
						if (pos < sInput.Length) {
							idxLeft = sInput.IndexOf(oldValue, pos, StringComparison.OrdinalIgnoreCase);
						}
					}
					return sb.ToString();
				}
			}
			return sInput;
		}

		public static string ToStringJoin(this IEnumerable<string> ienum, string separator = ", ") => String.Join(separator, from string x in ienum where !string.IsNullOrWhiteSpace(x) select x.Trim());

		#endregion



		#region ************************************** REGEX ******************************************************

		public static IEnumerable<Match> GetMatches(this Regex rgx, string input) {
			foreach (Match match in rgx.Matches(input)) {
				if (match.Index >= 0 && match.Length > 0) {
					yield return match;
				}
			}
		}

		public static IEnumerable<string> GetMatchedValues(this Regex rgx, string input) {
			foreach (Match match in rgx.GetMatches(input)) {
				yield return match.Value;
			}
		}

		public static IEnumerable<Group> FindNamedGroups(this Regex rgx, string input, string name) {
			foreach (Match match in rgx.GetMatches(input)) {
				if (match.Index >= 0 && match.Length > 0) {
					Group group = match.Groups[name];
					if (group.Index >= 0 && group.Length > 0) {
						yield return group;
					}
				}
			}
		}

		public static IEnumerable<string> FindNamedGroupValues(this Regex rgx, string input, string name) {
			foreach (Group group in rgx.FindNamedGroups(input, name)) {
				yield return group.Value;
			}
		}

		[MethodImpl(AggressiveInlining)]
		public static string FindGroupValue(this Match match, string name) {
			Group group = match.Groups[name];
			return group.Index >= 0 && group.Length > 0 ? group.Value : null;
		}


		public static string ReplaceNamedGroup(this Regex rgx, string sInput, string groupName, string newValue) {
			if (!string.IsNullOrWhiteSpace(sInput)) {
				StringBuilder sb = new StringBuilder(sInput.Length + newValue.Length);
				int idx = 0;
				foreach (Group group in rgx.FindNamedGroups(sInput, groupName)) {
					if (group.Index >= idx) {
						sb.Append(sInput.Substring(idx, group.Index - idx));
						sb.Append(newValue);
						idx = group.Index + group.Length;
					}
				}
				if (idx < sInput.Length) {
					sb.Append(sInput.Substring(idx));
				}
				return sb.ToString();
			}
			return sInput;
		}


		#endregion



		#region ************************************** Collections ******************************************************

		[MethodImpl(AggressiveInlining)] public static bool IsEmpty<T>(this T[] arr) => arr is null || arr.Length == 0;
		[MethodImpl(AggressiveInlining)] public static bool IsEmpty<T>(this ICollection<T> arr) => arr is null || arr.Count == 0;

		[MethodImpl(AggressiveInlining)]
		public static TValue GetItemOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue @default = default) {
			TValue value;
			return dict.TryGetValue(key, out value) ? value : @default;
		}

		#endregion



		#region ************************************** Type ******************************************************

		[MethodImpl(AggressiveInlining)] public static bool IsDefault<T>(this T input) => EqualityComparer<T>.Default.Equals(input, default(T));

		[MethodImpl(AggressiveInlining)] public static T UnBox<T>(this T? input, T @default = default(T)) where T : struct => input ?? @default;

		[MethodImpl(AggressiveInlining)] public static bool IsNullable(this Type type) => type.IsGenericType && !type.IsGenericTypeDefinition && type.GetGenericTypeDefinition() == typeof(Nullable<>);

		[MethodImpl(AggressiveInlining)] public static bool IsNumericType(this Type oType) => (uint)(Type.GetTypeCode(oType) - 5) <= 10U;

		[MethodImpl(AggressiveInlining)] public static TypeConverter GetTypeConverter(this Type src) => TypeConversion.TypeConverterLookup.GetItemOrDefault(src);


		internal static bool IsCastableTo(this Type src, Type target) => target.IsAssignableFrom(src) || src.GetMethods(BindingFlags.Public | BindingFlags.Static).Any(x => x.ReturnType == target && (x.Name == "op_Implicit" || x.Name == "op_Explicit"));


		#endregion



		#region ************************************** Reflection ******************************************************

		public static TTo CopyFieldsShallow<TTo, TFrom>(TTo target, TFrom src) where TTo : class {
			var shallowcopy = RuntimeCompiler.CompileShallowFieldCopier<TTo, TFrom>();
			shallowcopy(target, src);
			return target;
		}

		public static TTo CopyPropsShallow<TTo, TFrom>(TTo target, TFrom src) where TTo : class {
			var shallowcopy = RuntimeCompiler.CompileShallowPropertyCopier<TTo, TFrom>();
			shallowcopy(target, src);
			return target;
		}

		#endregion



		#region ************************************** SQL ******************************************************

		public static SqlParameter AddParam(this SqlCommand cmd, ParameterDirection direction, SqlDbType dbtype, string name, object value) {
			return cmd.Parameters.Add(new SqlParameter(name[0] != '@' ? '@' + name : name, dbtype) { Direction = direction, Value = value });
		}
		public static SqlParameter AddParam<T>(this SqlCommand cmd, ParameterDirection direction, SqlDbType dbtype, string name, T? value) where T : struct, IConvertible, IFormattable, IComparable {
			SqlParameter param = new SqlParameter(name[ 0 ] != '@' ? '@' + name : name, dbtype) { Direction = direction };
			if (value.HasValue) { param.Value = value; }
			return cmd.Parameters.Add(param);
		}

		public static byte[] GetBytes(this IDataRecord record, string name, int size) {
			int ord = record.GetOrdinal(name);
			if (record.IsDBNull(ord)) { return null; }
			Byte[] buffer = new byte[size];
			record.GetBytes(ord, 0, buffer, 0, size);
			return buffer;
		}

		public static string GetString(this IDataRecord record, string name) {
			int ord = record.GetOrdinal(name);
			return record.IsDBNull(ord) ? null : record.GetString(ord);
		}

		public static bool GetBoolean(this IDataRecord record, string name) {
			int ord = record.GetOrdinal(name);
			return record.IsDBNull(ord) ? default : record.GetBoolean(ord);
		}
		public static byte GetByte(this IDataRecord record, string name) {
			int ord = record.GetOrdinal(name);
			return record.IsDBNull(ord) ? default : record.GetByte(ord);
		}
		public static Int16 GetInt16(this IDataRecord record, string name) {
			int ord = record.GetOrdinal(name);
			return record.IsDBNull(ord) ? default : record.GetInt16(ord);
		}
		public static Int32 GetInt32(this IDataRecord record, string name) {
			int ord = record.GetOrdinal(name);
			return record.IsDBNull(ord) ? default : record.GetInt32(ord);
		}
		public static Int64 GetInt64(this IDataRecord record, string name) {
			int ord = record.GetOrdinal(name);
			return record.IsDBNull(ord) ? default : record.GetInt64(ord);
		}
		public static float GetFloat32(this IDataRecord record, string name) {
			int ord = record.GetOrdinal(name);
			return record.IsDBNull(ord) ? default : record.GetFloat(ord);
		}
		public static double GetFloat64(this IDataRecord record, string name) {
			int ord = record.GetOrdinal(name);
			return record.IsDBNull(ord) ? default : record.GetDouble(ord);
		}
		public static decimal GetDecimal(this IDataRecord record, string name) {
			int ord = record.GetOrdinal(name);
			return record.IsDBNull(ord) ? default : record.GetDecimal(ord);
		}
		public static DateTime GetDateTime(this IDataRecord record, string name) {
			int ord = record.GetOrdinal(name);
			return record.IsDBNull(ord) ? default : record.GetDateTime(ord);
		}


		public static bool? GetNBoolean(this IDataRecord record, string name) {
			int ord = record.GetOrdinal(name);
			return record.IsDBNull(ord) ? null : (bool?)record.GetBoolean(ord);
		}
		public static byte? GetNByte(this IDataRecord record, string name) {
			int ord = record.GetOrdinal(name);
			return record.IsDBNull(ord) ? null : (byte?)record.GetByte(ord);
		}
		public static Int16? GetNInt16(this IDataRecord record, string name) {
			int ord = record.GetOrdinal(name);
			return record.IsDBNull(ord) ? null : (Int16?)record.GetInt16(ord);
		}
		public static Int32? GetNInt32(this IDataRecord record, string name) {
			int ord = record.GetOrdinal(name);
			return record.IsDBNull(ord) ? null : (Int32?)record.GetInt32(ord);
		}
		public static Int64? GetNInt64(this IDataRecord record, string name) {
			int ord = record.GetOrdinal(name);
			return record.IsDBNull(ord) ? null : (Int64?)record.GetInt64(ord);
		}
		public static float? GetNFloat32(this IDataRecord record, string name) {
			int ord = record.GetOrdinal(name);
			return record.IsDBNull(ord) ? null : (float?)record.GetFloat(ord);
		}
		public static double? GetNFloat64(this IDataRecord record, string name) {
			int ord = record.GetOrdinal(name);
			return record.IsDBNull(ord) ? null : (double?)record.GetDouble(ord);
		}
		public static decimal? GetNDecimal(this IDataRecord record, string name) {
			int ord = record.GetOrdinal(name);
			return record.IsDBNull(ord) ? null : (decimal?)record.GetDecimal(ord);
		}
		public static DateTime? GetNDateTime(this IDataRecord record, string name) {
			int ord = record.GetOrdinal(name);
			return record.IsDBNull(ord) ? null : (DateTime?)record.GetDateTime(ord);
		}



		#endregion



	}

}
