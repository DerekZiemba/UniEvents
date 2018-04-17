using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


using ZMBA;
using UniEvents.Core;
using UniEvents.Models.ApiModels;
using UniEvents.Models.DBModels;


namespace UniEvents.Core.Managers {

   public class AccountManager {
      private readonly Factory Ctx;

      internal AccountManager(Factory ctx) {
         this.Ctx = ctx;
      }

      public async Task<bool> CheckPrivilege(string username, string apikey) {
         if (username.IsNotWhitespace() && apikey.IsNotWhitespace()) {
            try {
               DBLogin dbLogin = await DBLogin.SP_Account_Login_GetAsync(Ctx, username, apikey).ConfigureAwait(false);
               return dbLogin != null && HashUtils.VerifyHashMatch256(apikey, dbLogin.UserName, dbLogin.APIKeyHash);
            } catch { if (Ctx.Config.IsDebugMode) { throw; } }
         }
         return false;
      }


      public async Task<DBAccount> GetAccount(long AccountID, string UserName = null) {
         if(AccountID <= 0 && String.IsNullOrWhiteSpace(UserName)) { throw new ArgumentNullException("AccountID or UserName must be specified."); }

         using(SqlCommand cmd = new SqlCommand("[dbo].[sp_Account_GetOne]", new SqlConnection(Ctx.Config.dbUniHangoutsRead)) { CommandType = CommandType.StoredProcedure }) {
            cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(@AccountID), AccountID);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@UserName), @UserName);

             return await cmd.ExecuteReader_GetOneAsync<DBAccount>().ConfigureAwait(false);
         }
      }




   }
}
