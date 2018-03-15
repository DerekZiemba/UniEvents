using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Text;
using System.Threading.Tasks;
using UniEvents.Core;
using ZMBA;

namespace UniEvents.Models.DBModels {

   public class DBLogin {

      [DBCol("UserName", SqlDbType.VarChar, 20, false)]
      public string UserName { get; set; }

      [DBCol("APIKey", SqlDbType.VarChar, 50, false)]
      public string APIKey { get; set; }

      [DBCol("APIKeyHash", SqlDbType.Binary, 256, false)]
      public byte[] APIKeyHash { get; set; }

      [DBCol("LoginDate", SqlDbType.DateTime, 1, false)]
      public DateTime LoginDate { get; set; }

      public DBLogin() { }

      public DBLogin(IDataReader reader) {
         DBLogin model = new DBLogin();
         model.UserName = reader.GetString(nameof(UserName));
         model.APIKey = reader.GetString(nameof(APIKey));
         model.APIKeyHash = reader.GetBytes(nameof(APIKey), 32);
         model.LoginDate = reader.GetDateTime(nameof(LoginDate));
      }

      public static async Task<bool> SP_Account_LoginAsync(CoreContext ctx, DBLogin model) {
         Contract.Requires<ArgumentNullException>(!model.UserName.IsNullOrWhitespace(), "UserName_Invalid");
         Contract.Requires<ArgumentNullException>(!model.APIKey.IsEmpty(), "APIKey_Invalid");
         Contract.Requires<ArgumentNullException>(!model.APIKeyHash.IsEmpty(), "APIKeyHash_Invalid");

         using (SqlConnection conn = new SqlConnection(ctx.Config.dbUniHangoutsWrite))
         using (SqlCommand cmd = new SqlCommand("[dbo].[sp_Account_Login]", conn) { CommandType = CommandType.StoredProcedure }) {
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(UserName), model.UserName);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(APIKey), model.APIKey);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.Binary, nameof(APIKeyHash), model.APIKeyHash);
            SqlParameter date = cmd.AddParam(ParameterDirection.Output, SqlDbType.DateTime, nameof(LoginDate), model.LoginDate);

            if (cmd.Connection.State != ConnectionState.Open) { await cmd.Connection.OpenAsync().ConfigureAwait(false); }
            int rowsAffected = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);

            model.LoginDate = (DateTime)date.Value;

            return rowsAffected == 1;
         }
      }

      public static async Task<DBLogin> SP_Account_Login_GetAsync(CoreContext ctx, string UserName, string APIKey) {
         Contract.Requires<ArgumentNullException>(!UserName.IsNullOrWhitespace(), "UserName_Invalid");
         Contract.Requires<ArgumentNullException>(!APIKey.IsEmpty(), "APIKey_Invalid");

         using (SqlConnection conn = new SqlConnection(ctx.Config.dbUniHangoutsRead))
         using (SqlCommand cmd = new SqlCommand("[dbo].[sp_Account_Login_Get]", conn) { CommandType = CommandType.StoredProcedure }) {
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(UserName), UserName);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(APIKey), APIKey);

            if (cmd.Connection.State != ConnectionState.Open) { await cmd.Connection.OpenAsync().ConfigureAwait(false); }
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
               while (await reader.ReadAsync().ConfigureAwait(false)) {
                  return new DBLogin(reader);
               }
            }
            return null;
         }
      }


   }

}
