using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UniEvents.WebApp.Pages {
   public class SignUpModel : WebAppPageModel {
      public void OnGet() {

      }

      public SignUpModel(IHttpContextAccessor accessor): base(accessor) { }
   }
}