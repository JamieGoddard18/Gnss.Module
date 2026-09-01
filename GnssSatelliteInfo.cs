/* Constellation info (Android only) */ 

namespace Ocad.Gnss
{
    public class GnssSatelliteInfo
    {
        public int Svid { get; set; }
        public GnssConstellationType ConstellationType { get; set; }
        public float Cn0DbHz { get; set; }
        public float ElevationDegrees { get; set; }
        public float AzimuthDegrees { get; set; }
        public bool UsedInFix { get; set; }
    }
}
