using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using UniEvents.Core;
using UniEvents.Models.DBModels;
using ZMBA;

namespace UniEvents.Models.ApiModels {

   public class RSVPType {
      public Int16 RSVPTypeID { get; set; }
      public string Name { get; set; }
      public string Description { get; set; }

      public RSVPType() { }

   }
}
