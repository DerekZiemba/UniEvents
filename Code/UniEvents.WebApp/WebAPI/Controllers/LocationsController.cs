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
   [ApiExplorerSettings(IgnoreApi = false, GroupName = nameof(LocationsController))]
   public class LocationsController : WebAPIController {

      [HttpGet, Route("webapi/locations/search/{ParentLocationID?}/{Name?}/{AddressLine?}/{Locality?}/{AdminDistrict?}/{PostalCode?}/{Description?}")]
      public ApiResult<List<StreetAddress>> Search(long? ParentLocationID = null,
                                                         string Name = null,
                                                         string AddressLine = null,
                                                         string Locality = null,
                                                         string AdminDistrict = null,
                                                         string PostalCode = null,
                                                         string Description = null) {

         var apiresult = new ApiResult<List<StreetAddress>>();
         if (UserContext == null) { return apiresult.Failure("Must be logged in."); }
         if (!UserContext.IsVerifiedLogin) { return apiresult.Failure("Insufficient account permissions."); }

         apiresult.Result = new List<StreetAddress>();
         try {
            using(SqlCommand cmd = DBLocation.GetSqlCommandForSP_Locations_Search(WebAppContext.Factory, ParentLocationID, Name, AddressLine, Locality, AdminDistrict, PostalCode, Description)) {
               foreach (var item in cmd.ExecuteReader_GetManyRecords()) {
                  apiresult.Result.Add(new StreetAddress(new DBLocation(item)));
               }
            }
            return apiresult.Success(apiresult.Result);

         } catch (Exception ex) {
            return apiresult.Failure(ex);
         }
      }



      [HttpPost, Route("webapi/locations/create")]
      public async Task<ApiResult<StreetAddress>> Create(StreetAddress address) {
         var apiresult = new ApiResult<StreetAddress>();
         if (UserContext == null) { return apiresult.Failure("Must be logged in."); }
         if (!UserContext.IsVerifiedLogin) { return apiresult.Failure("Insufficient account permissions."); }
         return await Factory.LocationManager.CreateLocation(address);         
      }



      public LocationsController(IHttpContextAccessor accessor): base(accessor) { }
   }
}
