using System;
using System.Collections.Generic;
using System.Text;

namespace UniEvents.Core.CommonModels {

	public class StreetAddress {
		protected string _Name;
		protected string _AddressLine;
		protected string _Locality; //city
		protected string _AdminDistrict; //state
		protected string _PostalCode;
		protected string _CountryRegion;
		protected string _Description;

		//Unfortunately, unlike VisualBasic, C# properties don't allow pass by ref so we must expose the field. 
		public GPSCoordinate GPSCoords; 

		public StreetAddress() {}

		public StreetAddress(StreetAddress from) {
			_Name = from._Name;
			_AddressLine = from._AddressLine;
			_Locality = from._Locality;
			_AdminDistrict = from._AdminDistrict;
			_PostalCode = from._PostalCode;
			_CountryRegion = from._CountryRegion;
			GPSCoords = from.GPSCoords;
		}



	}



}
