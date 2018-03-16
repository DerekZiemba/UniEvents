using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using ZMBA;


namespace UniEvents.Models.DBModels {

	public class DBEventFeedItem {

	
		[DBCol("EventID", SqlDbType.BigInt, 1, false, isAutoValue:true)]
		public Int64 EventID { get; set; }

		[DBCol("EventTypeID", SqlDbType.Int, 1, false)]
		public int EventTypeID { get; set; }

		[DBCol("DateStart", SqlDbType.SmallDateTime, 1, false)]
		public DateTime DateStart { get; set; }

		[DBCol("DateEnd", SqlDbType.SmallDateTime, 1, false)]
		public DateTime DateEnd { get; set; }

		[DBCol("AccountID", SqlDbType.BigInt, 1, false)]
		public Int64 AccountID { get; set; }

		[DBCol("LocationID", SqlDbType.BigInt, 1, false)]
		public Int64 LocationID { get; set; }

		[DBCol("Title", SqlDbType.VarChar, 80, false)]
		public string Title { get; set; }

		[DBCol("Caption", SqlDbType.NVarChar, 160, false)]
		public string Caption { get; set; }

      public DBEventFeedItem() { }

		public DBEventFeedItem(IDataReader reader) {
			EventID = reader.GetInt64(nameof(EventID));
			EventTypeID = reader.GetInt32(nameof(EventTypeID));
			DateStart = reader.GetDateTime(nameof(DateStart));
			DateEnd = reader.GetDateTime(nameof(DateEnd));
			AccountID = reader.GetInt64(nameof(AccountID));
			LocationID = reader.GetInt64(nameof(LocationID));
			Title = reader.GetString(nameof(Title));
			Caption = reader.GetString(nameof(Caption));
		}

	}

}
