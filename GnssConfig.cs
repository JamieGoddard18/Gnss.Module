/* Configuration for the API. 
Holds tunable GNSS settings so they can be modified as opposed to hardcoded. */
namespace Ocad.Gnss
{
    public class GnssConfig
    {
        public int UpdateIntervalMs { get; set; } = 1000;

        public IGnssFilter Filter { get; set; } = new NoFilter();
        
        public double SmoothingAlpha { get; set; } = 0.3;

        public int SatelliteCountGood { get; set; } = 6;
        public int SatelliteCountExcellent { get; set; } = 12;

        public float Cn0Weak { get; set; } = 25f;
        public float Cn0Moderate { get; set; } = 30f;
        public float Cn0Strong { get; set; } = 35f;

        public float AccuracyLow { get; set; } = 5f;
        public float AccuracyGood { get; set; } = 3f;
        public float AccuracyExcellent { get; set; } = 1f;
    }
}