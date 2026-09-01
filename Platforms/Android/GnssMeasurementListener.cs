/* Listener to receive raw GNSS measurements from Android's GnssMeasurementsEvent API.
Maps measurement data into a list of GnssRawMeasurement objects. */

using Android.Locations;
using AndroidConstellationType = Android.Locations.GnssConstellationType;

namespace Ocad.Gnss.Platforms.Android
{
    // Extends GnssMeasurementsEvent.Callback to receive raw measurement updates
    public class GnssMeasurementListener : GnssMeasurementsEvent.Callback
    {
        public event Action<List<GnssRawMeasurement>>? RawMeasurementsReceived;

        // Called by Android whenever a new set of raw measurements is available
        public override void OnGnssMeasurementsReceived(GnssMeasurementsEvent eventArgs)
        {
            var measurements = new List<GnssRawMeasurement>();

            foreach (var measurement in eventArgs.Measurements)
            {
                measurements.Add(new GnssRawMeasurement
                {
                    Svid = measurement.Svid,
                    ConstellationType = MapConstellationType(measurement.ConstellationType),
                    Cn0DbHz = measurement.Cn0DbHz,
                    AutomaticGainControlLevelDb = measurement.AutomaticGainControlLevelDb,
                    PseudorangeRateMetersPerSecond = measurement.PseudorangeRateMetersPerSecond,
                    AccumulatedDeltaRangeMeters = measurement.AccumulatedDeltaRangeMeters,
                    AccumulatedDeltaRangeValid = measurement.AccumulatedDeltaRangeState.HasFlag(AccumulatedDeltaRangeState.Valid)
                });
            }

            // Notify subscribers 
            RawMeasurementsReceived?.Invoke(measurements);
        }

        // Map Android constellation enum to shared enum
        private static GnssConstellationType MapConstellationType(AndroidConstellationType androidType)
        {
            return androidType switch
            {
                AndroidConstellationType.Gps     => GnssConstellationType.Gps,
                AndroidConstellationType.Sbas    => GnssConstellationType.Sbas,
                AndroidConstellationType.Glonass => GnssConstellationType.Glonass,
                AndroidConstellationType.Qzss    => GnssConstellationType.Qzss,
                AndroidConstellationType.Beidou  => GnssConstellationType.Beidou,
                AndroidConstellationType.Galileo => GnssConstellationType.Galileo,
                AndroidConstellationType.Irnss   => GnssConstellationType.Irnss,
                                               _ => GnssConstellationType.Unknown
            };
        }
    }
}