﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


using UniEvents.WebApp;
using UniEvents.Core;
using UniEvents.Models.ApiModels;
using UniEvents.Models.DBModels;
using static ZMBA.Common;


namespace UniEvents.WebAPI.Controllers {

   [Produces("application/json")]
   [ApiExplorerSettings(IgnoreApi = false)]
   public class AccountsController : WebAppController {

      [HttpGet, Route("webapi/account/login/{UserName?}/{Password?}")]
      public async Task<ApiResult<UserLoginCookie>> Login(string username, string password) {
         ApiResult<UserLoginCookie> apiresult = new ApiResult<UserLoginCookie>();

         if (this.UserContext != null) { return apiresult.Failure("You are all ready logged in. Logout before you can login."); }
         if (username.IsNullOrWhitespace() || password.IsNullOrWhitespace()) { return apiresult.Failure("Invalid Username/Password");  }

         DBAccount dbAccount = null;
         try {
            dbAccount = await DBAccount.SP_Account_GetOneAsync(WebAppContext.CoreContext, 0, username).ConfigureAwait(false);
         } catch (Exception ex) { return apiresult.Failure(ex); }

         if (dbAccount == null) {  return apiresult.Failure("Account does not exist.");  }
         if (!Crypto.VerifyHashMatch(password, dbAccount.Salt, dbAccount.PasswordHash)) {  return apiresult.Failure("Invalid Password");  }

         DBLogin dbLogin = null;
         try {
            dbLogin = await DBLogin.LoginUserNameAsync(WebAppContext.CoreContext, dbAccount.UserName).ConfigureAwait(false);
         } catch (Exception ex) { return apiresult.Failure(ex); }
         
         if (dbLogin == null) {  apiresult.Failure("Login Failed");  }

         apiresult.Win(new UserLoginCookie() { UserName = username, APIKey = dbLogin.APIKey, VerifyDate = dbLogin.LoginDate });

         var ctx = await UserContext.InitContextFromCookie(this.HttpContext, apiresult.Result).ConfigureAwait(false);
         if (ctx == null) {
            return apiresult.Failure("Failed to set login cookie for unknown reason.");
         }
         return apiresult;
      }


      [HttpGet, Route("webapi/account/verifyapikey/{username?}/{apikey?}")]
      public async Task<ApiResult> VerifyApiKey(string username, string apikey) {
         ApiResult apiresult = new ApiResult();
         if (username.IsNullOrWhitespace()) { return apiresult.Failure("Invalid Username."); }
         if (apikey?.Length != Crypto.APIKeyLength) { return apiresult.Failure("Invalid APIKey."); }
         if (this.UserContext == null) { return apiresult.Failure("You do not have permission to perform this action."); }
         if (!this.UserContext.IsVerifiedLogin) { return apiresult.Failure("Login Credentials Expired. Try Relogging In."); }

         DBLogin dbLogin;
         try {
            dbLogin = await DBLogin.SP_Account_Login_GetAsync(WebAppContext.CoreContext, username, apikey).ConfigureAwait(false);
         } catch (Exception ex) { return apiresult.Failure(ex); }

         if (dbLogin != null && Crypto.VerifyHashMatch(apikey, dbLogin.UserName, dbLogin.APIKeyHash)) {
            apiresult.Win("Is Valid ApiKey");
         } else {
            apiresult.Failure("ApiKey is Invalid");
         }

         return apiresult;
      }

