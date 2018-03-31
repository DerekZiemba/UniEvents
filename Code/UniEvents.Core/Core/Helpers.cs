using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZMBA;


namespace UniEvents.Core {

   public static class Helpers {

      internal static void BlockUntilFinished(ref Task task) {
         if (task == null) { return; }
         if (!(task?.IsCompleted).UnBox()) {
            if ((task?.IsFaulted).UnBox()) {
               var ex = task?.Exception;
               if(ex != null) { throw ex; }
            }
            task?.ConfigureAwait(false).GetAwaiter().GetResult();
         }
         task?.Dispose();
         task = null;
      }

      internal static T BlockUntilFinished<T>(ref Task task, ref T value) {
         BlockUntilFinished(ref task);
         return value;
      }



      public static string FormatAddress(string sName, string sAddressline, string sCity, string sState, string sZip, string sCountry, string lineSeparator = ", \n") {
         return new [] {   sName,
                           sAddressline,
                           new [] { sCity,
                                    new [] { sState, sZip }.ToStringJoin(" "),
                                    sCountry }.ToStringJoin(", "),
                     }.ToStringJoin(lineSeparator);
      }


   }

}
