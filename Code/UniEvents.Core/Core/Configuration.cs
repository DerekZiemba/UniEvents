using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UniEvents.Core {

   public class Configuration {
      /**
       * Connection string to use when loading configuration settings from the database. 
       */
      public string dbUniHangoutsConfiguration { get; set; }
      public string dbUniHangoutsRead { get; set; }
      public string dbUniHangoutsWrite { get; set; }
      public string dbUniHangoutsReadWrite { get; set; }


      [JsonProperty(PropertyName = "IsDebugMode")]private bool? _IsDebugMode;

      [JsonIgnore] public bool IsDebugMode {
         get {
            if(_IsDebugMode.HasValue) { return _IsDebugMode.Value; }
#if DEBUG
            return true;
#else
            return System.Diagnostics.Debugger.IsAttached;
#endif
         }
      }

      public Configuration() { }

   }

}
