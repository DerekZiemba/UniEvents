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

		// GET: api/Locations/5
		[HttpGet(Name = "Get")]
		public IEnumerable<StreetAddress> Get() {
			var ls = StoredProcedures.Location.Search();
			foreach (var loc in ls) {
				yield return new Core.ApiModels.StreetAddress(loc);
			}
		}

		//[HttpGet("{plid}{name}{address}{city}{state}{zip}{desc}", Name = "Search")]
		[HttpGet, Route("search/{plid?}/{name?}/{address?}/{city?}/{state?}/{zip?}/{desc?}")]
		public IEnumerable<StreetAddress> Search(long? plid = null,
																string name = null,
																string address = null,
																string city = null,
																string state = null,
																string zip = null,
																string desc = null) {
			var ls = StoredProcedures.Location.Search(ParentLocationID:plid, Name:name, AddressLine:address, Locality:city, AdminDistrict:state, PostalCode:zip, Description:desc);
			foreach (var loc in ls) {
				yield return new Core.ApiModels.StreetAddress(loc);
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
