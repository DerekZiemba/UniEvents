using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMBA {

   public static partial class Common {

      internal struct TimeRecord<T> {
         public DateTime Time;
         public T Key;    
      }

   }

}
