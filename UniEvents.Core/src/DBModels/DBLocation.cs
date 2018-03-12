using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using ZMBA;

namespace UniEvents.Core.DBModels {

	public class DBLocation  {

		public DBLocation() { }

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

		public static DBLocation CreateModel(IDataReader reader) {
			DBLocation model = new DBLocation();
			model.LocationID = reader.GetInt64(nameof(LocationID));
			model.ParentLocationID = reader.GetNInt64(nameof(ParentLocationID));
			model.Name = reader.GetString(nameof(Name));
			model.AddressLine = reader.GetString(nameof(AddressLine));
			model.Locality = reader.GetString(nameof(Locality));
			model.AdminDistrict = reader.GetString(nameof(AdminDistrict));
			model.PostalCode = reader.GetString(nameof(PostalCode));
			model.CountryRegion = reader.GetString(nameof(CountryRegion));
			model.Latitude6x = reader.GetNInt32(nameof(Latitude6x));
			model.Longitude6x = reader.GetNInt32(nameof(Longitude6x));
			model.Description = reader.GetString(nameof(Description));
			return model;
		}


	}

}
