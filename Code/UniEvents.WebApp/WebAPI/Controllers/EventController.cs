using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


using UniEvents.WebApp;
using UniEvents.Core;
using UniEvents.Models.ApiModels;
using ApiModels = UniEvents.Models.ApiModels;
using DBModels = UniEvents.Models.DBModels;
using static ZMBA.Common;


namespace UniEvents.WebAPI.Controllers {

   [Produces("application/json")]
   [ApiExplorerSettings(IgnoreApi = false, GroupName = nameof(EventController))]
   public class EventController : WebAppController {

      public IActionResult Index() {
         return View();
      }

      public EventController(IHttpContextAccessor accessor): base(accessor) { }
   }
}