using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using static ZMBA.Common;

namespace UniEvents.Core.DBModels {

	public class DBLocation  {
		private Int64 _LocationID;
		private Int64? _ParentLocationID;
		private string _Name;
		private string _AddressLine;
		private string _Locality; //city
		private string _AdminDistrict; //state
		private string _PostalCode;
		private string _CountryRegion;
		private int? _Latitude6x;
		private int? _Longitude6x;
		private string _Description;

		public DBLocation() { }

		public DBLocation(DBLocation clone) {
			CopyFieldsShallow(this, clone);
		}

		public Int64 LocationID { get => _LocationID; set => _LocationID = value; }
		public Int64? ParentLocationID { get => _ParentLocationID; set => _ParentLocationID = value; }
		public string Name { get => _Name; set => _Name = value; }
		public string AddressLine { get => _AddressLine; set => _AddressLine = value; }
		public string Locality { get => _Locality; set => _Locality = value; }
		public string AdminDistrict { get => _AdminDistrict; set => _AdminDistrict = value; }
		public string PostalCode { get => _PostalCode; set => _PostalCode = value; }
		public string CountryRegion { get => _CountryRegion; set => _CountryRegion = value; }
		public int? Latitude6x { get => _Latitude6x; set => _Latitude6x = value; }
		public int? Longitude6x { get => _Longitude6x; set => _Longitude6x = value; }
		public string Description { get => _Description; set => _Description = value; }


		public double Latitude { get => _Latitude6x.UnBox()/10e6; set => _Latitude6x = (int)(value*10e6); }
		public double Longitude { get => _Longitude6x.UnBox()/ 10e6; set => _Longitude6x = (int)(value*10e6); }

		public static DBLocation CreateModel(IDataReader reader) {
			DBLocation model = new DBLocation();
			reader.PullValue(nameof(LocationID), out model._LocationID);
			reader.PullValue(nameof(ParentLocationID), out model._ParentLocationID);
			reader.PullValue(nameof(Name), out model._Name);
			reader.PullValue(nameof(AddressLine), out model._AddressLine);
			reader.PullValue(nameof(Locality), out model._Locality);
			reader.PullValue(nameof(AdminDistrict), out model._AdminDistrict);
			reader.PullValue(nameof(PostalCode), out model._PostalCode);
			reader.PullValue(nameof(CountryRegion), out model._CountryRegion);
			reader.PullValue(nameof(Latitude6x), out model._Latitude6x);
			reader.PullValue(nameof(Longitude6x), out model._Longitude6x);
			reader.PullValue(nameof(Description), out model._Description);
			return model;
		}

		
	}

}
