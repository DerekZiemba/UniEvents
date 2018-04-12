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

      [HttpGet, Route("webapi/locations/search")]
      public ApiResult<List<StreetAddress>> Search(StreetAddress address) {

         var apiresult = new ApiResult<List<StreetAddress>>();
         if (UserContext == null) { return apiresult.Failure("Must be logged in."); }
         if (!UserContext.IsVerifiedLogin) { return apiresult.Failure("Insufficient account permissions."); }
       
         try {
            var ls = Factory.LocationManager.SearchDBLocations(address);
            return apiresult.Success(ls.Select(x => new StreetAddress(x)).ToList());

         } catch (Exception ex) {
            return apiresult.Failure(ex);
         }
      }



      [HttpPost, Route("webapi/locations/create")]
      public ApiResult<StreetAddress> Create(StreetAddress address) {
         var apiresult = new ApiResult<StreetAddress>();
         if (UserContext == null) { return apiresult.Failure("Must be logged in."); }
         if (!UserContext.IsVerifiedLogin) { return apiresult.Failure("Insufficient account permissions."); }

         try {
            return apiresult.Success(new StreetAddress(Factory.LocationManager.CreateDBLocation(address)));
         } catch(Exception ex) { return apiresult.Failure(ex); }   
      }



      public LocationsController(IHttpContextAccessor accessor): base(accessor) { }
   }
}
