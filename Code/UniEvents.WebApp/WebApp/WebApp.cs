using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UniEvents.Core;
using UniEvents.Managers;
using UniEvents.WebApp;


namespace UniEvents.WebApp {

   /// <summary>
   /// Provide a way to mixin common functionality to both models and controllers. 
   /// </summary>
   public interface IWebAppContext {
      HttpContext HttpContext { get; }
      ModelStateDictionary ModelState { get; }
      HttpRequest Request { get; }
      HttpResponse Response { get; }
      RouteData RouteData { get; }        
      ITempDataDictionary TempData { get; }
      IUrlHelper Url { get; }
      ClaimsPrincipal User { get; }
      ViewDataDictionary ViewData { get; }

      UserContext UserContext { get; }
   }

   public class WebAppPageModel : PageModel, IWebAppContext {
      public UserContext UserContext { get; private set; }

      public WebAppPageModel(IHttpContextAccessor accessor) {
         UserContext = UserContext.InitContext(accessor.HttpContext);
      }
   }

   public class WebAppController : Controller, IWebAppContext {
      public UserContext UserContext { get; private set; }

      public WebAppController(IHttpContextAccessor accessor) {
         UserContext = UserContext.InitContext(accessor.HttpContext);
      }
   }


   /// <summary>
   /// Extension method Mixins for IWebApp
   /// </summary>
   public static class WebAppContext {
      internal static IConfiguration Configuration;
      internal static IServiceCollection Services;
      internal static IHostingEnvironment Environment;
      internal static MetaDataManager MetaData;
      internal static CoreContext CoreContext;
      
      internal static void _init(IConfiguration value) { Configuration = value; }
      internal static void _init(IServiceCollection value) {
         Services = value;
         MetaData = new MetaDataManager(Services.BuildServiceProvider().GetService<IApiDescriptionGroupCollectionProvider>());
      }
      internal static void _init(IHostingEnvironment value) {
         Environment = value;
         if (Environment.IsDevelopment()) {
            CoreContext = new Core.CoreContext("C:\\UniEvents.config.json");
         } else if (Environment.IsStaging()) {
            CoreContext = new Core.CoreContext("C:\\UniEvents.config.json");
         } else if (Environment.IsProduction()) {
            CoreContext = new Core.CoreContext("C:\\UniEvents.config.json");
         }
      }


      public static MetaDataManager MetaDataManager(this IWebAppContext context) => MetaData;
      public static AccountManager AccountManager(this IWebAppContext context) => CoreContext.AccountManager;
      public static LocationManager LocationManager(this IWebAppContext context) => CoreContext.LocationManager;

   }


}
