using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


using UniEvents.WebApp;
using UniEvents.Core;
using UniEvents.Models;
using UniEvents.Models.ApiModels;
using UniEvents.Models.DBModels;
using ZMBA;


namespace UniEvents.WebAPI.Controllers {

   [Produces("application/json")]
   [ApiExplorerSettings(IgnoreApi = false)]
   public class AccountsController : WebAPIController {

      [HttpGet, Route("webapi/account/login/{UserName?}/{Password?}")]
      public async Task<ApiResult<UserLoginCookie>> Login(string username, string password) {
         ApiResult<UserLoginCookie> apiresult = new ApiResult<UserLoginCookie>();

         if (this.UserContext != null) { return apiresult.Failure("You are all ready logged in. Logout before you can login."); }
         if (String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(password)) { return apiresult.Failure("Invalid Username/Password");  }

         DBAccount dbAccount = null;
         try {
            dbAccount = await WebAppContext.Factory.AccountManager.GetAccount(0, username).ConfigureAwait(false);
         } catch (Exception ex) { return apiresult.Failure(ex); }

         if (dbAccount == null) {  return apiresult.Failure("Account does not exist.");  }
         if (!HashUtils.VerifyHashMatch256(password, dbAccount.Salt, dbAccount.PasswordHash)) {  return apiresult.Failure("Invalid Password");  }

         DBLogin dbLogin = null;
         try {
            dbLogin = await DBLogin.LoginUserNameAsync(WebAppContext.Factory, dbAccount.UserName).ConfigureAwait(false);
         } catch (Exception ex) { return apiresult.Failure(ex); }
         
         if (dbLogin == null) {  apiresult.Failure("Login Failed");  }

         apiresult.Success(new UserLoginCookie() { UserName = username, APIKey = dbLogin.APIKey, VerifyDate = dbLogin.LoginDate });

         try {
            var ctx = await UserContext.InitContextFromCookie(this.HttpContext, apiresult.Result).ConfigureAwait(false);
            if (ctx == null) {
               return apiresult.Failure("Failed to set login cookie for unknown reason.");
            }
         } catch (Exception ex) { return apiresult.Failure(ex); }

         return apiresult;
      }


