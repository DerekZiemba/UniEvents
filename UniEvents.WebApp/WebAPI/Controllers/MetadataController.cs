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
   public class MetadataController : Controller {
      private static Dictionary<string, MethodMetadata> _methodCache;

      private readonly IApiDescriptionGroupCollectionProvider _apiExplorer;
      public MetadataController(IApiDescriptionGroupCollectionProvider apiExplorer) {
         _apiExplorer = apiExplorer;
      }


      [HttpGet, Route("webapi/metadata")]
      public ApiResult<Dictionary<string, MethodMetadata>> Get() {
         var result = new ApiResult<Dictionary<string, MethodMetadata>>();
         if(_methodCache == null) {
            BuildMethodCache();
         }
         result.Result = _methodCache;
         return result;
      }

      [HttpGet, Route("webapi/metadata/{route}")]
      public ApiResult<MethodMetadata> Get(string route) {
         var result = new ApiResult<MethodMetadata>();
         if (_methodCache == null) {
            BuildMethodCache();
         }

         result.Result = _methodCache.GetItemOrDefault(route);
         if(result.Result != null) { result.Success = true; }
         return result;
      }


      private void BuildMethodCache() {
         _methodCache = new Dictionary<string, MethodMetadata>(StringComparer.OrdinalIgnoreCase);

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

               var bRemoved = false;
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
               _methodCache.Add(meta.Path, meta);
            }
         }
      }


   }
}
