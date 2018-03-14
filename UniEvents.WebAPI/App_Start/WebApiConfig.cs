using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace UniEvents.WebAPI {
	public static class WebApiConfig {
#if DEBUG
		public static readonly bool IsDebug = true;
#else
		public static readonly bool IsDebug = false;
#endif

		private static JsonMediaTypeFormatter JSONFormattter = new JsonMediaTypeFormatter(){ Indent=IsDebug };

		public static void Register(HttpConfiguration config) {
			// Web API configuration and services
			config.Formatters.Clear();
			config.Formatters.Add(JSONFormattter);

			//https://www.strathweb.com/2013/06/supporting-only-json-in-asp-net-web-api-the-right-way/
			config.Services.Replace(typeof(IContentNegotiator), new JsonContentNegotiator(JSONFormattter));

			// Web API routes
			config.MapHttpAttributeRoutes();

			config.Routes.MapHttpRoute(
				 name: "DefaultApi",
				 routeTemplate: "{controller}/{id}",
				 defaults: new { id = RouteParameter.Optional }
			);

		}
	}


}
