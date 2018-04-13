using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
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


      public EventInfo() { }



   }
}
