/* 
 * Raw GNSS measurements from a single satellite (Android only)
*/ 

namespace Ocad.Gnss
{
    public class GnssRawMeasurement
    {
        public int Svid { get; set; }
        public GnssConstellationType ConstellationType { get; set; }
        public double Cn0DbHz { get; set; }
        public double AutomaticGainControlLevelDb { get; set; }
        public double PseudorangeRateMetersPerSecond { get; set; }        
        public double? AccumulatedDeltaRangeMeters { get; set; }
        public bool AccumulatedDeltaRangeValid { get; set; }
    }
}
