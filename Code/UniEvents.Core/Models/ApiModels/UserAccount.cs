﻿using System;
using System.Collections.Generic;
using System.Text;

using static ZMBA.Common;

namespace UniEvents.Models.ApiModels {

   public class UserAccount {

      public long AccountID { get; set; }
      public string UserName { get; set; }
      public string DisplayName { get; set; }
      public string FirstName { get; set; }
      public string LastName { get; set; }
      public string SchoolEmail { get; set; }
      public string ContactEmail { get; set; }
      public string PhoneNumber { get; set; }
      public bool VerifiedSchoolEmail { get; set; }
      public bool VerifiedContactEmail { get; set; }
      public bool IsAdmin { get; set; }

      public StreetAddress Location { get; set; }

      public string BestDisplayName => String.IsNullOrWhiteSpace(DisplayName) ? UserName : DisplayName;

      public UserAccount() {}

      public UserAccount(DBModels.DBAccount acct, StreetAddress loc) {
         AccountID = acct.AccountID;
         UserName = acct.UserName;
         DisplayName = acct.DisplayName;
         FirstName = acct.FirstName;
         LastName = acct.LastName;
         SchoolEmail = acct.SchoolEmail;
         ContactEmail = acct.ContactEmail;
         PhoneNumber = acct.PhoneNumber;
         VerifiedContactEmail = acct.VerifiedContactEmail;
         VerifiedSchoolEmail = acct.VerifiedSchoolEmail;
         IsAdmin = acct.IsAdmin;
         Location = loc;
      }


   }
}
