using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniEvents.Core;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;


using UniEvents.Core.DBModels;
using static UniEvents.Core.Extensions;


namespace UniEvents.WebAPI {

	internal static partial class StoredProcedures {

		internal static class Group {


			internal static async Task<long> CreateAsync(long @GroupOwnerAccountID,
																		string @GroupName, 
																		string @DisplayName = null,
																		string @ContactEmail = null,
																		string @PhoneNumber = null,
																		string @Description = null,
																		long? @LocationID = null) {

				Contract.Requires<ArgumentNullException>(@GroupOwnerAccountID > 0, "GroupOwnerAccountID must be valid.");
				Contract.Requires<ArgumentNullException>(!@GroupName.IsNullOrWhitespace(), "@GroupName cannot be empty.");

				using (SqlConnection conn = new SqlConnection(Settings.SqlDbUniHangoutsConnStr))
				using (SqlCommand cmd = new SqlCommand("[dbo].[sp_Group_Create]", conn) { CommandType = CommandType.StoredProcedure }) {
					cmd.Parameters.AddWithValue(ParameterDirection.Input, SqlDbType.VarChar, nameof(@GroupName), @GroupName);
					cmd.Parameters.AddWithValue(ParameterDirection.Input, SqlDbType.NVarChar, nameof(@DisplayName), @DisplayName);
					cmd.Parameters.AddWithValue(ParameterDirection.Input, SqlDbType.VarChar, nameof(@ContactEmail), @ContactEmail);
					cmd.Parameters.AddWithValue(ParameterDirection.Input, SqlDbType.VarChar, nameof(@PhoneNumber), @PhoneNumber);
					cmd.Parameters.AddWithValue(ParameterDirection.Input, SqlDbType.NVarChar, nameof(@Description), @Description);
					cmd.Parameters.AddWithValue(ParameterDirection.Input, SqlDbType.BigInt, nameof(@LocationID), @LocationID);

					SqlParameter @GroupID = cmd.Parameters.AddWithValue(ParameterDirection.Output, SqlDbType.BigInt, nameof(@GroupID), null);

					await cmd.Connection.OpenAsync().ConfigureAwait(false);
					int rowsAffected = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);

					return (long)@GroupID.Value;
				}
			}



		}
	}
}
