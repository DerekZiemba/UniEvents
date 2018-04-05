using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
   public class AutoCompleteController : WebAPIController {


      [HttpGet, Route("webapi/autocomplete/locations/{query?}")]
      public ApiResult<IEnumerable<Models.LocationNode>> Locations(string query) {
         var apiresult = new ApiResult<IEnumerable<Models.LocationNode>>();
         try {
            apiresult.Success(Factory.LocationManager.QueryCachedLocations(query));
            return apiresult;
         } catch (Exception ex) {
            return apiresult.Failure(ex);
         }
      }

      [HttpGet, Route("webapi/autocomplete/countries/{query?}")]
      public ApiResult<IEnumerable<Models.LocationNode>> Countries(string query) {
         var apiresult = new ApiResult<IEnumerable<Models.LocationNode>>();
         try {
            apiresult.Success(Factory.LocationManager.QueryCachedCountries(query));
            return apiresult;
         } catch (Exception ex) {
            return apiresult.Failure(ex);
         }
      }

      [HttpGet, Route("webapi/autocomplete/states/{query?}")]
      public ApiResult<IEnumerable<Models.LocationNode>> States(string query) {
         var apiresult = new ApiResult<IEnumerable<Models.LocationNode>>();
         try {
            apiresult.Success(Factory.LocationManager.QueryCachedStates(query));
            return apiresult;
         } catch (Exception ex) {
            return apiresult.Failure(ex);
         }
      }

      [HttpGet, Route("webapi/autocomplete/cities/{query?}")]
      public ApiResult<IEnumerable<Models.LocationNode>> Cities(string query) {
         var apiresult = new ApiResult<IEnumerable<Models.LocationNode>>();
         try {
            apiresult.Success(Factory.LocationManager.QueryCachedCities(query));
            return apiresult;
         } catch (Exception ex) {
            return apiresult.Failure(ex);
         }
      }

      [HttpGet, Route("webapi/autocomplete/postalcodes/{query?}")]
      public ApiResult<IEnumerable<Models.LocationNode>> PostalCodes(string query) {
         var apiresult = new ApiResult<IEnumerable<Models.LocationNode>>();
         try {
            apiresult.Success(Factory.LocationManager.QueryCachedPostalCodes(query));
            return apiresult;
         } catch (Exception ex) {
            return apiresult.Failure(ex);
         }
      }



      [HttpGet, Route("webapi/autocomplete/tags/{query?}")]
      public ApiResult<IEnumerable<DBTag>> Tags(string query) {
         var apiresult = new ApiResult<IEnumerable<DBTag>>();
         try {
            return apiresult.Success(Factory.TagManager.QueryCached(query));
         } catch (Exception ex) { return apiresult.Failure(ex); }
      }


      [HttpGet, Route("webapi/autocomplete/eventtypes/{query?}")]
      public ApiResult<IEnumerable<DBEventType>> EventTypes(string query) {
         var apiresult = new ApiResult<IEnumerable<DBEventType>>();
         try {
            return apiresult.Success(Factory.EventTypeManager.QueryCached(query));
         } catch (Exception ex) { return apiresult.Failure(ex); }
      }



      public AutoCompleteController(IHttpContextAccessor accessor): base(accessor) { }
   }
}
