using System;
using System.Collections.Generic;
using System.Text;

namespace UniEvents.Core.ApiModels {

	public class StreetAddress {
		private string name;
		private string addressLine;
		private string locality; //city
		private string adminDistrict; //state
		private string postalCode;
		private string countryRegion;
		private string description;

		//Unfortunately, unlike VisualBasic, C# properties don't allow pass by ref so we must expose the field. 
		public GPSCoordinate GPSCoords;

		public string Name { get => name; set => name = value; }
		public string AddressLine { get => addressLine; set => addressLine = value; }
		public string Locality { get => locality; set => locality = value; }
		public string AdminDistrict { get => adminDistrict; set => adminDistrict = value; }
		public string PostalCode { get => postalCode; set => postalCode = value; }
		public string CountryRegion { get => countryRegion; set => countryRegion = value; }
		public string Description { get => description; set => description = value; }

		public StreetAddress() {}

		public StreetAddress(StreetAddress from) {
			Name = from.Name;
			AddressLine = from.AddressLine;
			Locality = from.Locality;
			AdminDistrict = from.AdminDistrict;
			PostalCode = from.PostalCode;
			CountryRegion = from.CountryRegion;
			GPSCoords = from.GPSCoords;
		}

		public StreetAddress(DBModels.DBLocation from) {
			Name = from.Name;
			AddressLine = from.AddressLine;
			Locality = from.Locality;
			AdminDistrict = from.AdminDistrict;
			PostalCode = from.PostalCode;
			CountryRegion = from.CountryRegion;
			Description = from.Description;
			GPSCoords = new GPSCoordinate(from.Latitude, from.Longitude);
		}



		public override string ToString() => FormatAddress(Name, AddressLine, Locality, AdminDistrict, PostalCode, CountryRegion);


		public static string FormatAddress(string sName, string sAddressline, string sCity, string sState, string sZip, string sCountry, string lineSeparator = ", \n") {
			//This is a non performant call because ToStringJoin uses LINQ. But so much easier than alternatives. 
			return new string[] {   sName,
											sAddressline,
											new string[] { sCity,
																new string[] { sState, sZip }.ToStringJoin(" "),
																sCountry }.ToStringJoin(", "),
							}.ToStringJoin(lineSeparator);
		}


	}
}
