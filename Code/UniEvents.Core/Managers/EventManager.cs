using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


using ZMBA;
using UniEvents.Core;
using UniEvents.Models.ApiModels;
using UniEvents.Models.DBModels;


namespace UniEvents.Core.Managers {

   public class EventManager {
      private readonly Factory Ctx;

      internal EventManager(Factory ctx) {
         this.Ctx = ctx;
      }


      public async Task RemoveEventAsync(long EventID, long @AccountID) {
         using(SqlCommand cmd = new SqlCommand("[dbo].[sp_Event_Remove]", new SqlConnection(Ctx.Config.dbUniHangoutsWrite)) { CommandType = CommandType.StoredProcedure }) {
            cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(EventID), EventID);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(@AccountID), @AccountID);
            await cmd.ExecuteProcedureAsync().ConfigureAwait(false);
         }
      }



      public DBEventFeedItemExtended EventGetByID(long EventID) {
         using(SqlCommand cmd = new SqlCommand("[dbo].[sp_Event_GetById]", new SqlConnection(Ctx.Config.dbUniHangoutsRead)) { CommandType = CommandType.StoredProcedure }) {
            cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(EventID), EventID);
            return cmd.ExecuteReader_GetOne<DBEventFeedItemExtended>();
         }
      }


      public async Task<List<EventInfo>> EventSearch(long? EventID, long? EventTypeID, long? AccountID, long? LocationID, DateTime? @DateFrom, DateTime? @DateTo, string Title, string Caption) {
         List<EventInfo> ls = new List<EventInfo>(10);
         using(SqlCommand cmd = new SqlCommand("[dbo].[sp_Event_Search]", new SqlConnection(Ctx.Config.dbUniHangoutsRead)) { CommandType = CommandType.StoredProcedure }) {
            cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(EventID), EventID);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(EventTypeID), EventTypeID);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(AccountID), AccountID);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(LocationID), LocationID);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.SmallDateTime, nameof(@DateFrom), @DateFrom);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.SmallDateTime, nameof(@DateTo), @DateTo);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(Title), Title);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.NVarChar, nameof(Caption), Caption);

            if(cmd.Connection.State != ConnectionState.Open) { await cmd.Connection.OpenAsync().ConfigureAwait(false); }
            using(SqlDataReader reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
               while(await reader.ReadAsync().ConfigureAwait(false)) {
                  DBEventFeedItem  item = new DBEventFeedItem(reader);
                  EventInfo info = new EventInfo(Ctx, item);
                  ls.Add(info);
               }
            }
         }
         return ls;
      }


      public DBEventFeedItem CreateOrUpdate(long? EventID, long EventTypeID, DateTime DateStart, DateTime DateEnd, long AccountID, long LocationID, string Title, string Caption, string Details) {
         using(SqlCommand cmd = new SqlCommand("[dbo].[sp_Event_CreateOrUpdate]", new SqlConnection(Ctx.Config.dbUniHangoutsWrite)) { CommandType = CommandType.StoredProcedure }) {
            SqlParameter paramEventID = cmd.AddParam(ParameterDirection.Output, SqlDbType.BigInt, nameof(EventID), EventID);
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
               EventID = (Int64)paramEventID.Value,
               EventTypeID = EventTypeID,
               DateStart = DateStart,
               DateEnd = DateEnd,
               AccountID = AccountID,
               LocationID = LocationID,
               Title = Title,
               Caption = Caption
            };
            if(result.EventID > 0) {
               return result;
            }
         }
         return null;
      }


      public string GetEventDescription(long EventID) {
         using(SqlCommand cmd = new SqlCommand("[dbo].[sp_Event_Details_GetOne]", new SqlConnection(Ctx.Config.dbUniHangoutsRead)) { CommandType = CommandType.StoredProcedure }) {
            cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(EventID), EventID);
            if(cmd.Connection.State != ConnectionState.Open) { cmd.Connection.Open(); }
            using(SqlDataReader reader = cmd.ExecuteReader()) {
               if(reader.Read()) {
                  return reader.GetString("Details");
               }
            }
         }
         return "";
      }



   }
}
