using System;
using System.Collections.Generic;
using System.Text;

namespace UniEvents.Models.ApiModels {
   public class EventInfo {
      public Int64 EventID { get; set; }
      
      public DateTime DateStart { get; set; }
      public DateTime DateEnd { get; set; }
         
      public string Title { get; set; }
      public string Caption { get; set; }


      public int EventTypeID { get; set; }
      public DBModels.DBEventType EventType { get; set; }

      public Int64 LocationID { get; set; }
      public StreetAddress Location { get; set; }


      public Int64 AccountID { get; set; }
      public UserAccount UserAccount { get; set; }

      public List<DBModels.DBTag> Tags { get; set; } = new List<DBModels.DBTag>();


      public EventInfo() { }


   }
}
