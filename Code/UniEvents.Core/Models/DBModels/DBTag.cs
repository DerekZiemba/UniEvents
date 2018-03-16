using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using ZMBA;

namespace UniEvents.Models.DBModels {
   public class DBTag {

      [DBCol("TagID", SqlDbType.Int, 1, false, true)]
      public Int64 TagID { get; set; }

      [DBCol("Name", SqlDbType.VarChar, 30, false, false)]
      public string Name { get; set; }

      [DBCol("Description", SqlDbType.NVarChar, 160, false, false)]
      public string Description { get; set; }

      public DBTag() { }

      public DBTag(IDataReader reader) {
         TagID = reader.GetInt64(nameof(TagID));
         Name = reader.GetString(nameof(Name));
         Description = reader.GetString(nameof(Description));
      }

   }
}