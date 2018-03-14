using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Web.Http;

using UniEvents.Core;
using UniEvents.Core.ApiModels;


namespace UniEvents.WebAPI.Controllers {

	public class LocationsController : ApiController {

		[HttpGet, Route("locations/search/{ParentLocationID?}/{Name?}/{AddressLine?}/{Locality?}/{AdminDistrict?}/{PostalCode?}/{Description?}")]
		public IEnumerable<StreetAddress> Search(long? ParentLocationID = null,
																string Name = null,
																string AddressLine = null,
																string Locality = null,
																string AdminDistrict = null,
																string PostalCode = null,
																string Description = null) {
			var ls = Core.DBModels.DBLocation.SP_Locations_Search(WebApiApplication.CoreContext, ParentLocationID:ParentLocationID, Name:Name, AddressLine:AddressLine, Locality:Locality, AdminDistrict:AdminDistrict, PostalCode:PostalCode, Description:Description);
			foreach (Core.DBModels.DBLocation loc in ls) {
				yield return ZMBA.Common.CopyPropsShallow(new Core.ApiModels.StreetAddress(), loc);
			}
		}


		[HttpPost, Route("locations/create")]
		public async Task<StreetAddress> Create(StreetAddress address) {
         Core.DBModels.DBLocation model = ZMBA.Common.CopyPropsShallow(new Core.DBModels.DBLocation(), address);
         StreetAddress result = new StreetAddress();
         try {
            result.Success = await Core.DBModels.DBLocation.SP_Location_CreateAsync(WebApiApplication.CoreContext, model).ConfigureAwait(false); ;
            if (result.Success) {
               ZMBA.Common.CopyPropsShallow(result, model);
            }            
         } catch(Exception ex) {
            result.Message = ex.Message;
         }
         return result;
		}


	}
}
