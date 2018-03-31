using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UniEvents.Models.DBModels {

   public abstract class DBModel {

      [IgnoreDataMember()]
      public DateTime RetrievedOn { get; set; } = DateTime.UtcNow;

   }

}
