using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using ZMBA;

namespace UniEvents.Models.DBModels
{

    public class DBAccountGroupMap
    {

        [DBCol("AccountID", SqlDbType.BigInt, 1, false)]
        public Int64 AccountID { get; set; }

        [DBCol("GroupID", SqlDbType.BigInt, 1, false)]
        public Int64 GroupID { get; set; }

        [DBCol("IsGroupOwner", SqlDbType.Bit, 1, false)]
        public bool IsGroup { get; set; }

        [DBCol("IsGroupAdmin", SqlDbType.Bit, 1, false)]
        public bool IsGroupAdmin { get; set; }

        [DBCol("IsGroupFriend", SqlDbType.Bit, 1, false)]
        public bool IsGroupFriend { get; set; }

        [DBCol("IsPendingFriend", SqlDbType.Bit, 1, false)]
        public bool IsPendingFriend { get; set; }

        [DBCol("IsGroupFollower", SqlDbType.Bit, 1, false)]
        public bool IsGroupFollower { get; set; }

        public DBAccountGroupMap() { }

        public DBAccountGroupMap(IDataReader reader)
        {
            DBAccountGroupMap model = new DBAccountGroupMap();
            model.AccountID = reader.GetInt64(nameof(AccountID));
            model.GroupID = reader.GetInt64(nameof(GroupID));
            model.IsGroup = reader.GetBoolean(nameof(IsGroup));
            model.IsGroupAdmin = reader.GetBoolean(nameof(IsGroupAdmin));
            model.IsGroupFriend = reader.GetBoolean(nameof(IsGroupFriend));
            model.IsPendingFriend = reader.GetBoolean(nameof(IsPendingFriend));
            model.IsGroupFollower = reader.GetBoolean(nameof(IsGroupFollower));
        }
    }

}
