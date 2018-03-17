using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UniEvents.WebApp {
	public class Startup {
		public Startup(IConfiguration configuration) {        
		   Program.Configuration = configuration;
		}


		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services) {
			services.AddMvc().AddJsonOptions(options=> {
            options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
         });

         Program.MetaData = new MetaDataManager(services.BuildServiceProvider().GetService<IApiDescriptionGroupCollectionProvider>());
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

         if(env.IsDevelopment()){
            Program.CoreContext = new Core.CoreContext("C:\\UniEvents.config.json");
         } else if (env.IsStaging()) {
            Program.CoreContext = new Core.CoreContext("C:\\UniEvents.config.json");
         } else if (env.IsProduction()) {
            Program.CoreContext = new Core.CoreContext("C:\\UniEvents.config.json");
         }

			app.UseStaticFiles();

			app.UseMvc();

		}
	}
}
