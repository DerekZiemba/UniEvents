using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using ZMBA;


namespace UniEvents.Models.DBModels {

	public class DBEventDetails {

		public DBEventDetails() { }

		[DBCol("EventID", SqlDbType.BigInt, 1, false)]
		public Int64 EventID { get; set; }

		[DBCol("Description", SqlDbType.NVarChar, 8000, true)]
		public string Description { get; set; }


		public static DBEventDetails CreateModel(IDataReader reader) {
			DBEventDetails model = new DBEventDetails();
			model.EventID = reader.GetInt64(nameof(EventID));
			model.Description = reader.GetString(nameof(Description));
			return model;
		}

	}

}
