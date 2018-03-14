using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace UniEvents.WebAPI {
	public class WebApiApplication : System.Web.HttpApplication {
		internal static Core.CoreContext CoreContext { get; set; }

		protected void Application_Start() {
			GlobalConfiguration.Configure(WebApiConfig.Register);
         CoreContext = new Core.CoreContext(new ConnectionStrings());
		}

	}
}
