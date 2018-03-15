using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniEvents.Models.ApiModels {

   public interface IApiModel {
      bool Success { get; set; }
      string Message { get; set; }
   }

   public class APIModel : IApiModel {
      public bool Success { get; set; }
      public string Message { get; set; }
   }

   public class APIModel<T> : APIModel {
      public T Result { get; set; }
   }

}
