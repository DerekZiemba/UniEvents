using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniEvents.Core {

	public class CoreContext {
		public IConnectionStrings ConnStrings { get; private set; }

		public CoreContext(IConnectionStrings conn) {
			ConnStrings = conn;
		}

	}


}
