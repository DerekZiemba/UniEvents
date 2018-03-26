using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


using static ZMBA.Common;
using UniEvents.Core;
using UniEvents.Models.ApiModels;
using ApiModels = UniEvents.Models.ApiModels;
using DBModels = UniEvents.Models.DBModels;


namespace UniEvents.Managers {

	public class AccountManager {
      private readonly CoreContext Ctx;

		internal AccountManager(CoreContext ctx) {
         this.Ctx = ctx;
		}

      public async Task<bool> CheckPrivilege(string username, string apikey) {
         if (!username.IsNullOrWhitespace() && !apikey.IsNullOrWhitespace()) {
            try {
               DBModels.DBLogin dbLogin = await DBModels.DBLogin.SP_Account_Login_GetAsync(Ctx, username, apikey).ConfigureAwait(false);
               return dbLogin != null && Crypto.VerifyHashMatch(apikey, dbLogin.UserName, dbLogin.APIKeyHash);
            } catch (Exception ex) { }
         }
         return false;
      }

   }
}
