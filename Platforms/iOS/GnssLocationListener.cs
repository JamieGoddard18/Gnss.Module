/* iOS location listener. Receives CLLocation updates from Core Location
and maps them into the module's shared GnssLocation model.
Apple does not expose satellite or raw measurement data.*/

using CoreLocation;

namespace Ocad.Gnss.Platforms.iOS
{
    // Extends CLLocationManagerDelegate to receive Core Location callbacks
    public class GnssLocationListener : CLLocationManagerDelegate
    {
        // Fires when a new location is received 
        public event Action<GnssLocation>? LocationReceived;

        // Called by Core Location whenever new locations are available
        public override void LocationsUpdated(CLLocationManager manager, CLLocation[] locations)
        {
            // Core Location return an array - latest addition is the newest location
            var coreLocation = locations.LastOrDefault();

            // No locations - return
            if (coreLocation == null) 
                return;
            
            // Create location object
            var newLocation = new GnssLocation {
                // Guaranteed fields
                Latitude = coreLocation.Coordinate.Latitude,
                Longitude = coreLocation.Coordinate.Longitude,
                HorizontalAccuracy = (float)coreLocation.HorizontalAccuracy, // HorizontalAccuracy is negative if the fix is invalid
                Timestamp = DateTime.UtcNow
            };

            // Optional fields
            // Altitude (VerticalAccuracy is negative if altitude is invalid)
            if (coreLocation.VerticalAccuracy > 0) {
                newLocation.Altitude = coreLocation.Altitude;
                newLocation.VerticalAccuracy = (float)coreLocation.VerticalAccuracy;
            }

            // Speed (negative if invalid)
            if (coreLocation.Speed >= 0)
                newLocation.Speed = (float)coreLocation.Speed;

            // Course (bearing)
            if (coreLocation.Course >= 0)
                newLocation.Bearing = (float)coreLocation.Course;

            // iOS doesn't expose a provider so set manually
            newLocation.Provider = "CoreLocation";

            // Notify subscribers of event 
            LocationReceived?.Invoke(newLocation);
        }
    }
}
