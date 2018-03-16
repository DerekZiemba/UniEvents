using System;
using System.Collections.Generic;
using System.Text;

using static ZMBA.Common;

namespace UniEvents.Models.ApiModels {

	public class StreetAddress {
      public long? ParentLocationID { get; set; }
      public long? LocationID { get; set; }
      public string Name { get; set; }
		public string AddressLine { get; set; }
      public string Locality { get; set; }
      public string AdminDistrict { get; set; }
      public string PostalCode { get; set; }
      public string CountryRegion { get; set; }
      public string Description { get; set; }
      public double Latitude { get; set; }
      public double Longitude { get; set; }

      public StreetAddress() {}

      public StreetAddress(StreetAddress other) {
         ParentLocationID = other.ParentLocationID;
         LocationID = other.LocationID;
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
      public StreetAddress(DBModels.DBLocation other) {
         ParentLocationID = other.ParentLocationID;
         LocationID = other.LocationID;
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

      public bool IsValid() {
         if (CountryRegion.IsNullOrWhitespace()) return false;
         return true;
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