      /// <summary>
      /// If username and apikeyorpassword are empty, logs out current user.
      /// If username and apikeyorpassword have value, logs out just that apikeyorpassword
      /// If username has value, logs out that user everywhere, if this user has permission to do that.
      /// </summary>
      [HttpGet, Route("webapi/account/logout/{username?}/{apikeyorpassword?}")]
      public async Task<ApiResult> Logout(string username, string apikeyorpassword) {
         var bHasUserName = !username.IsNullOrWhitespace();
         var bHasPass = !apikeyorpassword.IsNullOrWhitespace();
         if (!bHasUserName && !bHasPass || (UserContext != null && UserContext.UserName == username && UserContext.APIKey == apikeyorpassword)) {
            return await LogOutCurrentUser().ConfigureAwait(false);
         }
         if(bHasUserName && !bHasPass) {
            return await LogOutCurrentUserEverywhere().ConfigureAwait(false);
         }
         if (!bHasUserName || !bHasPass) {
            return new ApiResult().Failure("Invalid Username, Password, or APIKey");
         }
         if (apikeyorpassword.Length == Crypto.APIKeyLength) {
            return await LogOutApiKey(username, apikeyorpassword).ConfigureAwait(false);
         } else {
            return await LogUserOutEverywhere(username, apikeyorpassword).ConfigureAwait(false);
         }

         async Task<ApiResult> LogOutCurrentUser() {
            ApiResult apiresult = new ApiResult();
            if (this.UserContext == null) { return apiresult.Failure("You must be logged in order to logout."); }

            bool bLoggedOut;
            try {
               bLoggedOut = await DBLogin.SP_Account_LogoutAsync(WebAppContext.CoreContext, UserContext.UserName, UserContext.APIKey).ConfigureAwait(false);
            } catch (Exception ex) { return apiresult.Failure(ex); }


            if (bLoggedOut) {
               apiresult.Win($"Logged out {UserContext.UserName}'s APIKey {UserContext.APIKey}");
               UserContext.RemoveCurrentUserContext(this.HttpContext);
            } else {
               apiresult.Failure($"Failed to log out {UserContext.UserName}'s APIKey {UserContext.APIKey}");
            }
            return apiresult;
         }
         async Task<ApiResult> LogOutCurrentUserEverywhere() {
            ApiResult apiresult = new ApiResult();
            if (this.UserContext == null) { return apiresult.Failure("You must be logged in order to logout."); }

            bool bLoggedOut;
            try {
               bLoggedOut = await DBLogin.SP_Account_LogoutAsync(WebAppContext.CoreContext, UserContext.UserName, null).ConfigureAwait(false);
            } catch (Exception ex) { return apiresult.Failure(ex); }

            if (bLoggedOut) {
               apiresult.Win($"Logged out {UserContext.UserName}'s everywhere.");
               UserContext.RemoveCurrentUserContext(this.HttpContext);
            } else {
               apiresult.Failure($"Failed to log out {UserContext.UserName}'s anywhere.");
            }
            return apiresult;
         }
         async Task<ApiResult> LogOutApiKey(string _username, string _apikey) {
            ApiResult apiresult = new ApiResult();
            bool bLoggedOut;
            try {
               bLoggedOut = await DBLogin.SP_Account_LogoutAsync(WebAppContext.CoreContext, _username, _apikey).ConfigureAwait(false);
            } catch (Exception ex) { return apiresult.Failure(ex); }

            if (bLoggedOut) {
               apiresult.Win($"Logged out: {_username}'s APIKey {_apikey}");
            } else {
               apiresult.Failure($"Failed to log out: {_username}'s APIKey {_apikey}");
            }
            return apiresult;
         }
         async Task<ApiResult> LogUserOutEverywhere(string _username, string _password) {
            ApiResult apiresult = new ApiResult();          
            DBAccount dbAccount;
            try {
               dbAccount = await DBAccount.SP_Account_GetOneAsync(WebAppContext.CoreContext, 0, _username).ConfigureAwait(false);
            } catch (Exception ex) { return apiresult.Failure(ex); }

            if (dbAccount == null) {
               return apiresult.Failure("Account does not exist.");
            }
            if (!Crypto.VerifyHashMatch(_password, dbAccount.Salt, dbAccount.PasswordHash)) {
               return apiresult.Failure("Invalid Password");
            }

            bool bLoggedOut;
            try {
               bLoggedOut = await DBLogin.SP_Account_LogoutAsync(WebAppContext.CoreContext, _username, null).ConfigureAwait(false);
            } catch (Exception ex) { return apiresult.Failure(ex); }

            if (bLoggedOut) {
               apiresult.Win($"Logged out {_username} everywhere.");
               if(UserContext != null && UserContext.UserName == _username) {
                  UserContext.RemoveCurrentUserContext(this.HttpContext);
               }
            } else {
               apiresult.Failure($"Failed to log out {_username} anywhere.");
            }
            return apiresult;
         }
      }


