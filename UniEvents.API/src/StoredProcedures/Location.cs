using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniEvents.Core;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;


using UniEvents.Core.DBModels;
using static UniEvents.Core.Extensions;


namespace UniEvents.WebAPI {

	internal static partial class StoredProcedures {

		internal static class Location {

			internal static async Task<long> CreateAsync(DBLocation oLocation) {
				oLocation.LocationID = await CreateAsync(oLocation.CountryRegion, oLocation.ParentLocationID, oLocation.Name,
																		oLocation.AddressLine, oLocation.Locality, oLocation.AdminDistrict, oLocation.PostalCode,
																		oLocation.Latitude, oLocation.Longitude, oLocation.Description).ConfigureAwait(false);
				return oLocation.LocationID;
			}

			internal static async Task<long> CreateAsync(string @CountryRegion,
																		long? @ParentLocationID = null,
																		string @Name = null,
																		string @AddressLine = null,
																		string @Locality = null,
																		string @AdminDistrict = null,
																		string @PostalCode = null,
																		double? @Latitude = null,
																		double? @Longitude = null,
																		string @Description = null) {

				Contract.Requires<ArgumentNullException>(!CountryRegion.IsNullOrWhitespace(), "CountryRegion cannot be null");
				Contract.Requires<ArgumentException>(Latitude.HasValue ^ Longitude.HasValue, "Latitude and Longitude must both be null or both have a value.");

				SqlConnection conn = TakeDbUniHangoutsConn();

				try {
					using (SqlCommand cmd = new SqlCommand("[dbo].[sp_Location_Create]", conn) { CommandType = CommandType.StoredProcedure }) {
						cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@CountryRegion), @CountryRegion);
						cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(@ParentLocationID), @ParentLocationID);
						cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@Name), @Name);
						cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@AddressLine), @AddressLine);
						cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@Locality), @Locality);
						cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@AdminDistrict), @AdminDistrict);
						cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@PostalCode), @PostalCode);
						cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@Description), @Description);
						cmd.AddParam(ParameterDirection.Input, SqlDbType.Real, nameof(@Latitude), @Latitude);
						cmd.AddParam(ParameterDirection.Input, SqlDbType.Real, nameof(@Longitude), @Longitude);

						SqlParameter @LocationID = cmd.AddParam(ParameterDirection.Output, SqlDbType.Real, nameof(@LocationID), null);

						int rowsAffected = await cmd.ExecuteStoredProcedureAsync().ConfigureAwait(false);

						return (long)LocationID.Value;
					}
				} catch { throw; } finally { ReturnDbUniHangoutsConn(conn); }
			}

			internal static async Task<DBLocation> GetAsync(long LocationID) {
				Contract.Requires<ArgumentNullException>(LocationID > 0, "LocationID must be greater than 0");
				SqlConnection conn = TakeDbUniHangoutsConn();
				try {
					using (SqlCommand cmd = new SqlCommand("[dbo].[sp_Location_Get]", conn) { CommandType = CommandType.StoredProcedure }) {
						cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(DBLocation.LocationID), LocationID);

						List<DBLocation> models = await cmd.ReadDataModelsAsync(DBLocation.CreateModel, 1);
						return models.FirstOrDefault();
					}
				} catch { throw; } finally { ReturnDbUniHangoutsConn(conn); }
			}


			internal static async Task<bool> UpdateAsync(long @LocationID,
																				string @CountryRegion,
																				long? @ParentLocationID = null,
																				string @Name = null,
																				string @AddressLine = null,
																				string @Locality = null,
																				string @AdminDistrict = null,
																				string @PostalCode = null,
																				double? @Latitude = null,
																				double? @Longitude = null,
																				string @Description = null) {

				Contract.Requires<ArgumentNullException>(LocationID > 0, "LocationID must be greater than 0");
				Contract.Requires<ArgumentNullException>(!CountryRegion.IsNullOrWhitespace(), "CountryRegion cannot be null");
				Contract.Requires<ArgumentException>(Latitude.HasValue ^ Longitude.HasValue, "Latitude and Longitude must both be null or both have a value.");

				SqlConnection conn = TakeDbUniHangoutsConn();
				try {
					using (SqlCommand cmd = new SqlCommand("[dbo].[sp_Location_Update]", conn) { CommandType = CommandType.StoredProcedure }) {
						cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(@LocationID), @LocationID);
						cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@CountryRegion), @CountryRegion);
						cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(@ParentLocationID), @ParentLocationID);
						cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@Name), @Name);
						cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@AddressLine), @AddressLine);
						cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@Locality), @Locality);
						cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@AdminDistrict), @AdminDistrict);
						cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@PostalCode), @PostalCode);
						cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@Description), @Description);
						cmd.AddParam(ParameterDirection.Input, SqlDbType.Real, nameof(@Latitude), @Latitude);
						cmd.AddParam(ParameterDirection.Input, SqlDbType.Real, nameof(@Longitude), @Longitude);

						int rowsAffected = await cmd.ExecuteStoredProcedureAsync().ConfigureAwait(false);
						return rowsAffected > 0;
					}
				} catch { throw; } finally { ReturnDbUniHangoutsConn(conn); }
			}


		}
	}
}
