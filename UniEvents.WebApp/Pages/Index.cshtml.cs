using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UniEvents.WebApp.Pages {
	public class IndexModel : PageModel {

		public string[] LocationFields { get; private set; }

		public string SearchMethodPath { get; private set; } = "/webapi/locations/search?";

		public void OnGet() {

			LocationFields = new []{ "ParentLocationID", "Name", "AddressLine", "Locality", "AdminDistrict", "PostalCode", "Description?" };

		}
	}
}
