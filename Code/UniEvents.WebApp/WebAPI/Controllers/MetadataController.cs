using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using UniEvents.WebApp;
using UniEvents.Core;
using UniEvents.Models.ApiModels;
using DBModels = UniEvents.Models.DBModels;


using static ZMBA.Common;

namespace UniEvents.WebAPI.Controllers {

   [Produces("application/json")]
   [ApiExplorerSettings(IgnoreApi = false)]
   public class MetadataController : Controller {

      [HttpGet, Route("webapi/metadata")]
      public ApiResult<SortedDictionary<string, MethodMetadata>> Get() {
         return new ApiResult<SortedDictionary<string, MethodMetadata>>(true, "", Program.MetaData.MethodsByRoute);
      }

      [HttpGet, Route("webapi/metadata/{route}")]
      public ApiResult<MethodMetadata> Get(string route) {
         var result = new ApiResult<MethodMetadata>();

         result.Result = Program.MetaData.MethodsByRoute.GetItemOrDefault(route) ?? Program.MetaData.MethodsByPath.GetItemOrDefault(route);
 
         if(result.Result != null) { result.Success = true; }
         return result;
      }



   }
}
