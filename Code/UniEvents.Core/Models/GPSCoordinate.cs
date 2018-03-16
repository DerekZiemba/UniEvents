using System;
using System.Collections.Generic;
using System.Text;

namespace UniEvents.Models {

	public struct GPSCoordinate {
		public double Latitude;
		public double Longitude;

		public GPSCoordinate(double lat, double lon) {
			this.Latitude = lat;
			this.Longitude = lon;
			GPSCoordinate.Verify(this, true);
		}

		public bool IsValid => GPSCoordinate.Verify(this);

		public static bool Verify(GPSCoordinate coord, bool bthrow = false) => Verify(coord.Latitude, coord.Longitude, bthrow);

		public static bool Verify(double lat, double lon, bool bthrow = false) {
			if (Math.Abs(lat) > 90) {
				if (bthrow) { throw new OverflowException($"Invalid Latitude({lat}). Latitude must be from 0° to (+/–)90°."); }
				return false;
			}
			if (Math.Abs(lon) > 180) {
				if (bthrow) { throw new OverflowException($"Invalid Longitude({lon}). Longitude must be from 0° to (+/–)180°"); }
				return false;
			}
			return true;
		}

	}



}
