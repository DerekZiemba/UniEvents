using System;
using System.Collections.Generic;
using System.Text;

using static ZMBA.Common;

namespace UniEvents.Models.ApiModels {

	public class UserAccount : APIModel {
      public Int64 AccountID { get; set; }
      public string Password { get; set; }
      public string UserName { get; set; }
      public string DisplayName { get; set; }
      public string FirstName { get; set; }
      public string LastName { get; set; }
      public string SchoolEmail { get; set; }
      public string ContactEmail { get; set; }
      public string PhoneNumber { get; set; }
      public bool VerifiedSchoolEmail { get; set; }
      public bool VerifiedContactEmail { get; set; }
      public StreetAddress Location { get; set; }


      public UserAccount() {}


	}
}