      [HttpPost, Route("webapi/account/createuser/{password?}")]
      public async Task<ApiResult<UserAccount>> CreateUser(UserAccount user, string password) {
         ApiResult<UserAccount> apiresult = new ApiResult<UserAccount>();

         if (user == null) {
            return apiresult.Failure("Account Null.");
         }
         if (password.IsNullOrWhitespace()) {
            return apiresult.Failure("Invalid Password.");
         }
         if (user.UserName.IsNullOrWhitespace()) {
            return apiresult.Failure("Invalid UserName.");
         }
         if (user.Location == null) {
            return apiresult.Failure("Location Invalid.");
         }
         if (user.Location.CountryRegion.IsNullOrWhitespace()) {
            return apiresult.Failure("Invalid CountryRegion.");
         }
         if (user.Location.AdminDistrict.IsNullOrWhitespace()) {
            return apiresult.Failure("Invalid State.");
         }
         if (user.Location.Locality.IsNullOrWhitespace()) {
            return apiresult.Failure("Invalid City");
         }
         if (user.VerifiedContactEmail || user.VerifiedSchoolEmail) {
            return apiresult.Failure("Attempt to submit unverified Emails logged and detected."); //not really, but sounds scary.
         }

         DBLocation dbLocation = new DBLocation(user.Location);
         DBAccount dbAccount = new DBAccount() {
            UserName = user.UserName,
            DisplayName = user.DisplayName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            SchoolEmail = user.SchoolEmail,
            ContactEmail = user.ContactEmail,
            PhoneNumber = user.PhoneNumber
         };

         (dbAccount.PasswordHash, dbAccount.Salt) = Crypto.HashPassword256(password);
         try {
            if (!await DBLocation.SP_Locations_CreateOneAsync(WebAppContext.CoreContext, dbLocation).ConfigureAwait(false)) {
               return apiresult.Failure("Failed to create location.");
            }

            dbAccount.LocationID = dbLocation.LocationID;
            if (!await DBAccount.SP_Account_CreateAsync(WebAppContext.CoreContext, dbAccount).ConfigureAwait(false)) {
               return apiresult.Failure("Failed to create account.");
            }

            return apiresult.Win("Account Created!", new UserAccount(dbAccount, new StreetAddress(dbLocation)));

         } catch (Exception ex) {
            return apiresult.Failure(ex);
         }

      }

      [HttpPost, Route("webapi/account/getuserinfo/{username?}/{includeLocation?}")]
      public async Task<ApiResult<UserAccount>> GetUserInfo(string username, bool includeLocation) {
         var apiresult = new ApiResult<UserAccount>();
         if (UserContext == null) { return apiresult.Failure("Must be logged in."); }
         if (!UserContext.IsVerifiedLogin) { return apiresult.Failure("Check your privilege. This is a privileged operation."); }

         try {
            DBAccount dbAccount = await DBAccount.SP_Account_GetOneAsync(WebAppContext.CoreContext, 0, username).ConfigureAwait(false);
            if (dbAccount == null) {
               return apiresult.Failure("User doesn't exist");
            }
            apiresult.Result = new UserAccount(dbAccount, null);

            if (includeLocation && dbAccount.LocationID.UnBox() > 0) {
               using (SqlCommand cmd = DBLocation.GetSqlCommandForSP_Locations_GetOne(WebAppContext.CoreContext, dbAccount.LocationID.Value)) {
                  DBLocation dbLocation = await cmd.ExecuteReader_GetOneAsync<DBLocation>().ConfigureAwait(false);
                  apiresult.Result.Location = new StreetAddress(dbLocation);
               }
            }
            return apiresult.Win(apiresult.Result);

         } catch (Exception ex) {
            return apiresult.Failure(ex);
         }
      }

      public AccountsController(IHttpContextAccessor accessor): base(accessor) { }
   }
}
