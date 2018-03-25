using System;
using System.Collections.Generic;
using System.Text;

using static ZMBA.Common;

namespace UniEvents.Models.ApiModels {

	public class UserAccount {

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

      public UserAccount(DBModels.DBAccount acct, StreetAddress loc) {
         UserName = acct.UserName;
         DisplayName = acct.DisplayName;
         FirstName = acct.FirstName;
         LastName = acct.LastName;
         SchoolEmail = acct.SchoolEmail;
         ContactEmail = acct.ContactEmail;
         PhoneNumber = acct.PhoneNumber;
         VerifiedContactEmail = acct.VerifiedContactEmail.UnBox();
         VerifiedSchoolEmail = acct.VerifiedSchoolEmail.UnBox();
         Location = loc;
      }


	}
}
