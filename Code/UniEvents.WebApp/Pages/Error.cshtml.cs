using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UniEvents.WebApp.Pages {
   public class ErrorModel : WebAppPageModel {
      public string RequestId { get; set; }

      public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

      public ErrorModel(IHttpContextAccessor accessor) : base(accessor) { }

      public void OnGet() {
         RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
      }
   }
}
