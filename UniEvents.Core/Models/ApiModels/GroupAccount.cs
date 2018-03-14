using System;
using System.Collections.Generic;
using System.Text;

using static ZMBA.Common;

namespace UniEvents.Models.ApiModels {

	public class GroupAccount : APIModelBase {
      public Int64 AccountID { get; set; }
      public string UserName { get; set; }
      public string DisplayName { get; set; }
      public string ContactEmail { get; set; }
      public string PhoneNumber { get; set; }
      public bool VerifiedContactEmail { get; set; }
      public StreetAddress Location { get; set; }
      
      public UserAccount GroupOwner { get; set; }
      public List<UserAccount> GroupAdmins { get; private set; } = new List<UserAccount>();

      public GroupAccount() {}


	}
}
