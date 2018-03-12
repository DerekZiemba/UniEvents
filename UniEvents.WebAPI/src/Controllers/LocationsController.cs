using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using UniEvents.Core;
using UniEvents.Core.ApiModels;


namespace UniEvents.WebAPI.Controllers {

	[Produces("application/json")]
	[Route("Locations")]
	public class LocationsController : Controller {

		[HttpGet, Route("search/{ParentLocationID?}/{Name?}/{AddressLine?}/{Locality?}/{AdminDistrict?}/{PostalCode?}/{Description?}")]
		public IEnumerable<StreetAddress> Search(long? ParentLocationID = null,
																string Name = null,
																string AddressLine = null,
																string Locality = null,
																string AdminDistrict = null,
																string PostalCode = null,
																string Description = null) {
			var ls = StoredProcedures.Location.Search(ParentLocationID:ParentLocationID, Name:Name, AddressLine:AddressLine, Locality:Locality, AdminDistrict:AdminDistrict, PostalCode:PostalCode, Description:Description);
			foreach (Core.DBModels.DBLocation loc in ls) {
				yield return ZMBA.Common.CopyPropsShallow(new Core.ApiModels.StreetAddress(), loc);
			}
		}


		// POST: api/Locations
		[HttpPost]
		public void Post([FromBody]string value) {
		}

		// PUT: api/Locations/5
		[HttpPut("{id}")]
		public void Put(int id, [FromBody]string value) {
		}

		// DELETE: api/ApiWithActions/5
		[HttpDelete("{id}")]
		public void Delete(int id) {
		}
	}
}
