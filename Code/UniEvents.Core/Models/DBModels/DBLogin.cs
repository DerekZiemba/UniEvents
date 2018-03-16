﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
         UserName = reader.GetString(nameof(UserName));
         APIKey = reader.GetString(nameof(APIKey));
         APIKeyHash = reader.GetBytes(nameof(APIKeyHash), 32);
         LoginDate = reader.GetDateTime(nameof(LoginDate));
      }

      public static async Task<bool> SP_Account_LoginAsync(CoreContext ctx, DBLogin model) {
         if (model.UserName.IsNullOrWhitespace()) { throw new ArgumentNullException("UserName_Invalid"); }
         if (model.APIKey.IsNullOrWhitespace()) { throw new ArgumentNullException("APIKey_Invalid"); }
         if (model.APIKeyHash.IsEmpty()) { throw new ArgumentNullException("APIKeyHash_Invalid"); }

         using (SqlConnection conn = new SqlConnection(ctx.Config.dbUniHangoutsWrite))
         using (SqlCommand cmd = new SqlCommand("[dbo].[sp_Account_Login]", conn) { CommandType = CommandType.StoredProcedure }) {
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(UserName), model.UserName);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(APIKey), model.APIKey);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.Binary, nameof(APIKeyHash), model.APIKeyHash);
            SqlParameter date = cmd.AddParam(ParameterDirection.Output, SqlDbType.DateTime, nameof(LoginDate), model.LoginDate);

            if (cmd.Connection.State != ConnectionState.Open) { await cmd.Connection.OpenAsync().ConfigureAwait(false); }
            int rowsAffected = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);

            model.LoginDate = (DateTime)date.Value;

            return model.LoginDate > DateTime.UtcNow.AddMinutes(-1) && model.LoginDate < DateTime.UtcNow.AddMinutes(1);
         }
      }

      public static async Task<DBLogin> SP_Account_Login_GetAsync(CoreContext ctx, string UserName, string APIKey) {
         if (UserName.IsNullOrWhitespace()) { throw new ArgumentNullException("UserName_Invalid"); }
         if (APIKey.IsNullOrWhitespace()) { throw new ArgumentNullException("APIKey_Invalid"); }
         
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