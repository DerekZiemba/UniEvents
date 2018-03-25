using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using UniEvents.Core;
using ZMBA;

namespace UniEvents.Models.DBModels {
   public class DBRSVPType {


      [DBCol("RSVPTypeID", SqlDbType.SmallInt, 1, false, isAutoValue: true)]
      public Int16 RSVPTypeID { get; set; }

      [DBCol("Name", SqlDbType.Char, 10, false)]
      public string Name { get; set; }

      [DBCol("Description", SqlDbType.NChar, 40, false)]
      public string Description { get; set; }

      public DBRSVPType() { }

      public DBRSVPType(IDataReader reader) {
         RSVPTypeID = reader.GetInt16(nameof(RSVPTypeID));
         Name = reader.GetString(nameof(Name));
         Description = reader.GetString(nameof(Description));
      }


      public static IEnumerable<DBRSVPType> SP_RSVPTypes_Get(CoreContext ctx) {
         using (SqlConnection conn = new SqlConnection(ctx.Config.dbUniHangoutsConfiguration))
         using (SqlCommand cmd = new SqlCommand("[dbo].[sp_RSVPTypes_Get]", conn) { CommandType = CommandType.StoredProcedure }) {
            if (cmd.Connection.State != ConnectionState.Open) { cmd.Connection.Open(); }
            using (SqlDataReader reader = cmd.ExecuteReader()) {
               while (reader.Read()) {
                  yield return new DBRSVPType(reader);
               }
            }
         }
      }


   }
}
