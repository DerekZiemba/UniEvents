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

	public class LocationManager {
      private Factory Ctx;

		internal LocationManager(Factory ctx) {
         this.Ctx = ctx;
		}


      public async Task<ApiResult<StreetAddress>> CreateLocation(StreetAddress address) {
         var result = new ApiResult<StreetAddress>();
         try {
            DBModels.DBLocation dbLocation = new DBModels.DBLocation(address);
            result.Success = await DBModels.DBLocation.SP_Locations_CreateOneAsync(Ctx, dbLocation).ConfigureAwait(false);
            result.Result = new StreetAddress(dbLocation);
            if (!result.Success) {
               result.Message = "Failed for Unknown Reason";
            }
         } catch (Exception ex) {
            result.Message = ex.Message;
         }
         return result;
      }


   }
}
