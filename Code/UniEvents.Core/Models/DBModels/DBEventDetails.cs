using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using ZMBA;


namespace UniEvents.Models.DBModels {

   public class DBEventDetails : DBModel {


      [DBCol("EventID", SqlDbType.BigInt, 1, false)]
      public Int64 EventID { get; set; }

      [DBCol("Description", SqlDbType.NVarChar, 8000, true)]
      public string Description { get; set; }

      public DBEventDetails() { }

      public DBEventDetails(IDataReader reader) {
         EventID = reader.GetInt64(nameof(EventID));
         Description = reader.GetString(nameof(Description));
      }

   }

}