      [HttpGet, Route("webapi/account/verifyapikey/{username?}/{apikey?}")]
      public async Task<ApiResult> VerifyApiKey(string username, string apikey) {
         ApiResult apiresult = new ApiResult();
         if (String.IsNullOrWhiteSpace(username)) { return apiresult.Failure("Invalid Username."); }
         if (apikey?.Length != HashUtils.APIKeyLength256) { return apiresult.Failure("Invalid APIKey."); }
         if (this.UserContext == null) { return apiresult.Failure("You do not have permission to perform this action."); }
         if (!this.UserContext.IsVerifiedLogin) { return apiresult.Failure("Login Credentials Expired. Try Relogging In."); }

         DBLogin dbLogin;
         try {
            dbLogin = await DBLogin.SP_Account_Login_GetAsync(WebAppContext.Factory, username, apikey).ConfigureAwait(false);
         } catch (Exception ex) { return apiresult.Failure(ex); }

         if (dbLogin != null && HashUtils.VerifyHashMatch256(apikey, dbLogin.UserName, dbLogin.APIKeyHash)) {
            apiresult.Success("Is Valid ApiKey");
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
         var bHasUserName = username.IsNotWhitespace();
         var bHasPass = apikeyorpassword.IsNotWhitespace();
         if (!bHasUserName && !bHasPass || (UserContext != null && UserContext.UserName == username && UserContext.APIKey == apikeyorpassword)) {
            return await LogOutCurrentUser().ConfigureAwait(false);
         }
         if(bHasUserName && !bHasPass) {
            return await LogOutCurrentUserEverywhere().ConfigureAwait(false);
         }
         if (!bHasUserName || !bHasPass) {
            return new ApiResult().Failure("Invalid Username, Password, or APIKey");
         }
         if (apikeyorpassword.Length == HashUtils.APIKeyLength256) {
            return await LogOutApiKey(username, apikeyorpassword).ConfigureAwait(false);
         } else {
            return await LogUserOutEverywhere(username, apikeyorpassword).ConfigureAwait(false);
         }

         async Task<ApiResult> LogOutCurrentUser() {
            ApiResult apiresult = new ApiResult();
            if (this.UserContext == null) { return apiresult.Failure("You must be logged in order to logout."); }

            bool bLoggedOut;
            try {
               bLoggedOut = await DBLogin.SP_Account_LogoutAsync(WebAppContext.Factory, UserContext.UserName, UserContext.APIKey).ConfigureAwait(false);
            } catch (Exception ex) { return apiresult.Failure(ex); }


            if (bLoggedOut) {
               apiresult.Success($"Logged out {UserContext.UserName}'s APIKey {UserContext.APIKey}");
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
               bLoggedOut = await DBLogin.SP_Account_LogoutAsync(WebAppContext.Factory, UserContext.UserName, null).ConfigureAwait(false);
            } catch (Exception ex) { return apiresult.Failure(ex); }

            if (bLoggedOut) {
               apiresult.Success($"Logged out {UserContext.UserName}'s everywhere.");
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
               bLoggedOut = await DBLogin.SP_Account_LogoutAsync(WebAppContext.Factory, _username, _apikey).ConfigureAwait(false);
            } catch (Exception ex) { return apiresult.Failure(ex); }

            if (bLoggedOut) {
               apiresult.Success($"Logged out: {_username}'s APIKey {_apikey}");
            } else {
               apiresult.Failure($"Failed to log out: {_username}'s APIKey {_apikey}");
            }
            return apiresult;
         }
         async Task<ApiResult> LogUserOutEverywhere(string _username, string _password) {
            ApiResult apiresult = new ApiResult();          
            DBAccount dbAccount;
            try {
               dbAccount = await WebAppContext.Factory.AccountManager.GetAccount( 0, _username).ConfigureAwait(false);
            } catch (Exception ex) { return apiresult.Failure(ex); }

            if (dbAccount == null) {
               return apiresult.Failure("Account does not exist.");
            }
            if (!HashUtils.VerifyHashMatch256(_password, dbAccount.Salt, dbAccount.PasswordHash)) {
               return apiresult.Failure("Invalid Password");
            }

            bool bLoggedOut;
            try {
               bLoggedOut = await DBLogin.SP_Account_LogoutAsync(WebAppContext.Factory, _username, null).ConfigureAwait(false);
            } catch (Exception ex) { return apiresult.Failure(ex); }

            if (bLoggedOut) {
               apiresult.Success($"Logged out {_username} everywhere.");
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
      public async Task<ApiResult<UserAccount>> CreateUser(UserAccount input, string password) {
         ApiResult<UserAccount> apiresult = new ApiResult<UserAccount>();

         if (input == null) {
            return apiresult.Failure("Account Null.");
         }
         if (String.IsNullOrWhiteSpace(password)) {
            return apiresult.Failure("Invalid Password.");
         }
         if (String.IsNullOrWhiteSpace(input.UserName)) {
            return apiresult.Failure("Invalid UserName.");
         }
         if (input.Location == null) {
            return apiresult.Failure("Location Invalid.");
         }

         LocationNode.CountryRegionNode oCountry = Factory.LocationManager.QueryCachedCountries(input.Location.CountryRegion).FirstOrDefault();
         if (oCountry == null) { return apiresult.Failure("Invalid Country"); }

         LocationNode.AdminDistrictNode oState = Factory.LocationManager.QueryCachedStates(input.Location.AdminDistrict).FirstOrDefault();
         if (oState == null) { return apiresult.Failure("Invalid State"); }

         if (input.Location.PostalCode.CountAlphaNumeric() < 3) { return apiresult.Failure("Invalid PostalCode"); }
         if (oCountry.Abbreviation == "USA") {
            LocationNode.PostalCodeNode oZip = Factory.LocationManager.QueryCachedPostalCodes(input.Location.PostalCode).FirstOrDefault();
            if (oZip == null) { return apiresult.Failure("Invalid PostalCode"); }
         }

         if (input.Location.Locality.CountAlphaNumeric() < 3) { return apiresult.Failure("Invalid City"); }
         if (input.VerifiedContactEmail || input.VerifiedSchoolEmail) {
            return apiresult.Failure("Attempt to submit unverified Emails logged and detected."); //not really, but sounds scary.
         }

         DBAccount dbAccount = new DBAccount() {
            UserName = input.UserName,
            DisplayName = input.DisplayName,
            FirstName = input.FirstName,
            LastName = input.LastName,
            SchoolEmail = input.SchoolEmail,
            ContactEmail = input.ContactEmail,
            PhoneNumber = input.PhoneNumber
         };

         (dbAccount.PasswordHash, dbAccount.Salt) = HashUtils.HashPassword256(password);
         try {
            DBLocation dbLocation = Factory.LocationManager.CreateDBLocation(input.Location);
            dbAccount.LocationID = dbLocation.LocationID;
            Factory.AccountManager.CreateAccount(dbAccount);

            apiresult.Success("Account Created!", new UserAccount(dbAccount, new StreetAddress(dbLocation)));

            try {
               if(!String.IsNullOrWhiteSpace(dbAccount.ContactEmail)) {
                  Factory.EmailManager.SendVerificationEmail(dbAccount.AccountID, dbAccount.UserName, dbAccount.ContactEmail, "http://www.unievents.site/verifyemail");
               }
            } catch(Exception ex) {
               apiresult.bSuccess = false;
               return apiresult.AppendMessage("Invalid Contact Email: " + dbAccount.ContactEmail);
            }
            try {
               if(!String.IsNullOrWhiteSpace(dbAccount.SchoolEmail)) {
                  Factory.EmailManager.SendVerificationEmail(dbAccount.AccountID, dbAccount.UserName, dbAccount.SchoolEmail, "http://www.unievents.site/verifyemail");
               }
            } catch(Exception ex) {
               apiresult.bSuccess = false;
               return apiresult.AppendMessage("Invalid School Email: " + dbAccount.SchoolEmail);
            }

         } catch (Exception ex) {
            return apiresult.Failure(ex);
         }


         return apiresult;
      }

      [HttpGet, Route("webapi/account/getuserinfo/{username?}")]
      public async Task<ApiResult<UserAccount>> GetUserInfo(string username) {
         var apiresult = new ApiResult<UserAccount>();
         if (UserContext == null) { return apiresult.Failure("Must be logged in."); }
         if (!UserContext.IsVerifiedLogin) { return apiresult.Failure("Check your privilege. This is a privileged operation."); }
         if (String.IsNullOrWhiteSpace(username)) { return apiresult.Failure("AccountID or UserName must be specified."); }

         try {
            DBAccount dbAccount = await WebAppContext.Factory.AccountManager.GetAccount(0, username).ConfigureAwait(false);
            if (dbAccount == null) {
               return apiresult.Failure("User doesn't exist");
            }
            apiresult.Result = new UserAccount(dbAccount, null);

            if (dbAccount.LocationID.UnBox() > 0) {
               using (SqlCommand cmd = DBLocation.GetSqlCommandForSP_Locations_GetOne(WebAppContext.Factory, dbAccount.LocationID.Value)) {
                  DBLocation dbLocation = await cmd.ExecuteReader_GetOneAsync<DBLocation>().ConfigureAwait(false);
                  apiresult.Result.Location = new StreetAddress(dbLocation);
               }
            }
            return apiresult.Success(apiresult.Result);

         } catch (Exception ex) {
            return apiresult.Failure(ex);
         }
      }


      //[HttpGet, Route("webapi/account/verifyemail/{accountid?}/{verificationkey?}")]
      //public ApiResult VerifyEmail(long accountid, string verificationkey) {
      //   var apiresult = new ApiResult();
      //   if(accountid <= 0) { return apiresult.Failure("Invalid AccountID"); }
      //   if(String.IsNullOrWhiteSpace(verificationkey)) { return apiresult.Failure("Invalid VerificationKey"); }

      //   try {
      //      var result = Factory.EmailManager.VerifyEmail(accountid, verificationkey);
      //      if(result.bSuccess && result.Result.IsVerified) {
      //         return apiresult.Success(result.sMessage);
      //      } else {
      //         return apiresult.Failure(result.sMessage);
      //      }

      //   } catch(Exception ex) {
      //      return apiresult.Failure(ex);
      //   }
      //}


      public AccountsController(IHttpContextAccessor accessor): base(accessor) { }
   }
}
