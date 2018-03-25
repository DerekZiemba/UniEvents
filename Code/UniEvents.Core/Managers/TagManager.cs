using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


using static ZMBA.Common;
using UniEvents.Core;
using UniEvents.Models.ApiModels;
using ApiModels = UniEvents.Models.ApiModels;
using DBModels = UniEvents.Models.DBModels;


namespace UniEvents.Managers {

	public class TagManager {
      private readonly CoreContext Ctx;


		internal TagManager(CoreContext ctx) {
         this.Ctx = ctx;

		}


   }
}
