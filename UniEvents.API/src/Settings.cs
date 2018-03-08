using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;


namespace UniEvents.WebAPI {


	public static class Settings {

		internal static string SqlDbUniHangoutsConnStr { get; private set; }

		static Settings() {
			SqlDbUniHangoutsConnStr = ConfigurationManager.AppSettings.Get(nameof(SqlDbUniHangoutsConnStr));

		}
	}

}
