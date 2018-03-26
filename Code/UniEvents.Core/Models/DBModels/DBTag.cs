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

      [DBCol("TagID", SqlDbType.BigInt, 1, false, true)]
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


      public static IEnumerable<DBTag> SP_Tags_Search(CoreContext ctx, long? TagID = null, string Name = null, string Description = null) {
         using (SqlConnection conn = new SqlConnection(ctx.Config.dbUniHangoutsRead))
         using (SqlCommand cmd = new SqlCommand("[dbo].[sp_Tags_Search]", conn) { CommandType = CommandType.StoredProcedure }) {
            cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(TagID), TagID);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(Name), Name);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.NVarChar, nameof(Description), Description);

            if (cmd.Connection.State != ConnectionState.Open) { cmd.Connection.Open(); }
            using (SqlDataReader reader = cmd.ExecuteReader()) {
               while (reader.Read()) {
                  yield return new DBTag(reader);
               }
            }
         }
      }


      public static DBTag SP_Tags_Create(CoreContext ctx, string Name, string Description) {
         using (SqlConnection conn = new SqlConnection(ctx.Config.dbUniHangoutsWrite))
         using (SqlCommand cmd = new SqlCommand("[dbo].[sp_Tags_Create]", conn) { CommandType = CommandType.StoredProcedure }) {
            var tagidParam = cmd.AddParam(ParameterDirection.Output, SqlDbType.BigInt, nameof(TagID), null);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(Name), Name);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.NVarChar, nameof(Description), Description);

            if (cmd.Connection.State != ConnectionState.Open) { cmd.Connection.Open(); }
            int rowsAffected = cmd.ExecuteNonQuery();

            DBTag result = new DBTag(){
               TagID = (Int64)tagidParam.Value,
               Name = Name,
               Description = Description
            };
            if(result.TagID > 0) {
               return result;
            }
         }
         return null;
      }

   }
}