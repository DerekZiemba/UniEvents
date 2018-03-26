using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniEvents.Core;
using ZMBA;


namespace UniEvents.Models.DBModels {

   public class DBEventFeedItem {


      [DBCol("EventID", SqlDbType.BigInt, 1, false, isAutoValue: true)]
      public Int64 EventID { get; set; }

      [DBCol("EventTypeID", SqlDbType.BigInt, 1, false)]
      public long EventTypeID { get; set; }

      [DBCol("DateStart", SqlDbType.SmallDateTime, 1, false)]
      public DateTime DateStart { get; set; }

      [DBCol("DateEnd", SqlDbType.SmallDateTime, 1, false)]
      public DateTime DateEnd { get; set; }

      [DBCol("AccountID", SqlDbType.BigInt, 1, false)]
      public Int64 AccountID { get; set; }

      [DBCol("LocationID", SqlDbType.BigInt, 1, false)]
      public Int64 LocationID { get; set; }

      [DBCol("Title", SqlDbType.VarChar, 80, false)]
      public string Title { get; set; }

      [DBCol("Caption", SqlDbType.NVarChar, 160, false)]
      public string Caption { get; set; }


      public DBEventFeedItem() { }

      public DBEventFeedItem(IDataReader reader) {
         EventID = reader.GetInt64(nameof(EventID));
         EventTypeID = reader.GetInt32(nameof(EventTypeID));
         DateStart = reader.GetDateTime(nameof(DateStart));
         DateEnd = reader.GetDateTime(nameof(DateEnd));
         AccountID = reader.GetInt64(nameof(AccountID));
         LocationID = reader.GetInt64(nameof(LocationID));
         Title = reader.GetString(nameof(Title));
         Caption = reader.GetString(nameof(Caption));
      }


      public static DBEventFeedItem SP_Event_Create(CoreContext ctx,
         long EventTypeID, DateTime DateStart, DateTime DateEnd, long AccountID, long LocationID, string Title, string Caption, string Details) {

         using (SqlConnection conn = new SqlConnection(ctx.Config.dbUniHangoutsWrite))
         using (SqlCommand cmd = new SqlCommand("[dbo].[sp_Event_Create]", conn) { CommandType = CommandType.StoredProcedure }) {
            SqlParameter @EventID = cmd.AddParam(ParameterDirection.Output, SqlDbType.BigInt, nameof(EventID), null);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(EventTypeID), EventTypeID);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.SmallDateTime, nameof(DateStart), DateStart);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.SmallDateTime, nameof(DateEnd), DateEnd);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(AccountID), AccountID);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(LocationID), LocationID);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(Title), Title);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.NVarChar, nameof(Caption), Caption);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.NVarChar, nameof(Details), Details);

            int rowsAffected = cmd.ExecuteProcedure();

            DBEventFeedItem result = new DBEventFeedItem(){
               EventID = (Int32)@EventID.Value,
               EventTypeID = EventTypeID,
               DateStart = DateStart,
               DateEnd = DateEnd,
               AccountID = AccountID,
               LocationID = LocationID,
               Title = Title,
               Caption = Caption
            };
            if (result.EventID > 0) {
               return result;
            }
         }
         return null;
      }


      public static IEnumerable<DBEventFeedItem> SP_Event_Search(CoreContext ctx,
         long? EventID = null,
         long? EventTypeID = null,
         long? AccountID = null,
         long? LocationID = null,
         DateTime? DateFrom = null, 
         DateTime? DateTo = null, 
         string Title = null,
         string Caption = null) {

         using (SqlConnection conn = new SqlConnection(ctx.Config.dbUniHangoutsRead))
         using (SqlCommand cmd = new SqlCommand("[dbo].[sp_Event_Search]", conn) { CommandType = CommandType.StoredProcedure }) {
            cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(EventID), null);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(EventTypeID), EventTypeID);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(AccountID), AccountID);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(LocationID), LocationID);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.SmallDateTime, nameof(DateFrom), DateFrom);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.SmallDateTime, nameof(DateTo), DateTo);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(Title), Title);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.NVarChar, nameof(Caption), Caption);

            foreach (var item in cmd.ExecuteReader_GetManyRecords()) { yield return new DBEventFeedItem(item); }
         }
      }
   }

}
