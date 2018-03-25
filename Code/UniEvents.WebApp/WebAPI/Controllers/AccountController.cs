using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


using UniEvents.WebApp;
using UniEvents.Core;
using UniEvents.Models.ApiModels;
using ApiModels = UniEvents.Models.ApiModels;
using DBModels = UniEvents.Models.DBModels;
using static ZMBA.Common;


namespace UniEvents.WebAPI.Controllers {

   [Produces("application/json")]
   [ApiExplorerSettings(IgnoreApi = false)]
   public class AccountsController : WebAppController {

      [HttpGet, Route("webapi/account/login/{UserName?}/{Password?}")]
      public async Task<ApiResult<AccountLogin>> Login(string username, string password) {
         if (this.UserContext.IsLoggedIn) {
            return new ApiResult<AccountLogin>(false, "You are all ready logged in. Logout before you can login.");
         }
         ApiResult<AccountLogin> login = await this.AccountManager().Login(username, password);
         //HttpContext.Request.Cookies.
         return login;
      }


      [HttpGet, Route("webapi/account/verifylogin/{username?}/{apikey?}")]
      public async Task<ApiResult<VerifiedLogin>> VerifyLogin(string username, string apikey) {
         if (username.IsNullOrWhitespace()) {
            return new ApiResult<VerifiedLogin>(false, "Invalid Username.");
         }
         if (apikey.IsNullOrWhitespace()) {
            return new ApiResult<VerifiedLogin>(false, "Invalid ApiKey.");
         }
         var result = new ApiResult<VerifiedLogin>();
         try {
            result.Result = await this.AccountManager().GetVerifiedLogin(username, apikey);
            if (!result.Result.IsLoggedIn) {
               result.Message = "Invalid Login";
            }
            result.Success = true;
         } catch (Exception ex) {
            result.Message = ex.Message + " \n " + ex.InnerException?.Message;
         }
         return result;
      }


      /// <param name="username"></param>
      /// <param name="keypass">If APIKey, logs out device.  If password, logout everywhere.</param>
      [HttpGet, Route("webapi/account/logout/{username?}/{keypass?}")]
      public async Task<ApiResult> Logout(string username, string keypass) {
         return await this.AccountManager().Logout(username, keypass);
      }


      [HttpPost, Route("webapi/account/createuser/{password?}")]
      public async Task<ApiResult<UserAccount>> CreateUser(UserAccount user, string password) {
         return await this.AccountManager().CreateUser(user, password);
      }

      /// <summary>
      /// Note: this allows any auth'ed user to get any other users info.  We need to implement a permission system. 
      /// </summary>
      [HttpPost, Route("webapi/account/getuserinfo/{username?}/{apikey?}/{includeLocation?}")]
      public async Task<ApiResult<UserAccount>> GetUserInfo(string username, string apikey, bool includeLocation) {
         if (!await this.AccountManager().CheckPrivilege(username, apikey)) {
            return new ApiResult<UserAccount>(false, "Check your privilege. This is a privileged operation.");
         }
         return await this.AccountManager().GetUser(username, includeLocation);
      }

      public AccountsController(IHttpContextAccessor accessor): base(accessor) { }
   }
}
