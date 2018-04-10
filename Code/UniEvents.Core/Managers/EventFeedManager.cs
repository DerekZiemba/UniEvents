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

   public class EventFeedManager {
      private readonly Factory Ctx;

      internal EventFeedManager(Factory ctx) {
         this.Ctx = ctx;
      }


      public async Task<List<EventInfo>> EventSearch() {
         List<EventInfo> ls = new List<EventInfo>(10);
         using(SqlCommand cmd = new SqlCommand("[dbo].[sp_Event_Search]", new SqlConnection(Ctx.Config.dbUniHangoutsRead)) { CommandType = CommandType.StoredProcedure }){
            if (cmd.Connection.State != ConnectionState.Open) { await cmd.Connection.OpenAsync().ConfigureAwait(false); }
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
               while (await reader.ReadAsync().ConfigureAwait(false)) {
                  DBEventFeedItem  item = new DBEventFeedItem(reader);
                  EventInfo info = new EventInfo();
                  info.AccountID = item.AccountID;
                  info.EventID = item.EventID;
                  info.EventTypeID = item.EventTypeID;
                  info.DateStart = item.DateStart;
                  info.DateEnd = item.DateEnd;
                  info.LocationID = item.LocationID;
                  info.Title = item.Title;
                  info.Caption = item.Caption;
                  info.RSVP_Attending = item.RSVP_Attending;
                  info.RSVP_Later = item.RSVP_Later;
                  info.RSVP_StopBy = item.RSVP_StopBy;
                  info.RSVP_Maybe = item.RSVP_Maybe;
                  info.RSVP_No = item.RSVP_No;

                  info.Host = String.IsNullOrWhiteSpace(item.UserDisplayName) ? item.UserName : item.UserDisplayName;

                  info.LocationName = item.LocationName;
                  info.AddressLine = item.AddressLine;
                  info.AddressLine2 = Helpers.FormatAddress(null, null, item.Locality, item.AdminDistrict, item.PostalCode, item.CountryRegion);

                  info.EventType = Ctx.EventTypeManager[item.EventTypeID];
                  info.Tags = item.TagIds?.Select(x => Ctx.TagManager[x]).Where(x => x != null).ToArray();
                  ls.Add(info);
               }
            }
         }
         return ls;
      }

   }
}
