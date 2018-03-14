using System;
using System.Collections.Generic;
using System.Text;

using static ZMBA.Common;

namespace UniEvents.Core.ApiModels {

	public class AccountLogin : APIModelBase {
      public string UserName { get; set; }
      public string APIKey { get; set; }
      public DateTime LoginDate { get; set; }

      public AccountLogin() {}

	}
}
