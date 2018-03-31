using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


using ZMBA;
using UniEvents.Core;
using UniEvents.Models.ApiModels;
using ApiModels = UniEvents.Models.ApiModels;
using DBModels = UniEvents.Models.DBModels;


namespace UniEvents.Core.Managers {

   public class AccountManager {
      private readonly Factory Ctx;

      internal AccountManager(Factory ctx) {
         this.Ctx = ctx;
      }

      public async Task<bool> CheckPrivilege(string username, string apikey) {
         if (username.IsNotWhitespace() && apikey.IsNotWhitespace()) {
            try {
               DBModels.DBLogin dbLogin = await DBModels.DBLogin.SP_Account_Login_GetAsync(Ctx, username, apikey).ConfigureAwait(false);
               return dbLogin != null && HashUtils.VerifyHashMatch256(apikey, dbLogin.UserName, dbLogin.APIKeyHash);
            } catch { if (Ctx.Config.IsDebugMode) { throw; } }
         }
         return false;
      }

   }
}
