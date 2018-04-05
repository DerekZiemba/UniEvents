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
   public class EventTypesController : WebAPIController {


      [HttpGet, Route("webapi/eventtypes/get/{id?}/{name?}")]
      public ApiResult<DBModels.DBEventType> EventTypesGet(long? id = null, string name = null) {
         var apiresult = new ApiResult<DBModels.DBEventType>();
         try {
            if (id.HasValue) {
               return apiresult.Success(Factory.EventTypeManager[id.Value]);
            } else {
               return apiresult.Success(Factory.EventTypeManager[name]);
            }
         } catch (Exception ex) { return apiresult.Failure(ex); }
      }


      [HttpPost, Route("webapi/eventtypes/create")]
      public ApiResult<DBModels.DBEventType> EventTypesCreate(string name, string description) {
         var apiresult = new ApiResult<DBModels.DBEventType>();
         if (UserContext == null) { return apiresult.Failure("Must be logged in."); }
         if (!UserContext.IsVerifiedLogin) { return apiresult.Failure("Insufficient account permissions."); }

         try {
            var tag = WebAppContext.Factory.EventTypeManager.Create(name, description);
            if (tag is null) {
               return apiresult.Failure("Failed to create EventType.");
            }
            return apiresult.Success(tag);
         } catch (Exception ex) { return apiresult.Failure(ex); }

      }


      public EventTypesController(IHttpContextAccessor accessor): base(accessor) { }
   }
}