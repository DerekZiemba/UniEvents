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
      private static SortedDictionary<string, MethodMetadata> _methodCacheByRoute;
      private static SortedDictionary<string, MethodMetadata> _methodCacheByPath;

      private readonly IApiDescriptionGroupCollectionProvider _apiExplorer;
      public MetadataController(IApiDescriptionGroupCollectionProvider apiExplorer) {
         _apiExplorer = apiExplorer;
      }


      [HttpGet, Route("webapi/metadata")]
      public ApiResult<SortedDictionary<string, MethodMetadata>> Get() {
         var result = new ApiResult<SortedDictionary<string, MethodMetadata>>();
         if(_methodCacheByRoute == null) {
            BuildMethodCache();
         }
         result.Result = _methodCacheByRoute;
         return result;
      }

      [HttpGet, Route("webapi/metadata/{route}")]
      public ApiResult<MethodMetadata> Get(string route) {
         var result = new ApiResult<MethodMetadata>();
         if (_methodCacheByRoute == null) {
            BuildMethodCache();
         }

         result.Result = _methodCacheByRoute.GetItemOrDefault(route) ?? _methodCacheByPath.GetItemOrDefault(route);
 
         if(result.Result != null) { result.Success = true; }
         return result;
      }


      private void BuildMethodCache() {
         _methodCacheByRoute = new SortedDictionary<string, MethodMetadata>(StringComparer.OrdinalIgnoreCase);
         _methodCacheByPath = new SortedDictionary<string, MethodMetadata>(StringComparer.OrdinalIgnoreCase);

         foreach (var group in _apiExplorer.ApiDescriptionGroups.Items) {
            foreach (var desc in group.Items) {
               var meta = new MethodMetadata();
               meta.Route = desc.RelativePath;
               meta.Path = meta.Route.SubstrBefore("/{", SubstrOptions.RetInput);
               meta.HttpMethod = Enum.Parse<Models.EHttpVerbs>(desc.HttpMethod, true);
               meta.MethodName = desc.ActionDescriptor.RouteValues["action"];
               meta.FullMethodName = desc.ActionDescriptor.DisplayName.SubstrBefore(" (", SubstrOptions.RetInput);

               var responseTYpe = desc.SupportedResponseTypes.FirstOrDefault();
               if (responseTYpe != null) {
                  meta.Output = new MethodMetadata.InputOutput();
                  meta.Output.SetType(responseTYpe.Type);
               }

               foreach (var paramdesc in desc.ParameterDescriptions) {
                  var param = new MethodMetadata.MethodParam();
                  param.SetType(paramdesc.Type);
                  param.Name = paramdesc.Name;
                  param.IsOptional = (paramdesc.RouteInfo?.IsOptional).UnBox();
                  if(paramdesc.Source.Id == "Path") {
                     param.Source = MethodMetadata.ParamSource.Url;
                  } else if(paramdesc.Source.Id == "ModelBinding") {
                     if(paramdesc.RouteInfo != null) {
                        param.Source = MethodMetadata.ParamSource.QueryString;
                     } else {
                        param.Source = MethodMetadata.ParamSource.Body;
                     }
                  }
                  meta.Input.Add(param);
               }
               _methodCacheByRoute.Add(meta.Route, meta);
               _methodCacheByPath[meta.Path] = meta;
            }
         }
      }


   }
}
