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
   public class TagsController : WebAPIController {

      [HttpGet, Route("webapi/tags/search/{id?}/{name?}/{description?}")]
      public ApiResult<IEnumerable<DBModels.DBTag>> TagSearch(long? id = null,
                                                               string name = null,
                                                               string description = null) {


         var apiresult = new ApiResult<IEnumerable<DBModels.DBTag>>();
         if (UserContext == null) { return apiresult.Failure("Must be logged in."); }
         if (!UserContext.IsVerifiedLogin) { return apiresult.Failure("Insufficient account permissions."); }

         try {
            if (id.HasValue) {
               return apiresult.Success(new[] { Factory.TagManager[id.Value] });
            } else {
               return apiresult.Success(Factory.TagManager.SearchCached(name, description));
            }
         } catch (Exception ex) { return apiresult.Failure(ex); }
      }

      [HttpGet, Route("webapi/tags/query/{query?}")]
      public ApiResult<IEnumerable<DBModels.DBTag>> TagQuery(string query) {
         var apiresult = new ApiResult<IEnumerable<DBModels.DBTag>>();
         if (UserContext == null) { return apiresult.Failure("Must be logged in."); }
         if (!UserContext.IsVerifiedLogin) { return apiresult.Failure("Insufficient account permissions."); }

         try {
            return apiresult.Success(Factory.TagManager.QueryCached(query));
         } catch (Exception ex) { return apiresult.Failure(ex); }
      }


      [HttpPost, Route("webapi/tags/create")]
      public ApiResult<DBModels.DBTag> TagCreate(string name, string description) {
         var apiresult = new ApiResult<DBModels.DBTag>();
         if (UserContext == null) { return apiresult.Failure("Must be logged in."); }
         if (!UserContext.IsVerifiedLogin) { return apiresult.Failure("Insufficient account permissions."); }

         try {
            var tag = WebAppContext.Factory.TagManager.Create(name, description);
            if (tag is null) {
               return apiresult.Failure("Failed to create tag.");
            }
            return apiresult.Success(tag);
         } catch (Exception ex) { return apiresult.Failure(ex); }

      }

      public TagsController(IHttpContextAccessor accessor): base(accessor) { }
   }
}