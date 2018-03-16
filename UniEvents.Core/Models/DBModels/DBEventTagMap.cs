using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using ZMBA;

namespace UniEvents.Models.DBModels
{
    public class DBEventTagMap
    {
        public DBEventTagMap() { }

        [DBCol("EventID", SqlDbType.BigInt, 1, false)]
        public Int64 EventID { get; set; }

        [DBCol("TagID", SqlDbType.Int, 1, false)]
        public Int64 TagID { get; set; }

        public static DBEventTagMap CreateModel(IDataReader reader)
        {
            DBEventTagMap model = new DBEventTagMap();
            model.EventID = reader.GetInt64(nameof(EventID));
            model.TagID = reader.GetInt64(nameof(TagID));
            return model;
        }
    }
}
