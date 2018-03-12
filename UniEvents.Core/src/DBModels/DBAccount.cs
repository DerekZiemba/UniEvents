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

		[DBCol("UserName", SqlDbType.VarChar, 20, false)]
		public string UserName { get; set; }

		[DBCol("DisplayName", SqlDbType.NVarChar, 50, true)]
		public string DisplayName { get; set; }

		[DBCol("PasswordHash", SqlDbType.Binary, 256, true)]
		public byte[] PasswordHash { get; set; }

		[DBCol("IsGroup", SqlDbType.Bit, 1, false)]
		public bool IsGroup { get; set; }

		public static DBAccount CreateModel(IDataReader reader) {
			DBAccount model = new DBAccount();
			model.AccountID = reader.GetInt64(nameof(AccountID));
			model.UserName = reader.GetString(nameof(UserName));
			model.DisplayName = reader.GetString(nameof(DisplayName));
			model.PasswordHash = reader.GetBytes(nameof(PasswordHash), 32);
			model.IsGroup = reader.GetBoolean(nameof(IsGroup));
			return model;
		}

	}

}
