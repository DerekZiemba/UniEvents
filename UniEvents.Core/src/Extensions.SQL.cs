using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace UniEvents.Core {
	public static partial class Extensions {


		public static SqlParameter AddWithValue(this SqlParameterCollection col, ParameterDirection direction, SqlDbType dbtype, string name, object value) {
			return col.Add(new SqlParameter(name[ 0 ] != '@' ? '@' + name : name, dbtype) { Direction = direction, Value = value });
		}
		public static SqlParameter AddWithValue<T>(this SqlParameterCollection col, ParameterDirection direction, SqlDbType dbtype, string name, T? value) where T : struct, IConvertible, IFormattable, IComparable {
			SqlParameter param = new SqlParameter(name[ 0 ] != '@' ? '@' + name : name, dbtype) { Direction = direction };
			if (value.HasValue) { param.Value = value; }
			return col.Add(param);
		}

		public static async Task<List<T>> ReadDataModelsAsync<T>(this SqlCommand cmd, Func<IDataReader, T> constructor, Int32 predictedRows = 4) {
			await cmd.Connection.OpenAsync().ConfigureAwait(false);
			List<T> ls = new List<T>(predictedRows);
			using (SqlDataReader reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
				while (await reader.ReadAsync().ConfigureAwait(false)) {
					ls.Add(constructor(reader));
				}
			}
			return ls;
		}


		public static bool PullBytes(this IDataRecord record, string name, out byte[] result, int size, byte[] @default = default) {
			int ord = record.GetOrdinal(name);
			if (record.IsDBNull(ord)) { result = @default; return false; }
			Byte[] buffer = new byte[size];
			record.GetBytes(ord, 0, buffer, 0, size);
			result = buffer;
			return true;
		}

		public static bool PullValue(this IDataRecord record, string name, out string result, string @default = default(string)) {
			int ord = record.GetOrdinal(name);
			if (record.IsDBNull(ord)) { result = @default; return false; }
			result = record.GetString(ord);
			return true;
		}

		public static bool PullValue(this IDataRecord record, string name, out bool result, bool @default = default(bool)) {
			int ord = record.GetOrdinal(name);
			if (record.IsDBNull(ord)) { result = @default; return false; }
			result = record.GetBoolean(ord);
			return true;
		}
		public static bool PullValue(this IDataRecord record, string name, out byte result, byte @default = default(byte)) {
			int ord = record.GetOrdinal(name);
			if (record.IsDBNull(ord)) { result = @default; return false; }
			result = record.GetByte(ord);
			return true;
		}
		public static bool PullValue(this IDataRecord record, string name, out Int16 result, Int16 @default = default(Int16)) {
			int ord = record.GetOrdinal(name);
			if (record.IsDBNull(ord)) { result = @default; return false; }
			result = record.GetInt16(ord);
			return true;
		}
		public static bool PullValue(this IDataRecord record, string name, out Int32 result, Int32 @default = default(Int32)) {
			int ord = record.GetOrdinal(name);
			if (record.IsDBNull(ord)) { result = @default; return false; }
			result = record.GetInt32(ord);
			return true;
		}
		public static bool PullValue(this IDataRecord record, string name, out Int64 result, Int64 @default = default(Int64)) {
			int ord = record.GetOrdinal(name);
			if (record.IsDBNull(ord)) { result = @default; return false; }
			result = record.GetInt64(ord);
			return true;
		}
		public static bool PullValue(this IDataRecord record, string name, out float result, float @default = default(float)) {
			int ord = record.GetOrdinal(name);
			if (record.IsDBNull(ord)) { result = @default; return false; }
			result = record.GetFloat(ord);
			return true;
		}
		public static bool PullValue(this IDataRecord record, string name, out double result, double @default = default(double)) {
			int ord = record.GetOrdinal(name);
			if (record.IsDBNull(ord)) { result = @default; return false; }
			result = record.GetDouble(ord);
			return true;
		}
		public static bool PullValue(this IDataRecord record, string name, out decimal result, decimal @default = default(decimal)) {
			int ord = record.GetOrdinal(name);
			if (record.IsDBNull(ord)) { result = @default; return false; }
			result = record.GetDecimal(ord);
			return true;
		}
		public static bool PullValue(this IDataRecord record, string name, out DateTime result, DateTime @default = default(DateTime)) {
			int ord = record.GetOrdinal(name);
			if (record.IsDBNull(ord)) { result = @default; return false; }
			result = record.GetDateTime(ord);
			return true;
		}

		public static bool PullValue(this IDataRecord record, string name, out bool? result, bool? @default = default(bool)) {
			int ord = record.GetOrdinal(name);
			if (record.IsDBNull(ord)) { result = @default; return false; }
			result = record.GetBoolean(ord);
			return true;
		}
		public static bool PullValue(this IDataRecord record, string name, out byte? result, byte? @default = default(byte)) {
			int ord = record.GetOrdinal(name);
			if (record.IsDBNull(ord)) { result = @default; return false; }
			result = record.GetByte(ord);
			return true;
		}
		public static bool PullValue(this IDataRecord record, string name, out Int16? result, Int16? @default = default(Int16)) {
			int ord = record.GetOrdinal(name);
			if (record.IsDBNull(ord)) { result = @default; return false; }
			result = record.GetInt16(ord);
			return true;
		}
		public static bool PullValue(this IDataRecord record, string name, out Int32? result, Int32? @default = default(Int32)) {
			int ord = record.GetOrdinal(name);
			if (record.IsDBNull(ord)) { result = @default; return false; }
			result = record.GetInt32(ord);
			return true;
		}
		public static bool PullValue(this IDataRecord record, string name, out Int64? result, Int64? @default = default(Int64)) {
			int ord = record.GetOrdinal(name);
			if (record.IsDBNull(ord)) { result = @default; return false; }
			result = record.GetInt64(ord);
			return true;
		}
		public static bool PullValue(this IDataRecord record, string name, out float? result, float? @default = default(float)) {
			int ord = record.GetOrdinal(name);
			if (record.IsDBNull(ord)) { result = @default; return false; }
			result = record.GetFloat(ord);
			return true;
		}
		public static bool PullValue(this IDataRecord record, string name, out double? result, double? @default = default(double)) {
			int ord = record.GetOrdinal(name);
			if (record.IsDBNull(ord)) { result = @default; return false; }
			result = record.GetDouble(ord);
			return true;
		}
		public static bool PullValue(this IDataRecord record, string name, out decimal? result, decimal? @default = default(decimal)) {
			int ord = record.GetOrdinal(name);
			if (record.IsDBNull(ord)) { result = @default; return false; }
			result = record.GetDecimal(ord);
			return true;
		}
		public static bool PullValue(this IDataRecord record, string name, out DateTime? result, DateTime? @default = null) {
			int ord = record.GetOrdinal(name);
			if (record.IsDBNull(ord)) { result = @default; return false; }
			result = record.GetDateTime(ord);
			return true;
		}


		public static bool PullValue<T>(this IDataRecord record, string name, out T result, T @default = default(T)) where T : struct, IConvertible, IFormattable, IComparable {
			System.Diagnostics.Contracts.Contract.Requires(typeof(T).IsEnum, "Must be Enum");
			int ord = record.GetOrdinal(name);
			if (record.IsDBNull(ord)) { result = @default; return false; }
			Type ftype = record.GetFieldType(ord);
			if (ftype == typeof(string)) {
				result = (T)Enum.Parse(typeof(T), record.GetString(ord), true);
				return true;
			} else {
				result = (T)record.GetValue(ord);
				return true;
			}
		}


	}

}

