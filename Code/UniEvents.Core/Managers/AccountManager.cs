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
      private readonly CoreContext Ctx;

		internal AccountManager(CoreContext ctx) {
         this.Ctx = ctx;
		}

      public async Task<bool> CheckPrivilege(string username, string apikey) {
         if (!username.IsNullOrWhitespace() && !apikey.IsNullOrWhitespace()) {
            try {
               DBModels.DBLogin dbLogin = await DBModels.DBLogin.SP_Account_Login_GetAsync(Ctx, username, apikey).ConfigureAwait(false);
               return dbLogin != null && Crypto.VerifyHashMatch(apikey, dbLogin.UserName, dbLogin.APIKeyHash);
            } catch (Exception ex) { }
         }
         return false;
      }


      public async Task<ApiResult<UserAccount>> CreateUser(UserAccount user, string password) {
         if (user == null) {
            return new ApiResult<UserAccount>(false, "Account Null.");
         }
         if (password.IsNullOrWhitespace()) {
            return new ApiResult<UserAccount>(false, "Invalid Password.");
         }
         if (user.UserName.IsNullOrWhitespace()) {
            return new ApiResult<UserAccount>(false, "Invalid UserName.");
         }
         if (user.Location == null) {
            return new ApiResult<UserAccount>(false, "Location Invalid.");
         }
         if (user.Location.CountryRegion.IsNullOrWhitespace()) {
            return new ApiResult<UserAccount>(false, "Invalid CountryRegion.");
         }
         if (user.Location.AdminDistrict.IsNullOrWhitespace()) {
            return new ApiResult<UserAccount>(false, "Invalid State.");
         }
         if (user.Location.Locality.IsNullOrWhitespace()) {
            return new ApiResult<UserAccount>(false, "Invalid City");
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

            return new ApiResult<UserAccount>(true, "Account Created!", new UserAccount(dbAccount, new StreetAddress(dbLocation)));

         } catch (Exception ex) {
            //TODO: If this happens, we need to roll back changes to database. Such as if the location was successfully added but the account was not or vice versa.
            return new ApiResult<UserAccount>(false, ex.Message + " \n " + ex.InnerException?.Message);
         }
      }


   }
}
