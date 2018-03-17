using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using UniEvents.Models.ApiModels;
using ZMBA;


namespace UniEvents.WebApp {

   public class MetaDataManager {
      public IApiDescriptionGroupCollectionProvider ApiExplorer { get; private set; }

      public SortedDictionary<string, MethodMetadata> MethodsByRoute { get; } = new SortedDictionary<string, MethodMetadata>(StringComparer.OrdinalIgnoreCase);
      public SortedDictionary<string, MethodMetadata> MethodsByPath { get; } = new SortedDictionary<string, MethodMetadata>(StringComparer.OrdinalIgnoreCase);


      public MetaDataManager(IApiDescriptionGroupCollectionProvider apiExplorer) {
         ApiExplorer = apiExplorer;
         foreach (var group in ApiExplorer.ApiDescriptionGroups.Items) {
            foreach (var desc in group.Items) {
               var meta = new MethodMetadata();
               meta.Route = desc.RelativePath;
               meta.Path = meta.Route.SubstrBefore("/{", Common.SubstrOptions.RetInput);
               meta.HttpMethod = Enum.Parse<Models.EHttpVerbs>(desc.HttpMethod, true);
               meta.MethodName = desc.ActionDescriptor.RouteValues["action"];
               meta.FullMethodName = desc.ActionDescriptor.DisplayName.SubstrBefore(" (", Common.SubstrOptions.RetInput);

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
                  if (paramdesc.Source.Id == "Path") {
                     param.Source = MethodMetadata.ParamSource.Url;
                  } else if (paramdesc.Source.Id == "ModelBinding") {
                     if (paramdesc.RouteInfo != null) {
                        param.Source = MethodMetadata.ParamSource.QueryString;
                     } else {
                        param.Source = MethodMetadata.ParamSource.Body;
                     }
                  }
                  meta.Input.Add(param);
               }
               MethodsByRoute.Add(meta.Route, meta);
               MethodsByPath[meta.Path] = meta;
            }
         }



      }

      



   }

}
