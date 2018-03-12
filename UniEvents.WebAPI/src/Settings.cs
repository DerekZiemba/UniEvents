using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Xml;
using System.IO;

namespace UniEvents.WebAPI {


	public static class Settings {

		internal static string SqlDbUniHangoutsConnStr { get; private set; }

		static Settings() {
			SqlDbUniHangoutsConnStr = ConfigurationManager.AppSettings.Get(nameof(SqlDbUniHangoutsConnStr));

			//Temporary because ConfigurationManager.AppSettings isn't working in dotnet Core apps apparently. 
			using (FileStream fs = new FileStream("web.config", FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
				using (StreamReader reader = new StreamReader(fs, System.Text.Encoding.UTF8)) {
					XmlDocument doc = new XmlDocument();
					doc.Load(reader);
					XmlNode appSettings = doc.SelectSingleNode("//configuration/appSettings");

					var dict = appSettings.SelectNodes("add").Cast<XmlNode>().ToDictionary(n=>n.Attributes["key"].Value, n=>n.Attributes["value"].Value, StringComparer.OrdinalIgnoreCase);

					SqlDbUniHangoutsConnStr = dict[nameof(SqlDbUniHangoutsConnStr)];

				}
			}

		}
	}

}
