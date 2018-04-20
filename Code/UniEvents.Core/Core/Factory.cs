using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using UniEvents.Core.Managers;

using static ZMBA.Common;

namespace UniEvents.Core {

   public class Factory {
      private readonly string _configFilePath;
      private Configuration _Config;

      private Task _loadConfigTask;
      public Configuration Config { get { Helpers.BlockUntilFinished(ref _loadConfigTask); return _Config; } }

      public AccountManager AccountManager { get; private set; }
      public LocationManager LocationManager { get; private set; }
      public TagManager TagManager { get; private set; }
      public EventTypeManager EventTypeManager { get; private set; }
      public RSVPTypeManager RSVPTypeManager { get; private set; }
      public EventManager EventManager { get; private set; }
       public EmailManager EmailManager { get; private set; }


      public Factory(string configFilePath) {
         this._configFilePath = configFilePath;

         _loadConfigTask = Task.Run((Func<bool>)LoadConfiguration);

         this.AccountManager = new AccountManager(this);
         this.LocationManager = new LocationManager(this);
         this.TagManager = new TagManager(this);
         this.EventTypeManager = new EventTypeManager(this);
         this.RSVPTypeManager = new RSVPTypeManager(this);
         this.EventManager = new EventManager(this);
         this.EmailManager = new EmailManager(this);
      }

      private bool LoadConfiguration() {
         Configuration result = null;
         if (System.IO.File.Exists(_configFilePath)) {
            using (var fs = new FileStream(_configFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs, Encoding.UTF8))
            using (var jsreader = new Newtonsoft.Json.JsonTextReader(sr)) {
               result = CompactJsonSerializer.Deserialize<Configuration>(jsreader);
            }
         }
         _Config = result;
         return true;
      }




   }
}
