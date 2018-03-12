using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using ZMBA;

namespace UniEvents.Core.DBModels {

	public class DBAccountDetails {
		private Int64 _AccountID;
		private Int64? _LocationID;
		private string _FirstName;
		private string _LastName;
		private string _ContactEmail;
		private string _PhoneNumber;
		private string _Description;

		public DBAccountDetails() { }

		[DBCol("AccountID", SqlDbType.BigInt, 1)]
		public Int64 AccountID { get => _AccountID; set => _AccountID = value; }

		[DBCol("LocationID", SqlDbType.BigInt, 1)]
		public Int64? LocationID { get => _LocationID; set => _LocationID = value; }

		[DBCol("FirstName", SqlDbType.BigInt, 1)]
		public string FirstName { get => _FirstName; set => _FirstName = value; }

		public string LastName { get => _LastName; set => _LastName = value; }

		public string ContactEmail { get => _ContactEmail; set => _ContactEmail = value; }

		public string PhoneNumber { get => _PhoneNumber; set => _PhoneNumber = value; }

		public string Description { get => _Description; set => _Description = value; }


		public static DBAccountDetails CreateModel(IDataReader reader) {
			DBAccountDetails model = new DBAccountDetails();
			reader.PullValue(nameof(AccountID), out model._AccountID);
			reader.PullValue(nameof(LocationID), out model._LocationID);
			reader.PullValue(nameof(FirstName), out model._FirstName);
			reader.PullValue(nameof(LastName), out model._LastName);
			reader.PullValue(nameof(ContactEmail), out model._ContactEmail);
			reader.PullValue(nameof(PhoneNumber), out model._PhoneNumber);
			reader.PullValue(nameof(Description), out model._Description);
			return model;
		}

	}

}
