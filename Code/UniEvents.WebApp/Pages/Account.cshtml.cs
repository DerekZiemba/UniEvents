using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UniEvents.WebApp.Pages {
   public class AccountModel : WebAppPageModel {

      public List<Models.ApiModels.AccountLogin> AllLogins { get; } = new List<Models.ApiModels.AccountLogin>();

      public Models.ApiModels.AccountLogin CurrentLogin { get; private set; }

      
      public void OnGet() {
         if(UserContext != null && UserContext.IsVerifiedLogin) {
            foreach(var dbLogin in Models.DBModels.DBLogin.SP_Account_Login_GetAll(WebAppContext.Factory, UserContext.UserName)) {
               var apiLogin = new Models.ApiModels.AccountLogin(dbLogin);
               if(apiLogin.APIKey == UserContext.APIKey) {
                  CurrentLogin = apiLogin;
               }
               AllLogins.Add(apiLogin);
            }
            AllLogins.Sort((x, y) => x.LoginDate.CompareTo(y.LoginDate));
         }       
      }

      public AccountModel(IHttpContextAccessor accessor) : base(accessor) { }

   }
}