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
   public class CommonController : WebAppController {

      [HttpGet, Route("webapi/tags/search/{id?}/{name?}/{description?}")]
      public ApiResult<DBModels.DBTag[]> TagSearch(long? id = null,
                                                   string name = null,
                                                   string description = null) {


         var apiresult = new ApiResult<DBModels.DBTag[]>();
         if (UserContext == null ) { return apiresult.Failure("Must be logged in.");  }
         if (!UserContext.IsVerifiedLogin) { return apiresult.Failure("Insufficient account permissions."); }

         try {
            apiresult.Win(DBModels.DBTag.SP_Tags_Search(WebAppContext.Factory, id, name, description).ToArray());
            return apiresult;
         } catch (Exception ex) { return apiresult.Failure(ex); }    
      }

      [HttpPost, Route("webapi/tags/create")]
      public ApiResult<DBModels.DBTag> TagCreate(string name, string description) {
         var apiresult = new ApiResult<DBModels.DBTag>();
         if (UserContext == null) { return apiresult.Failure("Must be logged in."); }
         if (!UserContext.IsVerifiedLogin) { return apiresult.Failure("Insufficient account permissions."); }

         try {
            var tag = DBModels.DBTag.SP_Tag_Create(WebAppContext.Factory,  name, description);
            if(tag is null) {
               return apiresult.Failure("Failed to create tag.");
            }         
            return apiresult.Win(tag);
         } catch (Exception ex) { return apiresult.Failure(ex); }

      }

      [HttpGet, Route("webapi/eventtypes/search/{id?}/{name?}/{description?}")]
      public ApiResult<DBModels.DBEventType[]> EventTypesSearch(long? id = null,
                                                               string name = null,
                                                               string description = null) {


         var apiresult = new ApiResult<DBModels.DBEventType[]>();
         if (UserContext == null ) { return apiresult.Failure("Must be logged in.");  }
         if (!UserContext.IsVerifiedLogin) { return apiresult.Failure("Insufficient account permissions."); }

         try {
            apiresult.Win(DBModels.DBEventType.SP_EventTypes_Search(WebAppContext.Factory, id, name, description).ToArray());
            return apiresult;
         } catch (Exception ex) { return apiresult.Failure(ex); }    
      }

      [HttpPost, Route("webapi/eventtypes/create")]
      public ApiResult<DBModels.DBEventType> EventTypesCreate(string name, string description) {
         var apiresult = new ApiResult<DBModels.DBEventType>();
         if (UserContext == null) { return apiresult.Failure("Must be logged in."); }
         if (!UserContext.IsVerifiedLogin) { return apiresult.Failure("Insufficient account permissions."); }

         try {
            var tag = DBModels.DBEventType.SP_EventType_Create(WebAppContext.Factory,  name, description);
            if(tag is null) {
               return apiresult.Failure("Failed to create tag.");
            }         
            return apiresult.Win(tag);
         } catch (Exception ex) { return apiresult.Failure(ex); }

      }



      [HttpPost, Route("webapi/rsvps")]
      public ApiResult<DBModels.DBRSVPType[]> GetRsvpTypes() {
         var apiresult = new ApiResult<DBModels.DBRSVPType[]>();

         try {
            apiresult.Win(DBModels.DBRSVPType.SP_RSVPTypes_Get(WebAppContext.Factory).ToArray());
            return apiresult;
         } catch (Exception ex) { return apiresult.Failure(ex); }
   
      }





      public CommonController(IHttpContextAccessor accessor): base(accessor) { }
   }
}
