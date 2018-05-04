using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

using ZMBA;

namespace UniEvents.Models.DBModels {

   public class DBEventRSVP : DBModel {

      [DBCol("EventID", SqlDbType.BigInt, 1, false)]
      public Int64 EventID { get; set; }

      [DBCol("AccountID", SqlDbType.BigInt, 1, false)]
      public Int64 AccountID { get; set; }

      [DBCol("RSVPTypeId", SqlDbType.SmallInt, 1, false)]
      public Int16 RSVPTypeID { get; set; }

      public DBEventRSVP() { }

      public DBEventRSVP(SqlDataReader reader) {
         EventID = reader.GetInt64(nameof(EventID));
         AccountID = reader.GetInt64(nameof(AccountID));
         RSVPTypeID = reader.GetInt16(nameof(RSVPTypeID));
      }
   }

}
