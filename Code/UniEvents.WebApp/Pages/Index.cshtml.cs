using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UniEvents.WebApp.Pages {
	public class IndexModel : WebAppPageModel {

      public int[] EventFeed = { 1,2,3,4,5,6,7,8,9,10};  // we can use something else instead of array to store data

      public void OnGet() {

      }

      public IndexModel(IHttpContextAccessor accessor): base(accessor) { }
   }
}
