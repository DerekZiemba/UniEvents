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

   internal class ConnectionStrings : Core.IConnectionStrings {
      public string dbUniHangoutsConfiguration { get; private set; }
      public string dbUniHangoutsRead { get; private set; }
      public string dbUniHangoutsWrite { get; private set; }
      public string dbUniHangoutsReadWrite { get; private set; }

      internal ConnectionStrings() {
         dbUniHangoutsConfiguration = ConfigurationManager.ConnectionStrings["SqlDbUniHangoutsConnStr"].ConnectionString;
         dbUniHangoutsRead = ConfigurationManager.ConnectionStrings["SqlDbUniHangoutsConnStr"].ConnectionString;
         dbUniHangoutsWrite = ConfigurationManager.ConnectionStrings["SqlDbUniHangoutsConnStr"].ConnectionString;
         dbUniHangoutsReadWrite = ConfigurationManager.ConnectionStrings["SqlDbUniHangoutsConnStr"].ConnectionString;
      }
   }



}
