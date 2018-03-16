using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using ZMBA;

namespace UniEvents.Models.DBModels
{

    public class DBEventType
    {

        [DBCol("EventTypeID", SqlDbType.BigInt, 1, false, isAutoValue: true)]
        public Int64 EventTypeID { get; set; }

        [DBCol("Name", SqlDbType.VarChar, 50, false)]
        public string Name { get; set; }

        [DBCol("Description", SqlDbType.NVarChar, 400, true)]
        public string Description { get; set; }

        public DBEventType() { }

        public DBEventType(IDataReader reader)
        {
            DBEventType model = new DBEventType();
            model.EventTypeID = reader.GetInt64(nameof(EventTypeID));
            model.Name = reader.GetString(nameof(Name));
            model.Description = reader.GetString(nameof(Description));
        }
    }

}
