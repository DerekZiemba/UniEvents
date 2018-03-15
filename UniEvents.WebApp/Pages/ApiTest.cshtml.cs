﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UniEvents.WebApp.Pages {
   public class ApiTestModel : PageModel {

      public IApiDescriptionGroupCollectionProvider ApiExplorer { get; set; }

      public ApiTestModel(IApiDescriptionGroupCollectionProvider apiExplorer) {
         ApiExplorer = apiExplorer;
      }


      public void OnGet() {

      }
   }
}