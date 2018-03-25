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
         return this.LocationManager().SearchLocations(ParentLocationID: ParentLocationID, Name: Name, AddressLine: AddressLine, Locality: Locality, AdminDistrict: AdminDistrict, PostalCode: PostalCode, Description: Description);
      }


      [HttpPost, Route("webapi/locations/create/{username?}/{apikey?}")]
      public async  Task<ApiResult<StreetAddress>> Create(string username, string apikey, StreetAddress address) {
         if (!await this.AccountManager().CheckPrivilege(username, apikey)) {
            return new ApiResult<StreetAddress>(false, "Check your privilege. This is a privileged operation.");
         }
         return await this.LocationManager().CreateLocation(address);         
      }


      public LocationsController(IHttpContextAccessor accessor): base(accessor) { }
   }
}
