﻿using System;
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
         ApiResult<AccountLogin> result = new ApiResult<AccountLogin>();

         try {
            DBModels.DBAccount dbAccount = await DBModels.DBAccount.SP_Account_GetAsync(Program.CoreContext, 0, username).ConfigureAwait(false);
            if(dbAccount == null) {
               result.Message = "Account does not exist.";
            } else {
               if (Crypto.VerifyHashMatch(password, dbAccount.Salt, dbAccount.PasswordHash)) {
                  DBModels.DBLogin dbLogin = new DBModels.DBLogin(){ UserName=username };
                  (dbLogin.APIKeyHash, dbLogin.APIKey) = Crypto.CreateAPIKey256(username);
                  if (await DBModels.DBLogin.SP_Account_LoginAsync(Program.CoreContext, dbLogin).ConfigureAwait(false)) {
                     result.Result = new AccountLogin(dbLogin); ;
                     result.Success = true;
                  } else {
                     result.Message = "Login Failed";
                  }
               } else {
                  result.Message = "Invalid_Password";
               }
            }
         } catch(Exception ex) {
            result.Message = ex.Message + " \n " + ex.InnerException?.Message;
         }

         return result;
      }


      [HttpGet, Route("webapi/account/verifylogin/{username?}/{apikey?}")]
      public async Task<ApiResult<VerifiedLogin>> VerifyLogin(string username, string apikey) {
         var result = new ApiResult<VerifiedLogin>();
         try {
            DBModels.DBLogin dbLogin = await DBModels.DBLogin.SP_Account_Login_GetAsync(Program.CoreContext, username, apikey).ConfigureAwait(false);
            VerifiedLogin verified = new VerifiedLogin(){
               IsLoggedIn= dbLogin != null && Crypto.VerifyHashMatch(apikey, dbLogin.UserName, dbLogin.APIKeyHash),
               LoginDate = dbLogin?.LoginDate
            };
            result.Result = verified;
            result.Success = true;
            
         } catch(Exception ex) {
            result.Message = ex.Message + " \n " + ex.InnerException?.Message;
         }
         return result;
      }


      [HttpPost, Route("webapi/account/create/{password?}")]
      public ApiResult<UserAccount> Create(UserAccount user, string password) {
         if (user == null) {
            return new ApiResult<UserAccount>(false, "Account Null.");
         }
         if (user.Location == null || !user.Location.IsValid()) {
            return new ApiResult<UserAccount>(false, "Location not set or invalid.");
         }
         if(user.VerifiedContactEmail || user.VerifiedSchoolEmail) {
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
            if(!DBModels.DBLocation.SP_Location_Create(Program.CoreContext, dbLocation)) {
               return new ApiResult<UserAccount>(false, "Failed to create location.");
            } 

            dbAccount.LocationID = dbLocation.LocationID;
            if (!DBModels.DBAccount.SP_Account_Create(Program.CoreContext, dbAccount)) {
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