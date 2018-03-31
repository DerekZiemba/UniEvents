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


   }

}
