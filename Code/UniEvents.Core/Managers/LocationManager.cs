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
      private CoreContext Ctx;

		internal LocationManager(CoreContext ctx) {
         this.Ctx = ctx;
		}


      public ApiResult<List<StreetAddress>> SearchLocations(long? ParentLocationID = null,
                                                            string Name = null,
                                                            string AddressLine = null,
                                                            string Locality = null,
                                                            string AdminDistrict = null,
                                                            string PostalCode = null,
                                                            string Description = null) {

         var apiresult = new ApiResult<List<StreetAddress>>();
         apiresult.Result = new List<StreetAddress>();
         try {
            var models = DBModels.DBLocation.SP_Locations_Search(Ctx, ParentLocationID, Name, AddressLine, Locality, AdminDistrict, PostalCode, Description);
            foreach (DBModels.DBLocation loc in models) {
               apiresult.Result.Add(new StreetAddress(loc));
            }
            return apiresult.Win(apiresult.Result);

         } catch (Exception ex) {
            return apiresult.Failure(ex);
         }
      }


      public async Task<ApiResult<StreetAddress>> CreateLocation(StreetAddress address) {
         var result = new ApiResult<StreetAddress>();
         try {
            DBModels.DBLocation dbLocation = new DBModels.DBLocation(address);
            result.Success = await DBModels.DBLocation.SP_Location_CreateAsync(Ctx, dbLocation).ConfigureAwait(false);
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
