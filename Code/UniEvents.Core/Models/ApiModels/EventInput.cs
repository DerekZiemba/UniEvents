using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniEvents.Models.ApiModels {

   public class EventInput {
      public string Title { get; set; }
      public string Caption { get; set; }
      public string Description { get; set; }

      public Int64 EventTypeID { get; set; }
      public string[] Tags { get; set; }
      public DateTime DateStart { get; set; }
      public DateTime DateEnd { get; set; }
      

      public StreetAddress Location { get; set; }

   }
}
