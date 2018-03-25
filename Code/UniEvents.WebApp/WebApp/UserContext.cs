using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using UniEvents.Models.ApiModels;
using ZMBA;


namespace UniEvents.WebApp {

   public class UserContext {
      public AccountLogin UserLoginCookie { get; set; }
      public UserAccount UserAccount { get; set; }
      public VerifiedLogin VerifiedLogin { get; set; }

      public bool IsLoggedIn => (VerifiedLogin?.IsLoggedIn).UnBox();

      public string GetUserDisplayName() {
         if(UserAccount != null) {
            if (!UserAccount.DisplayName.IsNullOrWhitespace()) { return UserAccount.DisplayName; }
            if (!UserAccount.FirstName.IsNullOrWhitespace()) { return new string[] { UserAccount.FirstName, UserAccount.LastName }.ToStringJoin(" "); }
            return UserAccount.UserName;
         }
         return UserLoginCookie?.UserName;
      }


      public void UpdateLoginStatus(HttpContext httpContext, AccountLogin login) {
         if(login == null) {
            if (httpContext.Items.ContainsKey(nameof(UserContext))) { httpContext.Items.Remove(nameof(UserContext)); }
            //httpContext.Request.Cookies.
         }
      }

      public static UserContext InitContext(HttpContext httpContext) {
         UserContext cached = (UserContext)httpContext.Items.GetItemOrDefault(nameof(UserContext)); //Check if we already Inited the context in another controller
         if(cached != null) { return cached; }

         UserContext uctx = null;
         byte[] bytes = null;
         if (httpContext.Session.TryGetValue(nameof(UserContext), out bytes)) {
            uctx = Common.CompactSerializer.DeserializeGZippedBytes<UserContext>(bytes); //Ussing Session may be expensive in both memory and CPU. 
         }

         if(uctx == null) { uctx = new UserContext(); }

         httpContext.Items[nameof(UserContext)] = uctx; //Expose it to the views and cached it hear for faster access if InitContext is called again in this request. 

         if (uctx.UserLoginCookie == null) {
            uctx.UserLoginCookie = Common.JsonSerializer.DeserializeOrDefault<AccountLogin>(httpContext.Request.Cookies["userlogin"]);
            if (uctx.UserLoginCookie != null && !uctx.UserLoginCookie.UserName.IsEmpty() && !uctx.UserLoginCookie.APIKey.IsNullOrWhitespace()) {
               Task<ApiResult<UserAccount>> getUserTask = WebAppContext.CoreContext.AccountManager.GetUser(uctx.UserLoginCookie.UserName, true);
               Task<VerifiedLogin> getVerifiedLoginTask = WebAppContext.CoreContext.AccountManager.GetVerifiedLogin(uctx.UserLoginCookie.UserName, uctx.UserLoginCookie.APIKey);

               uctx.UserAccount = getUserTask.ConfigureAwait(false).GetAwaiter().GetResult().Result;
               uctx.VerifiedLogin = getVerifiedLoginTask.ConfigureAwait(false).GetAwaiter().GetResult();

               uctx.UserLoginCookie.DisplayName = uctx.GetUserDisplayName();
               CacheToSession(httpContext, uctx);
            }
         }

         return uctx;
      }

      public static void CacheToSession(HttpContext httpContext, UserContext uctx) {
         httpContext.Session.Set(nameof(UserContext), Common.CompactSerializer.SerializeToGZippedBytes(uctx));
      }

      public static void SaveToCookie(HttpContext httpContext, UserContext uctx) {
         CookieOptions cookie = new CookieOptions() {Expires = DateTime.Now.AddDays(14) };
         if(uctx.UserLoginCookie != null) { uctx.UserLoginCookie.DisplayName = uctx.GetUserDisplayName();  }
         httpContext.Response.Cookies.Append("userlogin", Common.JsonSerializer.Serialize(uctx.UserLoginCookie), cookie);
      }

   }

}