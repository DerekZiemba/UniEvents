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
   [ApiExplorerSettings(IgnoreApi = false, GroupName = nameof(CommonController))]
   public class CommonController : WebAppController {

      [HttpGet, Route("webapi/tags/search/{id?}/{name?}/{description?}")]
      public ApiResult<DBModels.DBTag[]> TagSearch(int? id = null,
                                                   string name = null,
                                                   string description = null) {


         var apiresult = new ApiResult<DBModels.DBTag[]>();
         if (UserContext == null ) { return apiresult.Failure("Must be logged in.");  }
         if (!UserContext.IsVerifiedLogin) { return apiresult.Failure("Insufficient account permissions."); }

         try {
            apiresult.Win(DBModels.DBTag.SP_Tags_Search(WebAppContext.CoreContext, id, name, description).ToArray());
            return apiresult;
         } catch (Exception ex) { return apiresult.Failure(ex); }    
      }


      [HttpPost, Route("webapi/rsvps")]
      public async  Task<ApiResult<DBModels.DBRSVPType[]>> GetRsvpTypes() {
         var apiresult = new ApiResult<DBModels.DBRSVPType[]>();

         try {
            apiresult.Win(DBModels.DBRSVPType.SP_RSVPTypes_Get(WebAppContext.CoreContext).ToArray());
            return apiresult;
         } catch (Exception ex) { return apiresult.Failure(ex); }
   
      }


      public CommonController(IHttpContextAccessor accessor): base(accessor) { }
   }
}
