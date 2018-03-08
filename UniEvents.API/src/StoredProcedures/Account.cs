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

		internal static class Account {


			internal static async Task<long> CreateAsync(string UserName,
																		string Password = null, 
																		byte[] PasswordHash = null,
																		string DisplayName = null,
																		string FirstName = null,
																		string LastName = null,
																		string ContactEmail = null,
																		string PhoneNumber = null,
																		string Description = null,
																		long? LocationID = null) {

				Contract.Requires<ArgumentNullException>(!UserName.IsNullOrWhitespace(), "UserName cannot be empty");
				if (!PasswordHash.IsEmpty()) { Password = null; }

				SqlConnection conn = TakeDbUniHangoutsConn();
				try {
					using (SqlCommand cmd = new SqlCommand("[dbo].[sp_Account_Create]", conn) { CommandType = CommandType.StoredProcedure }) {
						cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(UserName), UserName);
						cmd.AddParam(ParameterDirection.Input, SqlDbType.NVarChar, nameof(Password), Password);
						cmd.AddParam(ParameterDirection.Input, SqlDbType.Binary, nameof(PasswordHash), PasswordHash);
						cmd.AddParam(ParameterDirection.Input, SqlDbType.NVarChar, nameof(DisplayName), DisplayName);
						cmd.AddParam(ParameterDirection.Input, SqlDbType.NVarChar, nameof(FirstName), FirstName);
						cmd.AddParam(ParameterDirection.Input, SqlDbType.NVarChar, nameof(LastName), LastName);
						cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(ContactEmail), ContactEmail);
						cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(PhoneNumber), PhoneNumber);
						cmd.AddParam(ParameterDirection.Input, SqlDbType.NVarChar, nameof(Description), Description);
						cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(LocationID), LocationID);

						SqlParameter AccountID = cmd.AddParam(ParameterDirection.Output, SqlDbType.BigInt, nameof(AccountID), null);

						int rowsAffected = await cmd.ExecuteStoredProcedureAsync().ConfigureAwait(false);

						return (long)AccountID.Value;
					}
				} catch { throw; } finally { ReturnDbUniHangoutsConn(conn); }

			}



		}
	}
}
