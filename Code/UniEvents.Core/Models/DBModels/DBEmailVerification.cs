using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using UniEvents.Core;
using ZMBA;

namespace UniEvents.Models.DBModels {

   public class DBEmailVerification : DBModel {

      public long AccountID { get; set; }
      public string VerificationKey { get; set; }
      public byte[] VerificationHash { get; set; }
      public string Email { get; set; }
      public DateTime Date { get; set; }
      public bool IsVerified { get; set; }

      public DBEmailVerification() { }

      public DBEmailVerification(SqlDataReader reader) {
         AccountID = reader.GetInt64(nameof(AccountID));
         VerificationKey = reader.GetString(nameof(VerificationKey));
         VerificationHash = reader.GetBytes(nameof(VerificationHash), 32);
         Email = reader.GetString(nameof(Email));
         Date = reader.GetDateTime(nameof(Date));
         IsVerified = reader.GetBoolean(nameof(IsVerified));
      }


   }

}
