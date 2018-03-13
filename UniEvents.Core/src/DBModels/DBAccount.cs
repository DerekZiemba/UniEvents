using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using ZMBA;


namespace UniEvents.Core.DBModels {

	public class DBAccount {
		public DBAccount() { }

		[DBCol("AccountID", SqlDbType.BigInt, 1, false, isAutoValue:true)]
		public Int64 AccountID { get; set; }

        [DBCol("LocationID", SqlDbType.BigInt, 1, true , isAutoValue: true)]
        public Int64 LocationID { get; set; }

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

        public static DBAccount CreateModel(IDataReader reader) {
			DBAccount model = new DBAccount();
			model.AccountID = reader.GetInt64(nameof(AccountID));
            model.LocationID = reader.GetInt64(nameof(LocationID));
            model.PasswordHash = reader.GetBytes(nameof(PasswordHash), 32);
            model.Salt = reader.GetString(nameof(Salt));
            model.UserName = reader.GetString(nameof(UserName));
			model.DisplayName = reader.GetString(nameof(DisplayName));
            model.FirstName = reader.GetString(nameof(FirstName));
            model.LastName = reader.GetString(nameof(LastName));
            model.SchoolEmail = reader.GetString(nameof(SchoolEmail));
            model.ContactEmail = reader.GetString(nameof(ContactEmail));
            model.IsGroup = reader.GetBoolean(nameof(IsGroup));
            model.VerifiedSchoolEmail = reader.GetBoolean(nameof(VerifiedSchoolEmail));
            model.VerifiedContactEmail = reader.GetBoolean(nameof(VerifiedContactEmail));
            
			return model;
		}

	}

}
