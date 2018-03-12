using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using ZMBA;

namespace UniEvents.Core.DBModels
{

    public class DBLogin
    {

        public DBLogin() { }

        [DBCol("AccountID", SqlDbType.BigInt, 1, false)]
        public Int64 AccountID { get; set; }

        [DBCol("UserName", SqlDbType.VarChar, 20, false)]
        public string UserName { get; set; }

        [DBCol("APIKey", SqlDbType.Binary, 256, false)]
        public byte[] APIKey { get; set; }

        [DBCol("LoginDate", SqlDbType.DateTime, 8, false)]
        public DateTime LoginDate { get; set; }

        public static DBLogin CreateModel(IDataReader reader)
        {
            DBLogin model = new DBLogin();
            model.AccountID = reader.GetInt64(nameof(AccountID));
            model.UserName = reader.GetString(nameof(UserName));
            model.APIKey = reader.GetBytes(nameof(APIKey), 32);
            model.LoginDate = reader.GetDateTime(nameof(LoginDate));
            return model;
        }


    }

}
