using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using UniEvents.WebApp;
using UniEvents.Core;
using ApiModels = UniEvents.Models.ApiModels;
using DBModels = UniEvents.Models.DBModels;


namespace UniEvents.WebAPI.Controllers {

   [Produces("application/json")]
   public class LocationsController : Controller {

      [HttpGet, Route("webapi/locations/search/{ParentLocationID?}/{Name?}/{AddressLine?}/{Locality?}/{AdminDistrict?}/{PostalCode?}/{Description?}")]
      public IEnumerable<ApiModels.StreetAddress> Search(long? ParentLocationID = null,
                                                string Name = null,
                                                string AddressLine = null,
                                                string Locality = null,
                                                string AdminDistrict = null,
                                                string PostalCode = null,
                                                string Description = null) {
         var ls = DBModels.DBLocation.SP_Locations_Search(Program.CoreContext, ParentLocationID:ParentLocationID, Name:Name, AddressLine:AddressLine, Locality:Locality, AdminDistrict:AdminDistrict, PostalCode:PostalCode, Description:Description);
         foreach (DBModels.DBLocation loc in ls) {
            yield return ZMBA.Common.CopyPropsShallow(new ApiModels.StreetAddress(), loc);
         }
      }


      [HttpPost, Route("webapi/locations/create")]
      public async Task<ApiModels.StreetAddress> Create(ApiModels.StreetAddress address) {
         DBModels.DBLocation model = ZMBA.Common.CopyPropsShallow(new DBModels.DBLocation(), address);
         ApiModels.StreetAddress result = new ApiModels.StreetAddress();
         try {
            result.Success = await DBModels.DBLocation.SP_Location_CreateAsync(Program.CoreContext, model).ConfigureAwait(false); ;
            if (result.Success) {
               ZMBA.Common.CopyPropsShallow(result, model);
            }
         } catch (Exception ex) {
            result.Message = ex.Message;
         }
         return result;
      }


   }
}
