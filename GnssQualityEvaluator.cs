/* Evaluates overall GNSS signal quality based on satellite count and signal strength.
Thresholds are supplied via GnssConfig. */
namespace Ocad.Gnss
{
    public static class GnssQualityEvaluator
    {
        private const int MinSatellites = 4; // Minimum satellites needed for a 3D fix

        public static GnssQualityStatus Evaluate(List<GnssSatelliteInfo> satellites, GnssLocation? location, GnssConfig config)
        {
            return new GnssQualityStatus
            {
                Signal = EvaluateSignal(satellites, config),
                Satellites = EvaluateSatellites(satellites, config),
                Accuracy = EvaluateAccuracy(location, config)
            };
        }

        // Signal strength from average C/N0
        private static GnssQuality EvaluateSignal(List<GnssSatelliteInfo> satellites, GnssConfig config)
        {
            // No satellite data (iOS)
            if (satellites == null || satellites.Count == 0)
                return GnssQuality.Unavailable;

            // Prefer satellites used in the fix but fall back to all in view
            bool haveUsedInFix = satellites.Any(s => s.UsedInFix);
            float averageCn0 = satellites
                .Where(s => !haveUsedInFix || s.UsedInFix)
                .Average(s => s.Cn0DbHz);

            if (averageCn0 >= config.Cn0Strong) return GnssQuality.Excellent;
            if (averageCn0 >= config.Cn0Moderate) return GnssQuality.Good;
            if (averageCn0 >= config.Cn0Weak) return GnssQuality.Low;
            return GnssQuality.Lost;
        }

        // Quality from satellite count
        private static GnssQuality EvaluateSatellites(List<GnssSatelliteInfo> satellites, GnssConfig config)
        {
            if (satellites == null || satellites.Count == 0)
                return GnssQuality.Unavailable;

            bool haveUsedInFix = satellites.Any(s => s.UsedInFix);
            int count = haveUsedInFix ? satellites.Count(s => s.UsedInFix) : satellites.Count;

            if (count < MinSatellites) return GnssQuality.Lost;
            if (count >= config.SatelliteCountExcellent) return GnssQuality.Excellent;
            if (count >= config.SatelliteCountGood) return GnssQuality.Good;
            return GnssQuality.Low;
        }

        // Quality from horizontal accuracy 
        private static GnssQuality EvaluateAccuracy(GnssLocation? location, GnssConfig config)
        {
            if (location is null)
                return GnssQuality.Unavailable;

            float acc = location.HorizontalAccuracy;
            if (acc <= config.AccuracyExcellent) return GnssQuality.Excellent;
            if (acc <= config.AccuracyGood) return GnssQuality.Good;
            if (acc <= config.AccuracyLow) return GnssQuality.Low;
            return GnssQuality.Lost;
        }
    }
}