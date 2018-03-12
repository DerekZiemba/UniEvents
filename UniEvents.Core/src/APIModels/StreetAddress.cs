using System;
using System.Collections.Generic;
using System.Text;

using static ZMBA.Common;

namespace UniEvents.Core.ApiModels {

	public class StreetAddress {
		private string _name;
		private string _addressLine;
		private string _locality; //city
		private string _adminDistrict; //state
		private string _postalCode;
		private string _countryRegion;
		private string _description;
		private float _latitude;
		private float _longitude;

		public string Name { get => _name; set => _name = value; }
		public string AddressLine { get => _addressLine; set => _addressLine = value; }
		public string Locality { get => _locality; set => _locality = value; }
		public string AdminDistrict { get => _adminDistrict; set => _adminDistrict = value; }
		public string PostalCode { get => _postalCode; set => _postalCode = value; }
		public string CountryRegion { get => _countryRegion; set => _countryRegion = value; }
		public string Description { get => _description; set => _description = value; }
		public float Latitude { get => _latitude; set => _latitude = value; }
		public float Longitude { get => _longitude; set => _longitude = value; }

		public StreetAddress() {}
 

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
