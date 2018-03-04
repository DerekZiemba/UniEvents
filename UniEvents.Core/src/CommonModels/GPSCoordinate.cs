using System;
using System.Collections.Generic;
using System.Text;

namespace UniEvents.Core.CommonModels {

	public struct GPSCoordinate {
		public double Latitude;
		public double Longitude;

		public GPSCoordinate(double lat, double lon) {
			this.Latitude = lat;
			this.Longitude = lon;
			GPSCoordinate.Verify(this, true);
		}

		public bool IsValid => GPSCoordinate.Verify(this);


		public static bool Verify(GPSCoordinate coord, bool bthrow = false) {
			if (Math.Abs(coord.Latitude) > 90) {
				if (bthrow) { throw new OverflowException($"Invalid Latitude({coord.Latitude}). Latitude must be from 0° to (+/–)90°."); }
				return false;
			}
			if (Math.Abs(coord.Longitude) > 180) {
				if (bthrow) {	throw new OverflowException($"Invalid Longitude({coord.Longitude}). Longitude must be from 0° to (+/–)180°");	}
				return false;
			}
			return true;
		}
	}



}
