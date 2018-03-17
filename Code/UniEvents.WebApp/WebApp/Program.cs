using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace UniEvents.WebApp {
	public class Program {

      internal static Core.CoreContext CoreContext { get; set; }

      internal static IConfiguration Configuration { get; set; }

      internal static MetaDataManager MetaData { get; set; }

		public static void Main(string[] args) {
			BuildWebHost(args).Run();
		}

		public static IWebHost BuildWebHost(string[] args) =>
			 WebHost.CreateDefaultBuilder(args)
				  .UseStartup<Startup>()
				  .Build();
	}
}
