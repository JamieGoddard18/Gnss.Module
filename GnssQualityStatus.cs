/* Holds the GNSS quality values for each metric
so they can be passed as one. */ 

namespace Ocad.Gnss
{
    public class GnssQualityStatus
    {
        public GnssQuality Signal { get; set; } = GnssQuality.Unavailable;

        public GnssQuality Satellites { get; set; } = GnssQuality.Unavailable;

        public GnssQuality Accuracy { get; set; } = GnssQuality.Unavailable;
    }
}