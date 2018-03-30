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
      public ApiResult<DBModels.DBRSVPType[]> GetRsvpTypes() {
         var apiresult = new ApiResult<DBModels.DBRSVPType[]>();

         try {
            apiresult.Success(DBModels.DBRSVPType.SP_RSVPTypes_Get(WebAppContext.Factory).ToArray());
            return apiresult;
         } catch (Exception ex) { return apiresult.Failure(ex); }
   
      }


      public RSVPController(IHttpContextAccessor accessor): base(accessor) { }
   }
}
