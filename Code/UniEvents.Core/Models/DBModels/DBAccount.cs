using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using UniEvents.Core;
using ZMBA;


namespace UniEvents.Models.DBModels {

   public class DBAccount : DBModel {

      [DBCol("AccountID", SqlDbType.BigInt, 1, false, isAutoValue: true)]
      public Int64 AccountID { get; set; }

      [DBCol("LocationID", SqlDbType.BigInt, 1, true)]
      public Int64? LocationID { get; set; }

      [DBCol("PasswordHash", SqlDbType.Binary, 256, true)]
      public byte[] PasswordHash { get; set; }

      [DBCol("Salt", SqlDbType.VarChar, 20, true)]
      public string Salt { get; set; }

      [DBCol("UserName", SqlDbType.VarChar, 20, false)]
      public string UserName { get; set; }

      [DBCol("DisplayName", SqlDbType.NVarChar, 50, true)]
      public string DisplayName { get; set; }

      [DBCol("FirstName", SqlDbType.NVarChar, 50, true)]
      public string FirstName { get; set; }

      [DBCol("LastName", SqlDbType.NVarChar, 50, true)]
      public string LastName { get; set; }

      [DBCol("SchoolEmail", SqlDbType.NVarChar, 50, true)]
      public string SchoolEmail { get; set; }

      [DBCol("ContactEmail", SqlDbType.VarChar, 50, true)]
      public string ContactEmail { get; set; }

      [DBCol("PhoneNumber", SqlDbType.VarChar, 20, true)]
      public string PhoneNumber { get; set; }

      [DBCol("IsGroup", SqlDbType.Bit, 1, false)]
      public bool IsGroup { get; set; }

      [DBCol("VerifiedSchoolEmail", SqlDbType.Bit, 1, true)]
      public bool VerifiedSchoolEmail { get; set; }

      [DBCol("VerifiedContactEmail", SqlDbType.Bit, 1, true)]
      public bool VerifiedContactEmail { get; set; }

      public bool IsAdmin { get; set; }

      public DBAccount() { }


      internal DBAccount(SqlDataReader reader) {
         AccountID = reader.GetInt64(nameof(AccountID));
         LocationID = reader.GetInt64(nameof(LocationID));
         PasswordHash = reader.GetBytes(nameof(PasswordHash), 32);
         Salt = reader.GetString(nameof(Salt));
         UserName = reader.GetString(nameof(UserName));
         DisplayName = reader.GetString(nameof(DisplayName));
         FirstName = reader.GetString(nameof(FirstName));
         LastName = reader.GetString(nameof(LastName));
         SchoolEmail = reader.GetString(nameof(SchoolEmail));
         ContactEmail = reader.GetString(nameof(ContactEmail));
         IsGroup = reader.GetBoolean(nameof(IsGroup));
         VerifiedSchoolEmail = reader.GetNBoolean(nameof(VerifiedSchoolEmail)).UnBox();
         VerifiedContactEmail = reader.GetNBoolean(nameof(VerifiedContactEmail)).UnBox();
         IsAdmin = reader.GetNBoolean(nameof(IsAdmin)).UnBox();
      }




   }
}
