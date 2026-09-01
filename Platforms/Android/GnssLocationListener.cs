/* Location listener to receive location updates from Android's LocationManager 
and convert them to GnssLocation objects. */

using Android.Locations;
using Android.OS;
using AndroidLocation = Android.Locations.Location; // Renamed as VS trying to match with different file (Android.Locations.Location and Ocad.Gnss.Platforms.Android)

namespace Ocad.Gnss.Platforms.Android
{
    public class GnssLocationListener : Java.Lang.Object, ILocationListener
    {
        public event Action<GnssLocation>? LocationReceived; 

        // Called by Android when a new location is available
        public void OnLocationChanged(AndroidLocation androidLocation)
        { 
            var location = new GnssLocation
            {
                Latitude = androidLocation.Latitude,
                Longitude = androidLocation.Longitude,
                HorizontalAccuracy = androidLocation.Accuracy,
                Timestamp = DateTime.UtcNow,
                Provider = androidLocation.Provider
            };

            if (androidLocation.HasAltitude)
                location.Altitude = androidLocation.Altitude;

            if (androidLocation.HasSpeed)
                location.Speed = androidLocation.Speed;

            if (androidLocation.HasBearing)
                location.Bearing = androidLocation.Bearing;

            if (androidLocation.HasVerticalAccuracy)
                location.VerticalAccuracy = androidLocation.VerticalAccuracyMeters;

            if (androidLocation.HasSpeedAccuracy)
                location.SpeedAccuracy = androidLocation.SpeedAccuracyMetersPerSecond;

            if (androidLocation.HasBearingAccuracy)
                location.BearingAccuracy = androidLocation.BearingAccuracyDegrees;

            // Notify subscribers of event 
            LocationReceived?.Invoke(location);
        }

        // Required by interface (not unused currently)
        public void OnStatusChanged(string? provider, Availability status, Bundle? extras) { }
        public void OnProviderEnabled(string provider) { }
        public void OnProviderDisabled(string provider) { }
    }
}

