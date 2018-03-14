using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Web.Http;

using UniEvents.Core;
using UniEvents.Core.ApiModels;


namespace UniEvents.WebAPI.Controllers {

   public class AccountsController : ApiController {

      [HttpGet, Route("account/login/{UserName}/{Password}")]
      public async Task<AccountLogin> Login(string username, string password) {
         AccountLogin login = new AccountLogin();
         Core.DBModels.DBAccount dbAccount = await Core.DBModels.DBAccount.SP_Account_GetAsync(WebApiApplication.CoreContext, 0, username).ConfigureAwait(false);
         
         if (Crypto.VerifyHashMatch(password, dbAccount.Salt, dbAccount.PasswordHash)) {
            Core.DBModels.DBLogin dbLogin = new Core.DBModels.DBLogin(){ UserName=username };
            (dbLogin.APIKeyHash, dbLogin.APIKey) = Crypto.CreateAPIKey256(username);
            if(await Core.DBModels.DBLogin.SP_Account_LoginAsync(WebApiApplication.CoreContext, dbLogin).ConfigureAwait(false)) {
               ZMBA.Common.CopyPropsShallow(login, dbLogin);
               login.Success = true;
            }
         } else {
            login.Message = "Invalid_Password";
         }
         return login;
      }

      [HttpGet, Route("account/login/{username}/{apikey}")]
      public async Task<bool> VerifyLogin(string username, string apikey) {
         Core.DBModels.DBLogin dbLogin = await Core.DBModels.DBLogin.SP_Account_Login_GetAsync(WebApiApplication.CoreContext, username, apikey).ConfigureAwait(false);
         return dbLogin != null && Crypto.VerifyHashMatch(apikey, dbLogin.UserName, dbLogin.APIKeyHash);
      }

      [HttpPost, Route("account/create")]
      public async Task<UserAccount> Create(UserAccount account) {
         UserAccount result = new UserAccount();
         Task<bool> addLocationTask = null;
         Task<bool> createAccountTask = null;

         if (account == null) {
            result.Message = "Account Null.";
            return result;
         }
         if (account.Location == null || !account.Location.IsValid()) {
            result.Message = "Location not set or invalid.";
            return result;
         }

         Core.DBModels.DBLocation dbLocation = ZMBA.Common.CopyPropsShallow(new Core.DBModels.DBLocation(), account.Location);
         addLocationTask = Core.DBModels.DBLocation.SP_Location_CreateAsync(WebApiApplication.CoreContext, dbLocation);//runs method on background thread and continues.

         Core.DBModels.DBAccount dbAccount = ZMBA.Common.CopyPropsShallow(new Core.DBModels.DBAccount(), account);
         (dbAccount.PasswordHash, dbAccount.Salt) = Crypto.HashPassword256(account.Password);
         account.Password = null; //don't want to keep in memory attached to model.

         createAccountTask = Core.DBModels.DBAccount.SP_Account_CreateAsync(WebApiApplication.CoreContext, dbAccount);

         try {
            await Task.WhenAll(addLocationTask, createAccountTask).ConfigureAwait(false); //wait for background tasks to complete

            result.Success = createAccountTask.Result && addLocationTask.Result;
            if (result.Success) {
               ZMBA.Common.CopyPropsShallow(result, dbAccount);
               result.Location = ZMBA.Common.CopyPropsShallow(new StreetAddress(), dbLocation);
            }
         } catch (Exception ex) {
            //TODO: If this happens, we need to roll back changes to database. Such as if the location was successfully added but the account was not or vice versa.
            result.Message = ex.Message;
         }
         return result;
      }


   }
}
