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

      public DBEventType(SqlDataReader reader) {
         EventTypeID = reader.GetInt64(nameof(EventTypeID));
         Name = reader.GetString(nameof(Name));
         Description = reader.GetString(nameof(Description));
      }


      internal static SqlCommand GetSqlCommandForSP_Search(Factory ctx, long? EventTypeID = null, string Name = null, string Description = null) {
         SqlCommand cmd = new SqlCommand("[dbo].[sp_EventTypes_Search]", new SqlConnection(ctx.Config.dbUniHangoutsRead)) { CommandType = CommandType.StoredProcedure };
         cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(EventTypeID), EventTypeID);
         cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(Name), Name);
         cmd.AddParam(ParameterDirection.Input, SqlDbType.NVarChar, nameof(Description), Description);
         return cmd;
      }

      internal static SqlCommand GetSqlCommandForSP_GetOne(Factory ctx, long? EventTypeID = null, string Name = null) {
         SqlCommand cmd = new SqlCommand("[dbo].[sp_EventTypes_GetOne]", new SqlConnection(ctx.Config.dbUniHangoutsRead)) { CommandType = CommandType.StoredProcedure };
         cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(EventTypeID), EventTypeID);
         cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(Name), Name);
         return cmd;
      }



      internal static DBEventType SP_EventType_Create(Factory ctx, string Name, string Description) {
         using (SqlConnection conn = new SqlConnection(ctx.Config.dbUniHangoutsWrite))
         using (SqlCommand cmd = new SqlCommand("[dbo].[sp_EventTypes_Create]", conn) { CommandType = CommandType.StoredProcedure }) {
            var tagidParam = cmd.AddParam(ParameterDirection.Output, SqlDbType.BigInt, nameof(EventTypeID), null);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(Name), Name);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.NVarChar, nameof(Description), Description);

            int rowsAffected = cmd.ExecuteProcedure();
            long id = (Int64)tagidParam.Value;

            if (id > 0) {
               DBEventType result = new DBEventType(){
                  EventTypeID = id,
                  Name = Name,
                  Description = Description
               };
               return result;
            }
         }
         return null;
      }


   }

}
