using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using ZMBA;

namespace UniEvents.Core.src.DBModels
{
    class DBTag
    {
        public DBTag() { }

        [DBCol("TagID", SqlDbType.Int, 1, false, true)]
        public Int64 TagID { get; set; }

        [DBCol("Name", SqlDbType.VarChar, 30, false, false)]
        public string Name { get; set; }

        [DBCol("Description", SqlDbType.NVarChar, 160, false, false)]
        public string Description { get; set; }

        public static DBTag CreateModel(IDataReader reader)
        {
            DBTag model = new DBTag();
            model.TagID = reader.GetInt64(nameof(TagID));
            model.Name = reader.GetString(nameof(Name));
            model.Description = reader.GetString(nameof(Description));
            return model;
        }
    }
}
