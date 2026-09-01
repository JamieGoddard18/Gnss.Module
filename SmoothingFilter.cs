/*
 * An exponential smoothing filter for GNSS locations.
 * The filter blends new readings with the previous smoothed estimate to reduce noise.
*/
namespace Ocad.Gnss
{
    public class SmoothingFilter : IGnssFilter
    {
        // Smoothing factor (0-1) - higher = responsive to change, lower = smoother
        private double _alpha;

        // Previous positions 
        private double? _smoothedLat;
        private double? _smoothedLng;

        // Default of 0.3 α for moderate smoothing
        public SmoothingFilter(double alpha = 0.3)
        {
            _alpha = alpha;
        }

        public GnssLocation Process(GnssLocation raw)
        {
            // First reading nothing to smooth against 
            if (_smoothedLat == null || _smoothedLng == null)
            {
                _smoothedLat = raw.Latitude;
                _smoothedLng = raw.Longitude;
                return raw;
            }

            // Blend new reading with previous estimate - smoothed = α × new + (1−α) × previous
            _smoothedLat = _alpha * raw.Latitude + (1 - _alpha) * _smoothedLat.Value;
            _smoothedLng = _alpha * raw.Longitude + (1 - _alpha) * _smoothedLng.Value;

            // Return new location with smoothed coordinates, keep all other fields 
            return new GnssLocation
            {
                Latitude = _smoothedLat.Value,
                Longitude = _smoothedLng.Value,
                Altitude = raw.Altitude,
                HorizontalAccuracy = raw.HorizontalAccuracy,
                VerticalAccuracy = raw.VerticalAccuracy,
                Speed = raw.Speed,
                SpeedAccuracy = raw.SpeedAccuracy,
                Bearing = raw.Bearing,
                BearingAccuracy = raw.BearingAccuracy,
                Provider = raw.Provider,
                Timestamp = raw.Timestamp
            };
        }

        // Reset the filter, clear the smoothed values
        public void Reset()
        {
            _smoothedLat = null;
            _smoothedLng = null;
        }
    }
}