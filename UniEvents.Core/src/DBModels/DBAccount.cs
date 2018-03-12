using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using ZMBA;


namespace UniEvents.Core.DBModels {

	public class DBAccount {
		private Int64 _AccountID;
		private string _UserName;
		private string _DisplayName;
		private byte[] _PasswordHash;
		private bool _IsGroup;

		public DBAccount() { }

		[DBCol("AccountID", SqlDbType.BigInt, 1)]
		public Int64 AccountID { get => _AccountID; set => _AccountID = value; }

		[DBCol("UserName", SqlDbType.VarChar, 20)]
		public string UserName { get => _UserName; set => _UserName = value; }

		[DBCol("DisplayName", SqlDbType.NVarChar, 50)]
		public string DisplayName { get => _DisplayName; set => _DisplayName = value; }

		[DBCol("PasswordHash", SqlDbType.Binary, 256)]
		public byte[] PasswordHash { get => _PasswordHash; set => _PasswordHash = value; }

		[DBCol("IsGroup", SqlDbType.Bit, 1)]
		public bool IsGroup { get => _IsGroup; set => _IsGroup = value; }

		public static DBAccount CreateModel(IDataReader reader) {
			DBAccount model = new DBAccount();
			reader.PullValue(nameof(AccountID), out model._AccountID);
			reader.PullValue(nameof(UserName), out model._UserName);
			reader.PullValue(nameof(DisplayName), out model._DisplayName);
			reader.PullBytes(nameof(PasswordHash), out model._PasswordHash, 32);
			reader.PullValue(nameof(IsGroup), out model._IsGroup);
			return model;
		}

	}

}
