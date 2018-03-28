using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using UniEvents.Core;
using ZMBA;


namespace UniEvents.Models.DBModels {

   public class DBEventType : DBModel {

      [DBCol("EventTypeID", SqlDbType.BigInt, 1, false, isAutoValue: true)]
      public Int64 EventTypeID { get; set; }

      [DBCol("Name", SqlDbType.VarChar, 50, false)]
      public string Name { get; set; }

      [DBCol("Description", SqlDbType.NVarChar, 400, true)]
      public string Description { get; set; }

      public DBEventType() { }

      public DBEventType(IDataReader reader) {
         EventTypeID = reader.GetInt64(nameof(EventTypeID));
         Name = reader.GetString(nameof(Name));
         Description = reader.GetString(nameof(Description));
      }

      public static IEnumerable<DBEventType> SP_EventTypes_Search(Factory ctx, long? EventTypeID = null, string Name = null, string Description = null) {
         using (SqlConnection conn = new SqlConnection(ctx.Config.dbUniHangoutsRead))
         using (SqlCommand cmd = new SqlCommand("[dbo].[sp_EventTypes_Search]", conn) { CommandType = CommandType.StoredProcedure }) {
            cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(EventTypeID), EventTypeID);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(Name), Name);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.NVarChar, nameof(Description), Description);

            if (cmd.Connection.State != ConnectionState.Open) { cmd.Connection.Open(); }
            using (SqlDataReader reader = cmd.ExecuteReader()) {
               while (reader.Read()) {
                  yield return new DBEventType(reader);
               }
            }
         }
      }


      public static DBEventType SP_EventType_Create(Factory ctx, string Name, string Description) {
         using (SqlConnection conn = new SqlConnection(ctx.Config.dbUniHangoutsWrite))
         using (SqlCommand cmd = new SqlCommand("[dbo].[sp_EventTypes_Create]", conn) { CommandType = CommandType.StoredProcedure }) {
            var tagidParam = cmd.AddParam(ParameterDirection.Output, SqlDbType.Int, nameof(EventTypeID), null);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(Name), Name);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.NVarChar, nameof(Description), Description);

            if (cmd.Connection.State != ConnectionState.Open) { cmd.Connection.Open(); }
            int rowsAffected = cmd.ExecuteNonQuery();

            DBEventType result = new DBEventType(){
               EventTypeID = (Int32)tagidParam.Value,
               Name = Name,
               Description = Description
            };
            if (result.EventTypeID > 0) {
               return result;
            }
         }
         return null;
      }


   }

}
