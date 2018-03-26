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
   [ApiExplorerSettings(IgnoreApi = false, GroupName = nameof(LocationsController))]
   public class LocationsController : WebAppController {

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
            var models = DBModels.DBLocation.SP_Locations_Search(WebAppContext.CoreContext, ParentLocationID, Name, AddressLine, Locality, AdminDistrict, PostalCode, Description);
            foreach (DBModels.DBLocation loc in models) { 
               apiresult.Result.Add(new StreetAddress(loc));
            }
            return apiresult.Win(apiresult.Result);

         } catch (Exception ex) {
            return apiresult.Failure(ex);
         }
      }


      [HttpPost, Route("webapi/locations/create/{username?}/{apikey?}")]
      public async  Task<ApiResult<StreetAddress>> Create(string username, string apikey, StreetAddress address) {
         var apiresult = new ApiResult<StreetAddress>();
         if (UserContext == null) { return apiresult.Failure("Must be logged in."); }
         if (!UserContext.IsVerifiedLogin) { return apiresult.Failure("Insufficient account permissions."); }
         return await this.LocationManager().CreateLocation(address);         
      }


      public LocationsController(IHttpContextAccessor accessor): base(accessor) { }
   }
}
