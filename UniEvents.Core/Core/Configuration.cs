using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniEvents.Core {

	public class Configuration {
      /**
       * Connection string to use when loading configuration settings from the database. 
       */
		public string dbUniHangoutsConfiguration { get; set; }
		public string dbUniHangoutsRead{ get; set; }
		public string dbUniHangoutsWrite { get; set; }
		public string dbUniHangoutsReadWrite { get; set; }

      public Configuration() { }

	}

}
