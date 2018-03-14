using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniEvents.Core {

	public interface IConnectionStrings {
      /**
       * Connection string to use when loading configuration settings from the database. 
       */
		string dbUniHangoutsConfiguration { get; }
		string dbUniHangoutsRead{ get; }
		string dbUniHangoutsWrite { get; }
		string dbUniHangoutsReadWrite { get; }
	}

}
