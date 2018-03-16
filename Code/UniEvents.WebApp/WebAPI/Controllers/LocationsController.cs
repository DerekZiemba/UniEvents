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
   public class LocationsController : Controller {

      [HttpGet, Route("webapi/locations/search/{ParentLocationID?}/{Name?}/{AddressLine?}/{Locality?}/{AdminDistrict?}/{PostalCode?}/{Description?}")]
      public ApiResult<List<StreetAddress>> Search(long? ParentLocationID = null,
                                                                  string Name = null,
                                                                  string AddressLine = null,
                                                                  string Locality = null,
                                                                  string AdminDistrict = null,
                                                                  string PostalCode = null,
                                                                  string Description = null) {

         var result = new ApiResult<List<StreetAddress>>();
         result.Result = new List<StreetAddress>();
         try {
            foreach (DBModels.DBLocation loc in DBModels.DBLocation.SP_Locations_Search(Program.CoreContext, ParentLocationID: ParentLocationID, Name: Name, AddressLine: AddressLine, Locality: Locality, AdminDistrict: AdminDistrict, PostalCode: PostalCode, Description: Description)) {
               result.Result.Add(new StreetAddress(loc));
            }
            result.Success = true;
         } catch(Exception ex) {
            result.Message = ex.Message;
         }
         return result;
      }


      [HttpPost, Route("webapi/locations/create")]
      public ApiResult<StreetAddress> Create(StreetAddress address) {
         var result = new ApiResult<StreetAddress>();
         try {
            DBModels.DBLocation dbLocation = new DBModels.DBLocation(address);
            result.Success = DBModels.DBLocation.SP_Location_Create(Program.CoreContext, dbLocation);
            result.Result = new StreetAddress(dbLocation);
            if (!result.Success) {
               result.Message = "Failed for Unknown Reason";
            }
         } catch (Exception ex) {
            result.Message = ex.Message;
         }
         return result;
      }


   }
}
