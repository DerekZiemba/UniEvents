using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using UniEvents.Core;
using ZMBA;

namespace UniEvents.Models.DBModels {

   public class DBLogin : DBModel {

      [DBCol("UserName", SqlDbType.VarChar, 20, false)]
      public string UserName { get; set; }

      [DBCol("APIKey", SqlDbType.VarChar, 50, false)]
      public string APIKey { get; set; }

      [DBCol("APIKeyHash", SqlDbType.Binary, 256, false)]
      public byte[] APIKeyHash { get; set; }

      [DBCol("LoginDate", SqlDbType.DateTime, 1, false)]
      public DateTime LoginDate { get; set; }

      public DBLogin() { }

      public DBLogin(SqlDataReader reader) {
         UserName = reader.GetString(nameof(UserName));
         APIKey = reader.GetString(nameof(APIKey));
         APIKeyHash = reader.GetBytes(nameof(APIKeyHash), 32);
         LoginDate = reader.GetDateTime(nameof(LoginDate));
      }

      public static async Task<DBLogin> LoginUserNameAsync(Factory ctx, string username) {
         var dbLogin = new DBLogin();
         dbLogin.UserName = username;
         (dbLogin.APIKeyHash, dbLogin.APIKey) = HashUtils.CreateAPIKey256(dbLogin.UserName);
         if(await SP_Account_LoginAsync(ctx, dbLogin).ConfigureAwait(false)) {
            return dbLogin;
         }
         return null;
      }

      private static async Task<bool> SP_Account_LoginAsync(Factory ctx, DBLogin model) {
         using (SqlConnection conn = new SqlConnection(ctx.Config.dbUniHangoutsWrite))
         using (SqlCommand cmd = new SqlCommand("[dbo].[sp_Account_Login]", conn) { CommandType = CommandType.StoredProcedure }) {
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(UserName), model.UserName);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(APIKey), model.APIKey);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.Binary, nameof(APIKeyHash), model.APIKeyHash);
            SqlParameter date = cmd.AddParam(ParameterDirection.Output, SqlDbType.DateTime, nameof(LoginDate), model.LoginDate);

            int rowsAffected = await cmd.ExecuteProcedureAsync().ConfigureAwait(false);

            model.LoginDate = (DateTime)date.Value;

            return model.LoginDate > DateTime.UtcNow.AddMinutes(-1) && model.LoginDate < DateTime.UtcNow.AddMinutes(1);
         }
      }

      public static async Task<DBLogin> SP_Account_Login_GetAsync(Factory ctx, string UserName, string APIKey) {
         if (String.IsNullOrWhiteSpace(UserName)) { throw new ArgumentNullException("UserName_Invalid"); }
         if (String.IsNullOrWhiteSpace(APIKey)) { throw new ArgumentNullException("APIKey_Invalid"); }
         
         using (SqlConnection conn = new SqlConnection(ctx.Config.dbUniHangoutsRead))
         using (SqlCommand cmd = new SqlCommand("[dbo].[sp_Account_Login_Get]", conn) { CommandType = CommandType.StoredProcedure }) {
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(UserName), UserName);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(APIKey), APIKey);

            return await cmd.ExecuteReader_GetOneAsync<DBLogin>().ConfigureAwait(false);
         }
      }

      public static IEnumerable<DBLogin> SP_Account_Login_GetAll(Factory ctx, string UserName) {
         if (String.IsNullOrWhiteSpace(UserName)) { throw new ArgumentNullException("UserName_Invalid"); }

         using (SqlConnection conn = new SqlConnection(ctx.Config.dbUniHangoutsRead))
         using (SqlCommand cmd = new SqlCommand("[dbo].[sp_Account_Logins_Get]", conn) { CommandType = CommandType.StoredProcedure }) {
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(UserName), UserName);

            foreach (var item in cmd.ExecuteReader_GetManyRecords()) { yield return new DBLogin(item); }
         }
      }

      public static async Task<bool> SP_Account_LogoutAsync(Factory ctx, string UserName, string APIKey) {
         if (String.IsNullOrWhiteSpace(UserName)) { throw new ArgumentNullException("UserName_Invalid"); }

         using (SqlConnection conn = new SqlConnection(ctx.Config.dbUniHangoutsRead))
         using (SqlCommand cmd = new SqlCommand("[dbo].[sp_Account_Logout]", conn) { CommandType = CommandType.StoredProcedure }) {
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(UserName), UserName);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(APIKey), APIKey);

            int rowsAffected = await cmd.ExecuteProcedureAsync().ConfigureAwait(false);
            return rowsAffected > 0;
         }
      }

   }

}
