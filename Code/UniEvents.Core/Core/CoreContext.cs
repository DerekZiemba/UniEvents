using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using UniEvents.Managers;

using static ZMBA.Common;

namespace UniEvents.Core {

   public class CoreContext {
      private readonly string _configFilePath;
      private readonly Lazy<Configuration> _Config;
      private readonly Lazy<AccountManager> _AccountManager;
      private readonly Lazy<LocationManager> _LocationManager;
      private readonly Lazy<TagManager> _TagManager;
      private readonly Lazy<RSVPTypeManager> _RSVPTypeManager;

      public Configuration Config => _Config.Value;
      public AccountManager AccountManager => _AccountManager.Value;
      public LocationManager LocationManager => _LocationManager.Value;
      public TagManager TagManager => _TagManager.Value;
      public RSVPTypeManager RSVPTypeManager => _RSVPTypeManager.Value;

      public CoreContext(string configFilePath) {
         this._configFilePath = configFilePath;

         _Config = new Lazy<Configuration>(() => {
            Configuration result = null;
            if (System.IO.File.Exists(_configFilePath)) {             
               using (var fs = new FileStream(_configFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
               using (var sr = new StreamReader(fs, Encoding.UTF8))
               using (var jsreader = new Newtonsoft.Json.JsonTextReader(sr)) {
                  result = JsonSerializer.Deserialize<Configuration>(jsreader);
               }
            }
            return result;
         }, LazyThreadSafetyMode.ExecutionAndPublication);

         this._AccountManager = new Lazy<AccountManager>(() => new AccountManager(this), LazyThreadSafetyMode.ExecutionAndPublication);
         this._LocationManager = new Lazy<LocationManager>(() => new LocationManager(this), LazyThreadSafetyMode.ExecutionAndPublication);
         this._TagManager = new Lazy<TagManager>(() => new TagManager(this), LazyThreadSafetyMode.ExecutionAndPublication);
         this._RSVPTypeManager = new Lazy<RSVPTypeManager>(() => new RSVPTypeManager(this), LazyThreadSafetyMode.ExecutionAndPublication);
      }



	}
}
