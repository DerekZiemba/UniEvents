using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UniEvents.WebApp {
   public class Startup {
      public Startup(IConfiguration configuration) {        
         WebApp.WebAppContext._init(configuration);
      }

      // This method gets called by the runtime. Use this method to add services to the container.
      public void ConfigureServices(IServiceCollection services) {
         //services.AddCors(options => {
         //   options.AddPolicy("CorsPolicy",
         //       builder => builder.AllowAnyOrigin()
         //       .AllowAnyMethod()
         //       .AllowAnyHeader()
         //       .AllowCredentials());
         //});

         services.AddMvc().AddJsonOptions(options=> {
            options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
         }).AddSessionStateTempDataProvider();

         services.AddDistributedMemoryCache();

         services.AddSession(opts=> {
            opts.Cookie.SecurePolicy = CookieSecurePolicy.None;
            opts.Cookie.SameSite = SameSiteMode.None;
         });

         services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

         

         WebApp.WebAppContext._init(services);
      }


      // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
      public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
         if (env.IsDevelopment()) {
            app.UseBrowserLink();
            app.UseDeveloperExceptionPage();
         } else {
            //app.UseExceptionHandler("/Error");
            app.UseDeveloperExceptionPage();
         }

         //app.UseMiddleware<CorsMiddleware>();
         //app.UseCors("CorsPolicy");

         WebApp.WebAppContext._init(env);

         app.UseStaticFiles();
         
         app.UseSession();

         //app.UseCookiePolicy();



         app.UseMvc();



      }


      public class CorsMiddleware {
         private readonly RequestDelegate _next;

         public CorsMiddleware(RequestDelegate next) {
            _next = next;
         }

         public Task Invoke(HttpContext httpContext) {
            httpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            httpContext.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
            httpContext.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, X-CSRF-Token, X-Requested-With, Accept, Accept-Version, Content-Length, Content-MD5, Date, X-Api-Version, X-File-Name");
            httpContext.Response.Headers.Add("Access-Control-Allow-Methods", "POST,GET,PUT,PATCH,DELETE,OPTIONS");
            return _next(httpContext);
         }
      }


   }
}
