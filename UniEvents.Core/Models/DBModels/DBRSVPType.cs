using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using ZMBA;

namespace UniEvents.Models.DBModels
{
    public class DBRSVPType
    {
        public DBRSVPType() { }

        [DBCol("RSVPTypeID", SqlDbType.SmallInt, 1, false, isAutoValue: true)]
        public Int16 RSVPTypeID { get; set; }

        [DBCol("Name", SqlDbType.Char, 10, false)]
        public string Name { get; set; }

        [DBCol("Description", SqlDbType.NChar, 40, false)]
        public string Description { get; set; }

        public static DBRSVPType CreateModel(IDataReader reader)
        {
            DBRSVPType model = new DBRSVPType();
            model.RSVPTypeID = reader.GetInt16(nameof(RSVPTypeID));
            model.Name = reader.GetString(nameof(Name));
            model.Description = reader.GetString(nameof(Description));
            return model;
        }
    }
}
