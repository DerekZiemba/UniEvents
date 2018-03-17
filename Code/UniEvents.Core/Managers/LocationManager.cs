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

         var result = new ApiResult<List<StreetAddress>>();
         result.Result = new List<StreetAddress>();
         try {
            foreach (DBModels.DBLocation loc in DBModels.DBLocation.SP_Locations_Search(Ctx, ParentLocationID: ParentLocationID, Name: Name, AddressLine: AddressLine, Locality: Locality, AdminDistrict: AdminDistrict, PostalCode: PostalCode, Description: Description)) {
               result.Result.Add(new StreetAddress(loc));
            }
            result.Success = true;
         } catch (Exception ex) {
            result.Message = ex.Message;
         }
         return result;
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
