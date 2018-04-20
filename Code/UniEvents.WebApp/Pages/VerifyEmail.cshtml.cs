using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniEvents.Models.ApiModels;
using UniEvents.Models.DBModels;


namespace UniEvents.WebApp.Pages {
   public class VerifyEmailModel : WebAppPageModel {

      public bool SuccessfullyVerified { get; set; }
      public string PageMessage { get; set; }
      public string PageErrorMessage { get; set; }


      public void OnGet() {
         var qs = this.Request.Query;
         try {
            long AccountID = long.Parse(qs["id"]);
            string VerificationKey = qs["key"];
            ApiResult<DBEmailVerification> result = Factory.EmailManager.VerifyEmail(AccountID, VerificationKey);

            SuccessfullyVerified = result.bSuccess && result.Result.IsVerified;

            if(SuccessfullyVerified) {
               PageMessage =  "Email Successfully Verified: " + result.Result.Email;
            } else {
               PageMessage = "Could not verify.";
            }

            PageErrorMessage = result.sMessage;

         } catch(Exception ex) {
            PageErrorMessage = ex.ToString() + " \r\n | " + ex.InnerException?.ToString();
         }

      }

      public VerifyEmailModel(IHttpContextAccessor accessor): base(accessor) { }

   }
}