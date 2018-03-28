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


      #region ************************************** JSON ******************************************************

      public static Newtonsoft.Json.JsonSerializer JsonSerializer { get; private set; } = ((Func<Newtonsoft.Json.JsonSerializer>)(
         () => {
            var ser = new Newtonsoft.Json.JsonSerializer();
            ser.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            ser.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            return ser;
         }))();

      public static JsonSerializer CompactSerializer { get; private set; } = ((Func<JsonSerializer>)(
         () => {
            var ser = new Newtonsoft.Json.JsonSerializer();
            ser.NullValueHandling = NullValueHandling.Ignore;
            ser.TypeNameHandling = TypeNameHandling.None;
            ser.MissingMemberHandling = MissingMemberHandling.Ignore;
            return ser;
         }))();


      public static string Serialize<T>(this Newtonsoft.Json.JsonSerializer ser, T value) {
         using (StringWriter sw = new StringWriter(new StringBuilder(256), (IFormatProvider)CultureInfo.InvariantCulture)) {
            using (JsonTextWriter writer = new JsonTextWriter(sw)) {
               ser.Serialize(writer, value, typeof(T));
            }
            return sw.ToString();
         }
      }

      public static byte[] SerializeToGZippedBytes<T>(this JsonSerializer ser, T value) {
         using (var mem = new MemoryStream())
         using (var zip = new GZipStream(mem, CompressionMode.Compress))
         using (var buffer = new BufferedStream(zip, 8192))
         using (var stream = new StreamWriter(buffer, Encoding.UTF8))
         using (var writer = new JsonTextWriter(stream)) {
            ser.Serialize(writer, value, typeof(T));
            writer.Flush();
            return mem.ToArray();
         }
      }

      public static T DeserializeGZippedBytes<T>(this JsonSerializer ser, byte[] bytes) {
         if(bytes is null) { return default; }
         using (var mem = new MemoryStream(bytes))
         using (var zip = new GZipStream(mem, CompressionMode.Decompress))
         using (var stream = new StreamReader(zip, Encoding.UTF8))
         using (var reader = new JsonTextReader(stream)) {
            return ser.Deserialize<T>(reader);
         }
      }


      public static T Deserialize<T>(this JsonSerializer ser, string value) {
         using (JsonTextReader reader = new JsonTextReader(new StringReader(value))) {
            return ser.Deserialize<T>(reader);
         }
      }

      public static T DeserializeOrDefault<T>(this JsonSerializer ser, string value, T @default = default(T)) {
         if (value.IsNotWhitespace()) {
            try {
               return ser.Deserialize<T>(value);
#pragma warning disable CS0168 // Variable is declared but never used
            } catch (Exception ex) { }
#pragma warning restore CS0168 // Variable is declared but never used
         }
         return @default;
      }


      #endregion



      #region ************************************** String ******************************************************
      private static readonly CompareInfo InvCmpInfo =CultureInfo.InvariantCulture.CompareInfo;
      private static readonly CompareOptions VbCmp = CmpOp.IgnoreWidth | CmpOp.IgnoreNonSpace | CmpOp.IgnoreKanaType; //Compare like VisualBasic


      [Flags]
      public enum SubstrOptions {
         Default = 0,
         /// <summary> Whether the sequence is included in the returned substring </summary>
         IncludeSeq = 1 << 0,
         /// <summary> OrdinalIgnoreCase </summary>
         IgnoreCase = 1 << 1,
         /// <summary> If operation fails, return the original input string. </summary>
         RetInput = 1 << 2
      }

      [MethodImpl(AggressiveInlining)] public static bool IsNullOrEmpty(this string str) => String.IsNullOrEmpty(str);

      [MethodImpl(AggressiveInlining)] public static bool IsNotWhitespace(this string str) => !String.IsNullOrWhiteSpace(str);

		[MethodImpl(AggressiveInlining)] public static bool Eq(this string str, string other) => String.Equals(str, other, StringComparison.Ordinal);

		[MethodImpl(AggressiveInlining)] public static bool EqIgCase(this string str, string other) => String.Equals(str, other, StringComparison.OrdinalIgnoreCase);

      [MethodImpl(AggressiveInlining)] public static bool EqAlphaNum(this string str, string other) => 0 == InvCmpInfo.Compare(str, other, VbCmp | CmpOp.IgnoreSymbols);

      [MethodImpl(AggressiveInlining)] public static bool EqAlphaNumIgCase(this string str, string other) => 0 == InvCmpInfo.Compare(str, other, VbCmp | CmpOp.IgnoreSymbols | CmpOp.IgnoreCase);


      public static string ToAlphaNumeric(this string str) {
         if (str.IsNullOrEmpty()) { return str; }
         var sb = StringBuilderCache.Take(str.Length);
         for (var i = 0; i < str.Length; i++) { if (Char.IsLetter(str[i]) || char.IsNumber(str[i])) { sb.Append(str[i]); } }
         return StringBuilderCache.Release(sb);
      }
      public static string ToAlphaNumericLower(this string str) {
         if (str.IsNullOrEmpty()) { return str; }
         var sb = StringBuilderCache.Take(str.Length);
         for (var i = 0; i < str.Length; i++) { if (Char.IsLetter(str[i]) || char.IsNumber(str[i])) { sb.Append(char.ToLower(str[i])); } }
         return StringBuilderCache.Release(sb);
      }

      public static string ReplaceIgCase(this string sInput, string oldValue, string newValue) {
			if (!string.IsNullOrEmpty(sInput) && !string.IsNullOrEmpty(oldValue)) {
				int idxLeft = sInput.IndexOf(oldValue, 0, StringComparison.OrdinalIgnoreCase);
				//Don't build a new string if it doesn't even contain the value
				if (idxLeft >= 0) {
					if (newValue == null)
						newValue = string.Empty;
					var sb = StringBuilderCache.Take(sInput.Length + Math.Max(0, newValue.Length - oldValue.Length) + 16);
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
					return StringBuilderCache.Release(sb);
				}
			}
			return sInput;
		}

      public static string ToStringJoin(this IEnumerable<string> ienum, string separator = ", ") {
         return String.Join(separator, from string x in ienum where !string.IsNullOrWhiteSpace(x) select x.Trim());
      }

      public static string SubstrBefore(this string input, string seq, SubstrOptions opts = SubstrOptions.Default) {
         if (input?.Length > 0 && seq?.Length > 0) {
            int index = input.IndexOf(seq, (opts & SubstrOptions.IgnoreCase) > 0 ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
            if (index >= 0) {
               if ((opts & SubstrOptions.IncludeSeq) > 0) { index += seq.Length; }
               return input.Substring(0, index);
            }
         }
         return (opts & SubstrOptions.RetInput) > 0 ? input : null;
      }
      public static string SubstrBeforeLast(this string input, string seq, SubstrOptions opts = SubstrOptions.Default) {
         if (input?.Length > 0 && seq?.Length > 0) {
            int index = input.LastIndexOf(seq, (opts & SubstrOptions.IgnoreCase) > 0 ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
            if (index >= 0) {
               if ((opts & SubstrOptions.IncludeSeq) > 0) { index += seq.Length; }
               return input.Substring(0, index);
            }
         }
         return (opts & SubstrOptions.RetInput) > 0 ? input : null;
      }
      public static string SubstrAfter(this string input, string seq, SubstrOptions opts = SubstrOptions.Default) {
         if (input?.Length > 0 && seq?.Length > 0) {
            int index = input.IndexOf(seq, (opts & SubstrOptions.IgnoreCase) > 0 ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
            if (index >= 0) {
               if ((opts & SubstrOptions.IncludeSeq) == 0) { index += seq.Length; }
               return input.Substring(index);
            }
         }
         return (opts & SubstrOptions.RetInput) > 0 ? input : null;
      }
      public static string SubstrAfterLast(this string input, string seq, SubstrOptions opts = SubstrOptions.Default) {
         if (input?.Length > 0 && seq?.Length > 0) {
            int index = input.LastIndexOf(seq, (opts & SubstrOptions.IgnoreCase) > 0 ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
            if (index >= 0) {
               if ((opts & SubstrOptions.IncludeSeq) == 0) { index += seq.Length; }
               return input.Substring(index);
            }
         }
         return (opts & SubstrOptions.RetInput) > 0 ? input : null;
      }


      public static string SubstrBefore(this string input, string[] sequences, SubstrOptions opts = SubstrOptions.Default) {
         if (input?.Length > 0 && sequences?.Length > 0) {
            int idx = input.Length;
            for (int i = 0; i < sequences.Length; i++) {
               string seq = sequences[i];
               if (seq?.Length > 0) {
                  int pos = input.IndexOf(seq, 0, idx, (opts & SubstrOptions.IgnoreCase) > 0 ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
                  if (pos >= 0 && pos <= idx) {
                     if ((opts & SubstrOptions.IncludeSeq) > 0) { pos += seq.Length; }
                     idx = pos;
                  }
               }
            }
            return input.Substring(0, idx);
         }
         return (opts & SubstrOptions.RetInput) > 0 ? input : null;
      }
      public static string SubstrBeforeLast(this string input, string[] sequences, SubstrOptions opts = SubstrOptions.Default) {
         if (input?.Length > 0 && sequences?.Length > 0) {
            int idx = input.Length;
            for (int i = 0; i < sequences.Length; i++) {
               string seq = sequences[i];
               if (seq?.Length > 0) {
                  int pos = input.LastIndexOf(seq, idx, idx, (opts & SubstrOptions.IgnoreCase) > 0 ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
                  if (pos >= 0 && pos <= idx) {
                     if ((opts & SubstrOptions.IncludeSeq) > 0) { pos += seq.Length; }
                     idx = pos;
                  }
               }
            }
            return input.Substring(0, idx);
         }
         return (opts & SubstrOptions.RetInput) > 0 ? input : null;
      }
      public static string SubstrAfter(this string input, string[] sequences, SubstrOptions opts = SubstrOptions.Default) {
         if (input?.Length > 0 && sequences?.Length > 0) {
            int idx = 0;
            for (int i = 0; i < sequences.Length; i++) {
               string seq = sequences[i];
               if (seq?.Length > 0) {
                  int pos = input.IndexOf(seq, idx, input.Length - idx, (opts & SubstrOptions.IgnoreCase) > 0 ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
                  if (pos >= idx && pos <= input.Length) {
                     if ((opts & SubstrOptions.IncludeSeq) == 0) { pos += seq.Length; }
                     idx = pos;
                  }
               }
            }
            return input.Substring(idx);
         }
         return (opts & SubstrOptions.RetInput) > 0 ? input : null;
      }
      public static string SubstrAfterLast(this string input, string[] sequences, SubstrOptions opts = SubstrOptions.Default) {
         if (input?.Length > 0 && sequences?.Length > 0) {
            int idx = 0;
            for (int i = 0; i < sequences.Length; i++) {
               string seq = sequences[i];
               if (seq?.Length > 0) {
                  int pos = input.LastIndexOf(seq, idx, idx, (opts & SubstrOptions.IgnoreCase) > 0 ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
                  if (pos >= idx && pos <= input.Length) {
                     if ((opts & SubstrOptions.IncludeSeq) == 0) { pos += seq.Length; }
                     idx = pos;
                  }
               }
            }
            return input.Substring(idx);
         }
         return (opts & SubstrOptions.RetInput) > 0 ? input : null;
      }


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

      //public static TTo CopyFieldsShallow<TTo, TFrom>(TTo target, TFrom src) where TTo : class {
      //	var shallowcopy = RuntimeCompiler.CompileShallowFieldCopier<TTo, TFrom>();
      //	shallowcopy(target, src);
      //	return target;
      //}

      //public static TTo CopyPropsShallow<TTo, TFrom>(TTo target, TFrom src) where TTo : class {
      //	var shallowcopy = RuntimeCompiler.CompileShallowPropertyCopier<TTo, TFrom>();
      //	shallowcopy(target, src);
      //	return target;
      //}

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



      #region ************************************** SQL ******************************************************

      public static SqlParameter AddParam(this SqlCommand cmd, ParameterDirection direction, SqlDbType dbtype, string name, object value) {
			return cmd.Parameters.Add(new SqlParameter(name[0] != '@' ? '@' + name : name, dbtype) { Direction = direction, Value = value });
		}
      public static SqlParameter AddParam(this SqlCommand cmd, ParameterDirection direction, SqlDbType dbtype, string name, string value) {
         SqlParameter param = new SqlParameter(name[ 0 ] != '@' ? '@' + name : name, dbtype) { Direction = direction };
         if (value.IsNotWhitespace()) { param.Value = value; }
         return cmd.Parameters.Add(param);
      }
      public static SqlParameter AddParam<T>(this SqlCommand cmd, ParameterDirection direction, SqlDbType dbtype, string name, T? value) where T : struct, IConvertible, IFormattable, IComparable {
			SqlParameter param = new SqlParameter(name[ 0 ] != '@' ? '@' + name : name, dbtype) { Direction = direction };
			if (value.HasValue) { param.Value = value; }
			return cmd.Parameters.Add(param);
		}

      public static int ExecuteProcedure(this SqlCommand cmd) {
         if (cmd.Connection.State != ConnectionState.Open) { cmd.Connection.Open(); }
         return cmd.ExecuteNonQuery();
      }
      public static async Task<int> ExecuteProcedureAsync(this SqlCommand cmd) {
         if (cmd.Connection.State != ConnectionState.Open) { await cmd.Connection.OpenAsync().ConfigureAwait(false); }
         return await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
      }


      public static T ExecuteReader_GetOne<T>(this SqlCommand cmd) {
         if (cmd.Connection.State != ConnectionState.Open) { cmd.Connection.Open(); }
         using (SqlDataReader reader = cmd.ExecuteReader()) {
            if (reader.Read()) {
               return RuntimeCompiler.GetConstructor<Func<IDataReader, T>>()(reader);
            }
         }
         return default;
      }

      public static async Task<T> ExecuteReader_GetOneAsync<T>(this SqlCommand cmd) {
         if (cmd.Connection.State != ConnectionState.Open) { await cmd.Connection.OpenAsync().ConfigureAwait(false); }
         using (SqlDataReader reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
            if (await reader.ReadAsync().ConfigureAwait(false)) {
               return RuntimeCompiler.GetConstructor<Func<IDataReader, T>>()(reader);
            }
         }
         return default;
      }

      public static IEnumerable<IDataReader> ExecuteReader_GetManyRecords(this SqlCommand cmd) {
         if (cmd.Connection.State != ConnectionState.Open) { cmd.Connection.Open(); }
         using (SqlDataReader reader = cmd.ExecuteReader()) {
            while (reader.Read()) {
               yield return reader;
            }
         }
      }

      public static IEnumerable<T> ExecuteReader_GetMany<T>(this SqlCommand cmd) {
         Func<IDataReader, T> ctor = RuntimeCompiler.GetConstructor<Func<IDataReader, T>>();

         if (cmd.Connection.State != ConnectionState.Open) { cmd.Connection.Open(); }
         
         using (SqlDataReader reader = cmd.ExecuteReader()) {
            while (reader.Read()) {
               yield return ctor(reader);
            }
         }
      }

      public static async Task<List<T>> ExecuteReader_GetManyAsync<T>(this SqlCommand cmd) {
         Func<IDataReader, T> ctor = RuntimeCompiler.GetConstructor<Func<IDataReader, T>>();

         if (cmd.Connection.State != ConnectionState.Open) { await cmd.Connection.OpenAsync().ConfigureAwait(false); }
         
         var ls = new List<T>();
         using (SqlDataReader reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
            while (await reader.ReadAsync().ConfigureAwait(false)) {
               ls.Add(ctor(reader));
            }
         }
         return ls;
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
