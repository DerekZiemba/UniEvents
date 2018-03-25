using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using UniEvents.Core;
using UniEvents.Models.ApiModels;
using ZMBA;
using static ZMBA.Common;

using ApiModels = UniEvents.Models.ApiModels;
using DBModels = UniEvents.Models.DBModels;

namespace UniEvents.WebApp {

   public class UserLoginCookie {
      public string UserName { get; set; }
      public string APIKey { get; set; }
      public DateTime VerifyDate { get; set; }
   }

   public class UserContext {
      public UserLoginCookie Cookie { get; set; }
      public UserAccount UserAccount { get; set; }
      public StreetAddress Location => UserAccount?.Location;

      public string UserName => Cookie?.UserName;
      public string APIKey => Cookie?.APIKey;
      public DateTime VerifyDate => (Cookie?.VerifyDate).UnBox();

      public long AccountID { get; set; }
      public DateTime LoginDate { get; set; }
      public string UserDisplayName { get; set; }

      public string FirstName => UserAccount?.FirstName;
      public string LastName => UserAccount?.LastName;
      public string SchoolEmail => UserAccount?.SchoolEmail;
      public string ContactEmail => UserAccount?.ContactEmail;
      public string PhoneNumber => UserAccount?.PhoneNumber;

      public long ParentLocationID { get; set; }
      public long LocationID { get; set; }

      public string LocationName => Location?.Name;
      public string AddressLine => Location?.AddressLine;
      public string Locality => Location?.Locality;
      public string AdminDistrict => Location?.AdminDistrict;
      public string PostalCode => Location?.PostalCode;
      public string CountryRegion => Location?.CountryRegion;

      //public bool IsVerifiedLogin => this.VerifyDate >= DateTime.UtcNow.AddSeconds(-(60*10+1)) && this.VerifyDate <= DateTime.UtcNow;
      public bool IsVerifiedLogin { get; set; }


      public static void RemoveCurrentUserContext(HttpContext httpContext) {
         httpContext.Response.Cookies.Delete("userlogin");
         if (httpContext.Items.ContainsKey(nameof(UserContext))) { httpContext.Items.Remove(nameof(UserContext)); }
         httpContext.Session.Remove(nameof(UserContext));
      }

      public static async Task<UserContext> InitContext(HttpContext httpContext) {
         UserContext ctx = (UserContext)httpContext.Items.GetItemOrDefault(nameof(UserContext)); //Check if we already Inited the context in another controller
         if (ctx != null) { return ctx; }

         UserLoginCookie cookie = Common.JsonSerializer.DeserializeOrDefault<UserLoginCookie>(httpContext.Request.Cookies["userlogin"]);

         return await InitContextFromCookie(httpContext, cookie).ConfigureAwait(false);
      }

      public static async Task<UserContext> InitContextFromCookie(HttpContext httpContext, UserLoginCookie cookie) {
         if(cookie == null || cookie.UserName.IsNullOrWhitespace() || cookie.APIKey.IsNullOrWhitespace()) {
            return null;
         }

         UserContext ctx = null;
         if (httpContext.Session.TryGetValue(nameof(UserContext), out byte[] bytes)) {
            ctx = CompactSerializer.DeserializeGZippedBytes<UserContext>(bytes); //Using Session may be expensive in both memory and CPU. 
            if(ctx.UserName != cookie.UserName || ctx.APIKey != cookie.APIKey) {
               httpContext.Session.Remove(nameof(UserContext));
               ctx = null;
            }
         } 

         if (ctx == null) {
            ctx = new UserContext();
            ctx.Cookie = cookie;

            DBModels.DBLogin dbLogin = await DBModels.DBLogin.SP_Account_Login_GetAsync(WebAppContext.CoreContext, ctx.UserName, ctx.APIKey).ConfigureAwait(false);
            if(dbLogin != null) {
               ctx.Cookie.UserName = dbLogin.UserName;
               ctx.LoginDate = dbLogin.LoginDate;
               ctx.IsVerifiedLogin = Crypto.VerifyHashMatch(ctx.Cookie.APIKey, dbLogin.UserName, dbLogin.APIKeyHash);
            } else {
               ctx.IsVerifiedLogin = false;
            }

            if (ctx.IsVerifiedLogin) {
               DBModels.DBAccount acct = await DBModels.DBAccount.SP_Account_GetAsync(WebAppContext.CoreContext, 0, ctx.UserName).ConfigureAwait(false);
               ctx.AccountID = acct.AccountID;
               ctx.LocationID = acct.LocationID.UnBox();
               ctx.UserAccount = new UserAccount(acct, null);
               ctx.Cookie.VerifyDate = DateTime.UtcNow; //Rolling date

               if (ctx.LocationID > 0) {
                  DBModels.DBLocation dbLoc = await DBModels.DBLocation.SP_Location_GetAsync(WebAppContext.CoreContext, ctx.LocationID).ConfigureAwait(false);
                  if (dbLoc != null) {
                     ctx.ParentLocationID = dbLoc.ParentLocationID.UnBox();
                     ctx.UserAccount.Location = new StreetAddress(dbLoc);
                  }
               }
            }
         } else {
            ctx.Cookie.VerifyDate = cookie.VerifyDate; 

            if (ctx.Cookie.VerifyDate < DateTime.UtcNow.AddMinutes(-10)) {
               DBModels.DBLogin dbLogin = await DBModels.DBLogin.SP_Account_Login_GetAsync(WebAppContext.CoreContext, ctx.Cookie.UserName, ctx.Cookie.APIKey).ConfigureAwait(false);
               if(dbLogin != null) {
                  ctx.Cookie.UserName = dbLogin.UserName;
                  ctx.LoginDate = dbLogin.LoginDate;
                  ctx.IsVerifiedLogin = Crypto.VerifyHashMatch(ctx.Cookie.APIKey, dbLogin.UserName, dbLogin.APIKeyHash);
               } else {
                  ctx.IsVerifiedLogin = false;
               }
            } 
         }

         if (ctx.UserDisplayName.IsEmpty()) {
            if (!(ctx.UserAccount?.DisplayName).IsNullOrWhitespace()) {
               ctx.UserDisplayName = ctx.UserAccount.DisplayName;
            } else if (!(ctx.UserAccount?.FirstName).IsNullOrWhitespace()) {
               ctx.UserDisplayName = new string[] { ctx.UserAccount.FirstName, ctx.UserAccount.LastName }.ToStringJoin(" ");
            } else {
               ctx.UserDisplayName = ctx.UserName;
            }
         }

         if (!ctx.IsVerifiedLogin) {
            httpContext.Response.Cookies.Delete("userlogin");
         } else {
            ctx.Cookie.VerifyDate = DateTime.UtcNow;
            httpContext.Response.Cookies.Append("userlogin", JsonSerializer.Serialize(ctx.Cookie), new CookieOptions() { Expires = DateTime.Now.AddDays(14), SameSite = SameSiteMode.None });
         }

         httpContext.Items[nameof(UserContext)] = ctx; //Expose it to the views and cached it hear for faster access if InitContext is called again in this request. 
         httpContext.Session.Set(nameof(UserContext), CompactSerializer.SerializeToGZippedBytes(ctx));

         return ctx;
      }

   }

}