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

			internal static async Task<long> CreateAsync(string CountryRegion,
																		long? ParentLocationID = null,
																		string Name = null,
																		string AddressLine = null,
																		string Locality = null,
																		string AdminDistrict = null,
																		string PostalCode = null,
																		double? Latitude = null,
																		double? Longitude = null,
																		string Description = null) {

				Contract.Requires<ArgumentNullException>(!CountryRegion.IsNullOrWhitespace(), "CountryRegion cannot be null");
				Contract.Requires<ArgumentException>(Latitude.HasValue ^ Longitude.HasValue, "Latitude and Longitude must both be null or both have a value.");

				using (SqlConnection conn = new SqlConnection(Settings.SqlDbUniHangoutsConnStr))
				using (SqlCommand cmd = new SqlCommand("[dbo].[sp_Location_Create]", conn) { CommandType = CommandType.StoredProcedure }) {
					cmd.Parameters.AddWithValue(ParameterDirection.Input, SqlDbType.VarChar, nameof(DBLocation.CountryRegion), CountryRegion);
					cmd.Parameters.AddWithValue(ParameterDirection.Input, SqlDbType.BigInt, nameof(DBLocation.ParentLocationID), ParentLocationID);
					cmd.Parameters.AddWithValue(ParameterDirection.Input, SqlDbType.VarChar, nameof(DBLocation.Name), Name);
					cmd.Parameters.AddWithValue(ParameterDirection.Input, SqlDbType.VarChar, nameof(DBLocation.AddressLine), AddressLine);
					cmd.Parameters.AddWithValue(ParameterDirection.Input, SqlDbType.VarChar, nameof(DBLocation.Locality), Locality);
					cmd.Parameters.AddWithValue(ParameterDirection.Input, SqlDbType.VarChar, nameof(DBLocation.AdminDistrict), AdminDistrict);
					cmd.Parameters.AddWithValue(ParameterDirection.Input, SqlDbType.VarChar, nameof(DBLocation.Description), Description);
					cmd.Parameters.AddWithValue(ParameterDirection.Input, SqlDbType.Real, nameof(DBLocation.Latitude), Latitude);
					cmd.Parameters.AddWithValue(ParameterDirection.Input, SqlDbType.Real, nameof(DBLocation.Longitude), Longitude);

					SqlParameter oLocationID = cmd.Parameters.AddWithValue(ParameterDirection.Output, SqlDbType.Real, nameof(DBLocation.LocationID), null);

					await cmd.Connection.OpenAsync().ConfigureAwait(false);
					int rowsAffected = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);

					return (long)oLocationID.Value;
				}
			}

			internal static async Task<DBLocation> GetAsync(long LocationID) {
				Contract.Requires<ArgumentNullException>(LocationID > 0, "LocationID must be greater than 0");

				using (SqlConnection conn = new SqlConnection(Settings.SqlDbUniHangoutsConnStr))
				using (SqlCommand cmd = new SqlCommand("[dbo].[sp_Location_Get]", conn) { CommandType = CommandType.StoredProcedure }) {
					cmd.Parameters.AddWithValue(ParameterDirection.Input, SqlDbType.BigInt, nameof(DBLocation.LocationID), LocationID);

					List<DBLocation> models = await cmd.ReadDataModelsAsync(DBLocation.CreateModel, 1);
					return models.FirstOrDefault();
				}
			}


		}
	}
}
