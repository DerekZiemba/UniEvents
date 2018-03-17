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

      [HttpGet, Route("webapi/account/login/{UserName?}/{Password?}")]
      public async Task<ApiResult<AccountLogin>> Login(string username, string password) {
         return await Program.CoreContext.AccountManager.Login(username, password);
      }


      [HttpGet, Route("webapi/account/verifylogin/{username?}/{apikey?}")]
      public async Task<ApiResult<VerifiedLogin>> VerifyLogin(string username, string apikey) {
         return await Program.CoreContext.AccountManager.VerifyLogin(username, apikey);
      }


      /// <param name="username"></param>
      /// <param name="keypass">If APIKey, logs out device.  If password, logout everywhere.</param>
      [HttpGet, Route("webapi/account/logout/{username?}/{keypass?}")]
      public async Task<ApiResult> Logout(string username, string keypass) {
         return await Program.CoreContext.AccountManager.Logout(username, keypass);
      }


      [HttpPost, Route("webapi/account/createuser/{password?}")]
      public async Task<ApiResult<UserAccount>> CreateUser(UserAccount user, string password) {
         return await Program.CoreContext.AccountManager.CreateUser(user, password);
      }


   }
}
