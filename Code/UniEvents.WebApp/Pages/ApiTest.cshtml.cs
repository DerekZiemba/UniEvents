using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UniEvents.WebApp.Pages {
   public class ApiTestModel : WebAppPageModel {

      public ApiTestModel(IHttpContextAccessor accessor) : base(accessor) { }

      public void OnGet() {

      }

     
   }
}