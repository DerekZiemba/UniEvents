using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using UniEvents.WebApp;
using UniEvents.Core;
using UniEvents.Models.ApiModels;
using DBModels = UniEvents.Models.DBModels;


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


      public RSVPController(IHttpContextAccessor accessor): base(accessor) { }
   }
}
