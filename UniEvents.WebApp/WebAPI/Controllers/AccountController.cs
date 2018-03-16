using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


using UniEvents.WebApp;
using UniEvents.Core;
using UniEvents.Models.ApiModels;
using ApiModels = UniEvents.Models.ApiModels;
using DBModels = UniEvents.Models.DBModels;



namespace UniEvents.WebAPI.Controllers {

   [Produces("application/json")]
   [ApiExplorerSettings(IgnoreApi = false)]
   public class AccountsController : Controller {

      [HttpGet, Route("webapi/account/login/{UserName}/{Password}")]
      public async Task<ApiResult<AccountLogin>> Login(string username, string password) {
         ApiResult<AccountLogin> result = new ApiResult<AccountLogin>();

         AccountLogin login = new AccountLogin();
         DBModels.DBAccount dbAccount = await DBModels.DBAccount.SP_Account_GetAsync(Program.CoreContext, 0, username).ConfigureAwait(false);

         if (Crypto.VerifyHashMatch(password, dbAccount.Salt, dbAccount.PasswordHash)) {
            DBModels.DBLogin dbLogin = new DBModels.DBLogin(){ UserName=username };
            (dbLogin.APIKeyHash, dbLogin.APIKey) = Crypto.CreateAPIKey256(username);
            if (await DBModels.DBLogin.SP_Account_LoginAsync(Program.CoreContext, dbLogin).ConfigureAwait(false)) {
               ZMBA.Common.CopyPropsShallow(login, dbLogin);
               result.Result = login;
               result.Success = true;
            }
         } else {
            result.Message = "Invalid_Password";
         }
         return result;
      }

      [HttpGet, Route("webapi/account/verifylogin/{username}/{apikey}")]
      public async Task<bool> VerifyLogin(string username, string apikey) {
         DBModels.DBLogin dbLogin = await DBModels.DBLogin.SP_Account_Login_GetAsync(Program.CoreContext, username, apikey).ConfigureAwait(false);
         return dbLogin != null && Crypto.VerifyHashMatch(apikey, dbLogin.UserName, dbLogin.APIKeyHash);
      }

      [HttpPost, Route("webapi/account/create")]
      public async Task<ApiResult<UserAccount>> Create(UserAccount account) {
         Task<bool> addLocationTask = null;
         Task<bool> createAccountTask = null;

         if (account == null) {
            return new ApiResult<UserAccount>(false, "Account Null.");
         }
         if (account.Location == null || !account.Location.IsValid()) {
            return new ApiResult<UserAccount>(false, "Location not set or invalid.");
         }

         DBModels.DBLocation dbLocation = ZMBA.Common.CopyPropsShallow(new DBModels.DBLocation(), account.Location);
         DBModels.DBAccount dbAccount = ZMBA.Common.CopyPropsShallow(new DBModels.DBAccount(), account);
         (dbAccount.PasswordHash, dbAccount.Salt) = Crypto.HashPassword256(account.Password);
         account.Password = null; //don't want to keep in memory attached to model.

         addLocationTask = DBModels.DBLocation.SP_Location_CreateAsync(Program.CoreContext, dbLocation);//runs method on background thread and continues.
         createAccountTask = DBModels.DBAccount.SP_Account_CreateAsync(Program.CoreContext, dbAccount);

         ApiResult<UserAccount> result = new ApiResult<UserAccount>();

         try {
            await Task.WhenAll(addLocationTask, createAccountTask).ConfigureAwait(false); //wait for background tasks to complete        
            result.Success = createAccountTask.Result && addLocationTask.Result;
            if (result.Success) {
               result.Result = ZMBA.Common.CopyPropsShallow(new ApiModels.UserAccount(), dbAccount);
               result.Result.Location = ZMBA.Common.CopyPropsShallow(new ApiModels.StreetAddress(), dbLocation);
            }
         } catch (Exception ex) {
            //TODO: If this happens, we need to roll back changes to database. Such as if the location was successfully added but the account was not or vice versa.
            result.Message = ex.Message;
         }
         return result;
      }


   }
}
