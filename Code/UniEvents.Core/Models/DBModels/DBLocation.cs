using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniEvents.Core;
using ZMBA;

namespace UniEvents.Models.DBModels {

	public class DBLocation  {

		[DBCol("LocationID", SqlDbType.BigInt, 1, false, isAutoValue: true)]
		public Int64 LocationID { get; set; }

		[DBCol("ParentLocationID", SqlDbType.BigInt, 1, true)]
		public Int64? ParentLocationID { get; set; }

		[DBCol("Name", SqlDbType.VarChar, 80, true)]
		public string Name { get; set; }

		[DBCol("AddressLine", SqlDbType.VarChar, 80, true)]
		public string AddressLine { get; set; }

		[DBCol("Locality", SqlDbType.VarChar, 40, true)]
		public string Locality { get; set; }

		[DBCol("AdminDistrict", SqlDbType.VarChar, 40, true)]
		public string AdminDistrict { get; set; }

		[DBCol("PostalCode", SqlDbType.VarChar, 20, true)]
		public string PostalCode { get; set; }

		[DBCol("CountryRegion", SqlDbType.VarChar, 40, false)]
		public string CountryRegion { get; set; }

		[DBCol("Latitude6x", SqlDbType.Int, 1, true)]
		public int? Latitude6x { get; set; }

		[DBCol("Longitude6x", SqlDbType.Int, 1, true)]
		public int? Longitude6x { get; set; }

		[DBCol("Description", SqlDbType.VarChar, 160, true)]
		public string Description { get; set; }

		public double Latitude { get => Latitude6x.UnBox()/10e6; set => Latitude6x = (int)(value*10e6); }
		public double Longitude { get => Longitude6x.UnBox()/ 10e6; set => Longitude6x = (int)(value*10e6); }

		public DBLocation() { }

      public DBLocation(ApiModels.StreetAddress other) {
         Name = other.Name;
         AddressLine = other.AddressLine;
         Locality = other.Locality;
         AdminDistrict = other.AdminDistrict;
         PostalCode = other.PostalCode;
         CountryRegion = other.CountryRegion;
         Description = other.Description;
         Latitude = other.Latitude;
         Longitude = other.Longitude;
      }

      private DBLocation(IDataReader reader) {
			LocationID = reader.GetInt64(nameof(LocationID));
			ParentLocationID = reader.GetNInt64(nameof(ParentLocationID));
			Name = reader.GetString(nameof(Name));
			AddressLine = reader.GetString(nameof(AddressLine));
			Locality = reader.GetString(nameof(Locality));
			AdminDistrict = reader.GetString(nameof(AdminDistrict));
			PostalCode = reader.GetString(nameof(PostalCode));
			CountryRegion = reader.GetString(nameof(CountryRegion));
			Latitude6x = reader.GetNInt32(nameof(Latitude6x));
			Longitude6x = reader.GetNInt32(nameof(Longitude6x));
			Description = reader.GetString(nameof(Description));
		}

