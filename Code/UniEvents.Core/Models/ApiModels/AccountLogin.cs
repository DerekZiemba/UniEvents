using System;
using System.Collections.Generic;
using System.Text;

using static ZMBA.Common;

namespace UniEvents.Models.ApiModels {

	public class AccountLogin {
      public string UserName { get; set; }
      public string APIKey { get; set; }
      public DateTime LoginDate { get; set; }

      public AccountLogin() {}

      public AccountLogin(DBModels.DBLogin login) {
         UserName = login.UserName;
         APIKey = login.APIKey;
         LoginDate = login.LoginDate;
      }
   }

   public class VerifiedLogin {
      public bool IsLoggedIn { get; set; }
      public DateTime? LoginDate { get; set; }

   }

}
