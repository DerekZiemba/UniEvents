using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using UniEvents.WebApp;
using UniEvents.Core;
using UniEvents.Models.ApiModels;
using UniEvents.Models.DBModels;
using ZMBA;


namespace UniEvents.WebAPI.Controllers {

   [Produces("application/json")]
   [ApiExplorerSettings(IgnoreApi = false)]
   public class RSVPController : WebAPIController {



      [HttpPost, Route("webapi/rsvps")]
      public ApiResult<RSVPType[]> GetRsvpTypes() {
         var apiresult = new ApiResult<RSVPType[]>();

         try {
            return apiresult.Success(Factory.RSVPTypeManager.RSVPTypes.ToArray());
         } catch (Exception ex) { return apiresult.Failure(ex); }

      }


      [HttpGet, Route("webapi/rsvp/toevent/{eventid?}/{rsvpTypeId?}/{rsvpName?}")]
      public async Task<ApiResult> RsvpToEvent(long eventid, int? rsvpTypeId, string rsvpName) {
         var apiresult = new ApiResult();
         if (UserContext == null) { return apiresult.Failure("Must be logged in."); }
         if (!UserContext.IsVerifiedLogin) { return apiresult.Failure("Insufficient account permissions."); }
         if (eventid <= 0) { return apiresult.Failure("Invalid Event"); }
         if (rsvpTypeId.UnBox() <= 0 && string.IsNullOrWhiteSpace(rsvpName)) { return apiresult.Failure("Must specifify an rsvpTypeId or rsvpName"); }

         RSVPType rsvp = rsvpTypeId.UnBox() > 0 ? Factory.RSVPTypeManager[rsvpTypeId.UnBox()] : Factory.RSVPTypeManager[rsvpName];
         if(rsvp == null) {
            return apiresult.Failure("RSVPType does not exists");
         }
         try {
            await Factory.RSVPTypeManager.AddOrUpdateRSVPToEvent(UserContext.AccountID, eventid, rsvp.RSVPTypeID);
            return apiresult.Success("Success");
         } catch (Exception ex) { return apiresult.Failure(ex); }

      }

      
      [HttpGet, Route("webapi/rsvp/getrsvp/{eventid?}")]
      public ApiResult<RSVPType> GetRsvpStatus(long eventid) {
         var apiresult = new ApiResult<RSVPType>();
         if (UserContext == null) { return apiresult.Failure("Must be logged in."); }
         if (!UserContext.IsVerifiedLogin) { return apiresult.Failure("Insufficient account permissions."); }
         if (eventid <= 0) { return apiresult.Failure("Invalid Event"); }

         try {
            var status = Factory.RSVPTypeManager.GetUsersRSVP(UserContext.AccountID, eventid);
            if(status != null) {
               return apiresult.Success(status);
            }
            return apiresult.Failure("");
         } catch (Exception ex) { return apiresult.Failure(ex); }

      }


      public RSVPController(IHttpContextAccessor accessor) : base(accessor) { }
   }
}
