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

				SqlConnection conn = TakeDbUniHangoutsConn();
				try {
					using (SqlCommand cmd = new SqlCommand("[dbo].[sp_Group_Create]", conn) { CommandType = CommandType.StoredProcedure }) {
						cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@GroupName), @GroupName);
						cmd.AddParam(ParameterDirection.Input, SqlDbType.NVarChar, nameof(@DisplayName), @DisplayName);
						cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@ContactEmail), @ContactEmail);
						cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@PhoneNumber), @PhoneNumber);
						cmd.AddParam(ParameterDirection.Input, SqlDbType.NVarChar, nameof(@Description), @Description);
						cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(@LocationID), @LocationID);

						SqlParameter @GroupID = cmd.AddParam(ParameterDirection.Output, SqlDbType.BigInt, nameof(@GroupID), null);

						int rowsAffected = await cmd.ExecuteStoredProcedureAsync().ConfigureAwait(false);

						return (long)@GroupID.Value;
					}
				} catch { throw; } finally { ReturnDbUniHangoutsConn(conn); }

			}

		}
	}
}
