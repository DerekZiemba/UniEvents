using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using UniEvents.Core;
using UniEvents.Models.DBModels;


namespace UniEvents.Models.ApiModels {
   public class EventInfo {
      [JsonProperty(PropertyName = "id")] public Int64 EventID { get; set; }

      [JsonProperty(PropertyName = "time_start")] public DateTime DateStart { get; set; }
      [JsonProperty(PropertyName = "time_end")] public DateTime DateEnd { get; set; }
         
      public string Title { get; set; }
      public string Caption { get; set; }


      public long EventTypeID { get; set; }
      [JsonProperty(PropertyName = "event_type")] public DBModels.DBEventType EventType { get; set; }

      public Int64 LocationID { get; set; }

      [JsonProperty(PropertyName = "location")] public string LocationName { get; set; }
      [JsonProperty(PropertyName = "address")] public string AddressLine { get; set; }

      public Int64 AccountID { get; set; }
      public string Host { get; set; }

      public DBModels.DBTag[] Tags { get; set; }

      [JsonProperty(PropertyName = "rsvp_attending")] public long RSVP_Attending { get; set; }
      [JsonProperty(PropertyName = "rsvp_later")] public long RSVP_Later { get; set; }
      [JsonProperty(PropertyName = "rsvp_stopby")] public long RSVP_StopBy { get; set; }
      [JsonProperty(PropertyName = "rsvp_maybe")] public long RSVP_Maybe { get; set; }
      [JsonProperty(PropertyName = "rsvp_no")] public long RSVP_No { get; set; }


      public string Details { get; set; }



      public EventInfo() { }


      public EventInfo(Factory Ctx, DBEventFeedItem item) {
         AccountID = item.AccountID;
         EventID = item.EventID;
         EventTypeID = item.EventTypeID;
         DateStart = item.DateStart;
         DateEnd = item.DateEnd;
         LocationID = item.LocationID;
         Title = item.Title;
         Caption = item.Caption;
         RSVP_Attending = item.RSVP_Attending;
         RSVP_Later = item.RSVP_Later;
         RSVP_StopBy = item.RSVP_StopBy;
         RSVP_Maybe = item.RSVP_Maybe;
         RSVP_No = item.RSVP_No;

         Host = String.IsNullOrWhiteSpace(item.UserDisplayName) ? item.UserName : item.UserDisplayName;

         LocationName = item.LocationName;
         AddressLine = Helpers.FormatAddress(null, item.AddressLine, item.Locality, item.AdminDistrict, item.PostalCode, item.CountryRegion, ", ");

         EventType = Ctx.EventTypeManager[item.EventTypeID];
         Tags = item.TagIds?.Select(x => Ctx.TagManager[x]).Where(x => x != null).ToArray();
      }

   }



   public class EventInfoUserView : EventInfo {
      [JsonProperty(PropertyName = "user_rsvp_status")] public string UserRsvpStatus { get; set; }

      public EventInfoUserView(Factory Ctx, DBEventFeedItemExtended item) : base(Ctx, item) {
         Details = item.Details;
         if(item.UserRsvpID > 0) { UserRsvpStatus = Ctx.RSVPTypeManager[item.UserRsvpID].Name;}
        
      }

   }


}
