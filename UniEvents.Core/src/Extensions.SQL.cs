using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace UniEvents.Core {
	public static partial class Extensions {


		public static SqlParameter AddWithValue(this SqlParameterCollection col, ParameterDirection direction, DbType dbtype, string name, object value) {
			return col.Add(new SqlParameter(name[ 0 ] != '@' ? '@' + name : name, dbtype) { Direction = direction, Value = value });
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


	}

}

