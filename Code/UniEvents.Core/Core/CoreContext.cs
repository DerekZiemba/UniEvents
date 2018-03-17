using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UniEvents.Managers;


namespace UniEvents.Core {

	public class CoreContext {
		public Configuration Config { get; set; }

      public Newtonsoft.Json.JsonSerializer JsonSerializer { get; set; }

      public AccountManager AccountManager { get; private set; }
      public LocationManager LocationManager { get; private set; }


      public CoreContext(string configFilePath) {
         JsonSerializer = new Newtonsoft.Json.JsonSerializer();
         JsonSerializer.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

         if (System.IO.File.Exists(configFilePath)) {
            using (var fs = new FileStream(configFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs, Encoding.UTF8))
            using (var jsreader = new Newtonsoft.Json.JsonTextReader(sr)) {
               Config = JsonSerializer.Deserialize<Configuration>(jsreader);
            }           
         }

         this.AccountManager = new AccountManager(this);
         this.LocationManager = new LocationManager(this);

		}



	}
}
