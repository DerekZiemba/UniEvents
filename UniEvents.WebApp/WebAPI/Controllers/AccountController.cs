using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


using UniEvents.WebApp;
using UniEvents.Core;
using ApiModels = UniEvents.Models.ApiModels;
using DBModels = UniEvents.Models.DBModels;



namespace UniEvents.WebAPI.Controllers {

   [Produces("application/json")]
   [ApiExplorerSettings(IgnoreApi = false, GroupName = nameof(AccountsController))]
   public class AccountsController : Controller {

      [HttpGet, Route("webapi/account/login/{UserName}/{Password}")]
      public async Task<ApiModels.AccountLogin> Login(string username, string password) {
         ApiModels.AccountLogin login = new ApiModels.AccountLogin();
         DBModels.DBAccount dbAccount = await DBModels.DBAccount.SP_Account_GetAsync(Program.CoreContext, 0, username).ConfigureAwait(false);

         if (Crypto.VerifyHashMatch(password, dbAccount.Salt, dbAccount.PasswordHash)) {
            DBModels.DBLogin dbLogin = new DBModels.DBLogin(){ UserName=username };
            (dbLogin.APIKeyHash, dbLogin.APIKey) = Crypto.CreateAPIKey256(username);
            if (await DBModels.DBLogin.SP_Account_LoginAsync(Program.CoreContext, dbLogin).ConfigureAwait(false)) {
               ZMBA.Common.CopyPropsShallow(login, dbLogin);
               login.Success = true;
            }
         } else {
            login.Message = "Invalid_Password";
         }
         return login;
      }

      [HttpGet, Route("webapi/account/verifylogin/{username}/{apikey}")]
      public async Task<bool> VerifyLogin(string username, string apikey) {
         DBModels.DBLogin dbLogin = await DBModels.DBLogin.SP_Account_Login_GetAsync(Program.CoreContext, username, apikey).ConfigureAwait(false);
         return dbLogin != null && Crypto.VerifyHashMatch(apikey, dbLogin.UserName, dbLogin.APIKeyHash);
      }

      [HttpPost, Route("webapi/account/create")]
      public async Task<ApiModels.UserAccount> Create(ApiModels.UserAccount account) {
         ApiModels.UserAccount result = new ApiModels.UserAccount();
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

         DBModels.DBLocation dbLocation = ZMBA.Common.CopyPropsShallow(new DBModels.DBLocation(), account.Location);
         addLocationTask = DBModels.DBLocation.SP_Location_CreateAsync(Program.CoreContext, dbLocation);//runs method on background thread and continues.

         DBModels.DBAccount dbAccount = ZMBA.Common.CopyPropsShallow(new DBModels.DBAccount(), account);
         (dbAccount.PasswordHash, dbAccount.Salt) = Crypto.HashPassword256(account.Password);
         account.Password = null; //don't want to keep in memory attached to model.

         createAccountTask = DBModels.DBAccount.SP_Account_CreateAsync(Program.CoreContext, dbAccount);

         try {
            await Task.WhenAll(addLocationTask, createAccountTask).ConfigureAwait(false); //wait for background tasks to complete

            result.Success = createAccountTask.Result && addLocationTask.Result;
            if (result.Success) {
               ZMBA.Common.CopyPropsShallow(result, dbAccount);
               result.Location = ZMBA.Common.CopyPropsShallow(new ApiModels.StreetAddress(), dbLocation);
            }
         } catch (Exception ex) {
            //TODO: If this happens, we need to roll back changes to database. Such as if the location was successfully added but the account was not or vice versa.
            result.Message = ex.Message;
         }
         return result;
      }


   }
}
