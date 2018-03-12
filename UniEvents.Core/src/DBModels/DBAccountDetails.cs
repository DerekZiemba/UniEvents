using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using ZMBA;

namespace UniEvents.Core.DBModels {

	public class DBAccountDetails {

		public DBAccountDetails() { }

		[DBCol("AccountID", SqlDbType.BigInt, 1, false)]
		public Int64 AccountID { get; set; }

		[DBCol("LocationID", SqlDbType.BigInt, 1, true)]
		public Int64? LocationID { get; set; }

		[DBCol("FirstName", SqlDbType.NVarChar, 20, true)]
		public string FirstName { get; set; }

		[DBCol("LastName", SqlDbType.NVarChar, 20, true)]
		public string LastName { get; set; }

		[DBCol("ContactEmail", SqlDbType.VarChar, 50, true)]
		public string ContactEmail { get; set; }

		[DBCol("PhoneNumber", SqlDbType.VarChar, 20, true)]
		public string PhoneNumber { get; set; }

		[DBCol("Description", SqlDbType.NVarChar, 4000, true)]
		public string Description { get; set; }


		public static DBAccountDetails CreateModel(IDataReader reader) {
			DBAccountDetails model = new DBAccountDetails();
			model.LocationID = reader.GetInt64(nameof(LocationID));
			model.LocationID = reader.GetNInt64(nameof(LocationID));
			model.FirstName = reader.GetString(nameof(FirstName));
			model.LastName = reader.GetString(nameof(LastName));
			model.ContactEmail = reader.GetString(nameof(ContactEmail));
			model.PhoneNumber = reader.GetString(nameof(PhoneNumber));
			model.Description = reader.GetString(nameof(Description));
			return model;
		}

	}

}
