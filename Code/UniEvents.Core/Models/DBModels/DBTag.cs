using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using UniEvents.Core;
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


      public static IEnumerable<DBTag> SP_Tags_Search(CoreContext ctx, int? TagID = null, string Name = null, string Description = null) {
         using (SqlConnection conn = new SqlConnection(ctx.Config.dbUniHangoutsRead))
         using (SqlCommand cmd = new SqlCommand("[dbo].[sp_Tags_Search]", conn) { CommandType = CommandType.StoredProcedure }) {
            cmd.AddParam(ParameterDirection.Input, SqlDbType.Int, nameof(TagID), TagID);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.NVarChar, nameof(Name), Name);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.NVarChar, nameof(Description), Description);

            if (cmd.Connection.State != ConnectionState.Open) { cmd.Connection.Open(); }
            using (SqlDataReader reader = cmd.ExecuteReader()) {
               while (reader.Read()) {
                  yield return new DBTag(reader);
               }
            }
         }
      }


   }
}