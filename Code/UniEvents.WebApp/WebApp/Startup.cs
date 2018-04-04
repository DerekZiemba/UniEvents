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
         WebApp.WebAppContext._init(env);

         app.UseStaticFiles();
         
         app.UseSession();

         //app.UseCookiePolicy();

         app.UseMvc();

      }
   }
}