      public static bool SP_Location_Create(CoreContext ctx, DBLocation model) {
         if(model == null) { throw new ArgumentNullException("DBLocation_Null"); }
         if (model.CountryRegion.IsNullOrWhitespace()) { throw new ArgumentNullException("CountryRegion cannot be empty"); }
         if (model.Latitude6x.HasValue ^ model.Longitude6x.HasValue) { throw new ArgumentException("Latitude and Longitude must both be null or both have a value."); }
         if (Math.Abs(model.Latitude) > 90) { throw new OverflowException("Latitude_Invalid"); }
         if (Math.Abs(model.Longitude) > 180) { throw new OverflowException("Longitude_Invalid"); }

         using (SqlConnection conn = new SqlConnection(ctx.Config.dbUniHangoutsWrite))
         using (SqlCommand cmd = new SqlCommand("[dbo].[sp_Location_Create]", conn) { CommandType = CommandType.StoredProcedure }) {
            SqlParameter @LocationID = cmd.AddParam(ParameterDirection.Output, SqlDbType.BigInt, nameof(@LocationID), null);
            SqlParameter @ParentLocationID = cmd.AddParam(ParameterDirection.InputOutput, SqlDbType.BigInt, nameof(@ParentLocationID), model.ParentLocationID);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@Name), model.@Name);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@AddressLine), model.@AddressLine);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@Locality), model.@Locality);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@AdminDistrict), model.@AdminDistrict);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@PostalCode), model.@PostalCode);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@CountryRegion), model.CountryRegion);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@Description), model.@Description);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.Real, nameof(@Latitude6x), model.@Latitude6x);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.Real, nameof(@Longitude6x), model.@Longitude6x);

            if (cmd.Connection.State != ConnectionState.Open) { cmd.Connection.Open(); }
            int rowsAffected = cmd.ExecuteNonQuery();
            model.LocationID = (long)@LocationID.Value;
            model.ParentLocationID = (long?)ParentLocationID.Value;
            return model.LocationID > 0;
         }
      }

      public static async Task<DBLocation> SP_Location_GetAsync(CoreContext ctx, long LocationID) {
         if (LocationID <= 0) { throw new ArgumentNullException("LocationID_Invalid"); }

			using (SqlConnection conn = new SqlConnection(ctx.Config.dbUniHangoutsRead))
			using (SqlCommand cmd = new SqlCommand("[dbo].[sp_Location_Get]", conn) { CommandType = CommandType.StoredProcedure }) {
				cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(DBLocation.@LocationID), LocationID);

				if (cmd.Connection.State != ConnectionState.Open) { await cmd.Connection.OpenAsync().ConfigureAwait(false); }
				using (SqlDataReader reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
					while (await reader.ReadAsync().ConfigureAwait(false)) {
						return new DBLocation(reader);
					}
				}
				return null;
			} 
		}


		public static async Task<bool> SP_Location_UpdateAsync(CoreContext ctx, DBLocation model) {
         if (model == null) { throw new ArgumentNullException("DBLocation_Null"); }
         if (model.LocationID <= 0) { throw new ArgumentNullException("LocationID_Invalid"); }
         if (model.CountryRegion.IsNullOrWhitespace()) { throw new ArgumentNullException("CountryRegion cannot be empty"); }
         if (model.Latitude6x.HasValue ^ model.Longitude6x.HasValue) { throw new ArgumentException("Latitude and Longitude must both be null or both have a value."); }
         if (Math.Abs(model.Latitude) > 90) { throw new OverflowException("Latitude_Invalid"); }
         if (Math.Abs(model.Longitude) > 180) { throw new OverflowException("Longitude_Invalid"); }
         
         using (SqlConnection conn = new SqlConnection(ctx.Config.dbUniHangoutsWrite))
			using (SqlCommand cmd = new SqlCommand("[dbo].[sp_Location_Update]", conn) { CommandType = CommandType.StoredProcedure }) {
				cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(@LocationID), model.LocationID);
				cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(@ParentLocationID), model.ParentLocationID);
				cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@Name), model.@Name);
				cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@AddressLine), model.@AddressLine);
				cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@Locality), model.@Locality);
				cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@AdminDistrict), model.@AdminDistrict);
				cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@PostalCode), model.@PostalCode);
				cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@CountryRegion), model.CountryRegion);
				cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@Description), model.@Description);
				cmd.AddParam(ParameterDirection.Input, SqlDbType.Real, nameof(@Latitude), model.@Latitude);
				cmd.AddParam(ParameterDirection.Input, SqlDbType.Real, nameof(@Longitude), model.@Longitude);

				if (cmd.Connection.State != ConnectionState.Open) { await cmd.Connection.OpenAsync().ConfigureAwait(false); }
				int rowsAffected = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
				return rowsAffected == 1;
			}
		}


		public static async Task<List<DBLocation>> SP_Locations_SearchAsync(CoreContext ctx, 
																									long? @ParentLocationID = null,
																									string @Name = null,
																									string @AddressLine = null,
																									string @Locality = null,
																									string @AdminDistrict = null,
																									string @PostalCode = null,
																									string @Description = null) {

			using (SqlConnection conn = new SqlConnection(ctx.Config.dbUniHangoutsRead))
			using (SqlCommand cmd = new SqlCommand("[dbo].[sp_Locations_Search]", conn) { CommandType = CommandType.StoredProcedure }) {
				cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(@ParentLocationID), @ParentLocationID);
				cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@Name), @Name);
				cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@AddressLine), @AddressLine);
				cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@Locality), @Locality);
				cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@AdminDistrict), @AdminDistrict);
				cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@PostalCode), @PostalCode);
				cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@Description), @Description);

				if (cmd.Connection.State != ConnectionState.Open) { await cmd.Connection.OpenAsync().ConfigureAwait(false); }

				var ls = new List<DBLocation>();
				using (SqlDataReader reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
					while (await reader.ReadAsync().ConfigureAwait(false)) {
						ls.Add(new DBLocation(reader));
					}
				}
				return ls;
			}
		}

		public static IEnumerable<DBLocation> SP_Locations_Search(CoreContext ctx,
																						long? @ParentLocationID = null,
																						string @Name = null,
																						string @AddressLine = null,
																						string @Locality = null,
																						string @AdminDistrict = null,
																						string @PostalCode = null,
																						string @Description = null) {

			using (SqlConnection conn = new SqlConnection(ctx.Config.dbUniHangoutsRead))
			using (SqlCommand cmd = new SqlCommand("[dbo].[sp_Locations_Search]", conn) { CommandType = CommandType.StoredProcedure }) {
				cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(@ParentLocationID), @ParentLocationID);
				cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@Name), @Name);
				cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@AddressLine), @AddressLine);
				cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@Locality), @Locality);
				cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@AdminDistrict), @AdminDistrict);
				cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@PostalCode), @PostalCode);
				cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@Description), @Description);

				if (cmd.Connection.State != ConnectionState.Open) { cmd.Connection.Open(); }
				using (SqlDataReader reader = cmd.ExecuteReader()) {
					while (reader.Read()) {
						yield return new DBLocation(reader);
					}
				}
			}
		}


	}

}
