﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
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
      Factory Factory { get; }
   }

   public class WebAppPageModel : PageModel, IWebAppContext {
      private readonly IHttpContextAccessor _httpContextAccessor;
      private object _userctxLock = new object();
      private UserContext _UserContext = null;

      public new HttpContext HttpContext => base.HttpContext ?? _httpContextAccessor.HttpContext;
      public UserContext UserContext {
         get {
            if(_UserContext == null && _userctxLock != null) {
               lock (_userctxLock) {
                  if (_UserContext != null) { return _UserContext; }
                  _UserContext = UserContext.InitContext(this.HttpContext).ConfigureAwait(false).GetAwaiter().GetResult();
                  Interlocked.Exchange(ref _userctxLock, null);
               }
            }
            return _UserContext;
         }
      }

      public Factory Factory => WebAppContext.Factory;

      public WebAppPageModel(IHttpContextAccessor accessor) {
         _httpContextAccessor = accessor;
      }
   }

   public class WebAPIController : Controller, IWebAppContext {
      private readonly IHttpContextAccessor _httpContextAccessor;
      private object _userctxLock = new object();
      private UserContext _UserContext = null;

      public new HttpContext HttpContext => base.HttpContext ?? _httpContextAccessor.HttpContext;
      public UserContext UserContext {
         get {
            if (_UserContext == null && _userctxLock != null) {
               lock (_userctxLock) {
                  if (_UserContext != null) { return _UserContext; }
                  _UserContext = UserContext.InitContext(this.HttpContext).ConfigureAwait(false).GetAwaiter().GetResult();
                  Interlocked.Exchange(ref _userctxLock, null);
               }
            }
            return _UserContext;
         }
      }

      public Factory Factory => WebAppContext.Factory;

      public WebAPIController(IHttpContextAccessor accessor) {
         _httpContextAccessor = accessor;
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
      internal static Factory Factory;
      public static string EnvironmentString;
      public static string ServerName;
      
      internal static void _init(IConfiguration value) { Configuration = value; }
      internal static void _init(IServiceCollection value) {
         Services = value;
         MetaData = new MetaDataManager(Services.BuildServiceProvider().GetService<IApiDescriptionGroupCollectionProvider>());
      }
      internal static void _init(IHostingEnvironment value) {
         ServerName = System.Environment.MachineName;
         Environment = value;
         if (Environment.IsDevelopment()) {
            Factory = new Core.Factory("C:\\UniEvents.config.json");
            EnvironmentString = "dev";
         } else if (Environment.IsStaging()) {
            Factory = new Core.Factory("C:\\UniEvents.config.json");
            EnvironmentString = "stg";
         } else if (Environment.IsProduction()) {
            Factory = new Core.Factory("C:\\UniEvents.config.json");
            EnvironmentString = "live";
         } else {
            Factory = new Core.Factory("C:\\UniEvents.config.json");
            EnvironmentString = "unknown";
         }
      }


      public static MetaDataManager MetaDataManager(this IWebAppContext context) => MetaData;

   }


}
