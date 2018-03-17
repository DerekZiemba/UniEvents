using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


using static ZMBA.Common;
using UniEvents.Core;
using UniEvents.Models.ApiModels;
using ApiModels = UniEvents.Models.ApiModels;
using DBModels = UniEvents.Models.DBModels;



namespace UniEvents.Managers {

	public class AccountManager {
      private CoreContext Ctx;

		internal AccountManager(CoreContext ctx) {
         this.Ctx = ctx;
		}

      public async Task<ApiResult<UserAccount>> GetUser(string username, bool bIncludeLocation) {
         try {
            DBModels.DBAccount dbAccount = await DBModels.DBAccount.SP_Account_GetAsync(Ctx, 0, username).ConfigureAwait(false);
            if (dbAccount == null) {
               return new ApiResult<UserAccount>(false, "User doesn't Exist");
            }
            if (bIncludeLocation && dbAccount.LocationID.HasValue) {
               DBModels.DBLocation dbLocation = await DBModels.DBLocation.SP_Location_GetAsync(Ctx, dbAccount.LocationID.Value);
               return new ApiResult<UserAccount>(false, "", new UserAccount(dbAccount, new StreetAddress(dbLocation)));
            }
            return new ApiResult<UserAccount>(false, "", new UserAccount(dbAccount, null));
         } catch (Exception ex) {
            return new ApiResult<UserAccount>(false, ex.Message + " \n " + ex.InnerException?.Message);
         }
      }

      public async Task<ApiResult<AccountLogin>> Login(string username, string password) {
         if (username.IsNullOrWhitespace() || password.IsNullOrWhitespace()) {
            return new ApiResult<AccountLogin>(false, "Invalid Username/Password");
         }
         try {
            DBModels.DBAccount dbAccount = await DBModels.DBAccount.SP_Account_GetAsync(Ctx, 0, username).ConfigureAwait(false);
            if (dbAccount == null) {
               return new ApiResult<AccountLogin>(false, "Account does not exist.");
            }

            if (!Crypto.VerifyHashMatch(password, dbAccount.Salt, dbAccount.PasswordHash)) {
               return new ApiResult<AccountLogin>(false, "Invalid_Password");
            }

            DBModels.DBLogin dbLogin = new DBModels.DBLogin(){ UserName=username };
            (dbLogin.APIKeyHash, dbLogin.APIKey) = Crypto.CreateAPIKey256(username);

            if (!await DBModels.DBLogin.SP_Account_LoginAsync(Ctx, dbLogin).ConfigureAwait(false)) {
               return new ApiResult<AccountLogin>(false, "Login_Failed");
            } 

            return new ApiResult<AccountLogin>(true, "", new AccountLogin(dbLogin));

         } catch (Exception ex) {
            return new ApiResult<AccountLogin>(false, ex.Message + " \n " + ex.InnerException?.Message);
         }
      }


      public async Task<ApiResult<VerifiedLogin>> VerifyLogin(string username, string apikey) {
         var result = new ApiResult<VerifiedLogin>();
         try {
            DBModels.DBLogin dbLogin = await DBModels.DBLogin.SP_Account_Login_GetAsync(Ctx, username, apikey).ConfigureAwait(false);
            result.Result = new VerifiedLogin() {
               IsLoggedIn = dbLogin != null && Crypto.VerifyHashMatch(apikey, dbLogin.UserName, dbLogin.APIKeyHash),
               LoginDate = dbLogin?.LoginDate
            };
            result.Success = true;
         } catch (Exception ex) {
            result.Message = ex.Message + " \n " + ex.InnerException?.Message;
         }
         return result;
      }

      /// <param name="username"></param>
      /// <param name="keypass">If APIKey, logs out device.  If password, logout everywhere.</param>
      public async Task<ApiResult> Logout(string username, string keypass) {
         if(username.IsNullOrWhitespace() || keypass.IsNullOrWhitespace()) {
            return new ApiResult(false, "Invalid Username/KeyPass");
         }
         try {
            if(await DBModels.DBLogin.SP_Account_LogoutAsync(Ctx, username, keypass).ConfigureAwait(false)) {
               return new ApiResult(true, "Logged out APIKey: " + keypass);
            }

            DBModels.DBAccount dbAccount = await DBModels.DBAccount.SP_Account_GetAsync(Ctx, 0, username).ConfigureAwait(false);
            if (dbAccount == null) {
               return new ApiResult(false, "Account does not exist.");
            } 
            if (!Crypto.VerifyHashMatch(keypass, dbAccount.Salt, dbAccount.PasswordHash)) {
               return new ApiResult(false, "Invalid_Password");
            }

            if (await DBModels.DBLogin.SP_Account_LogoutAsync(Ctx, username, null).ConfigureAwait(false)) {
               return new ApiResult(true, "User logged out everywhere.");
            }

            return new ApiResult(false, "Logout Failed.");

         } catch (Exception ex) {
            return new ApiResult(false, ex.Message + " \n " + ex.InnerException?.Message);
         }
      }


      public async Task<ApiResult<UserAccount>> CreateUser(UserAccount user, string password) {
         if (user == null) {
            return new ApiResult<UserAccount>(false, "Account Null.");
         }
         if (user.Location == null || !user.Location.IsValid()) {
            return new ApiResult<UserAccount>(false, "Location not set or invalid.");
         }
         if (user.VerifiedContactEmail || user.VerifiedSchoolEmail) {
            return new ApiResult<UserAccount>(false, "Attempt to submit unverified Emails logged and detected."); //not really, but sounds scary.
         }

         DBModels.DBLocation dbLocation = new DBModels.DBLocation(user.Location);
         DBModels.DBAccount dbAccount = new DBModels.DBAccount() {
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
            if (!await DBModels.DBLocation.SP_Location_CreateAsync(Ctx, dbLocation)) {
               return new ApiResult<UserAccount>(false, "Failed to create location.");
            }

            dbAccount.LocationID = dbLocation.LocationID;
            if (!await DBModels.DBAccount.SP_Account_CreateAsync(Ctx, dbAccount)) {
               return new ApiResult<UserAccount>(false, "Failed to create account.");
            }

            return new ApiResult<UserAccount>(false, "", new UserAccount(dbAccount, new StreetAddress(dbLocation)));

         } catch (Exception ex) {
            //TODO: If this happens, we need to roll back changes to database. Such as if the location was successfully added but the account was not or vice versa.
            return new ApiResult<UserAccount>(false, ex.Message + " \n " + ex.InnerException?.Message);
         }
      }





   }
}
