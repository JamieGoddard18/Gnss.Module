/* GNSS Location Info */ 

namespace Ocad.Gnss
{
    public class GnssLocation
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public float HorizontalAccuracy { get; set; }
        public DateTime Timestamp { get; set; }
        public double? Altitude { get; set; }
        public float? Speed { get; set; }
        public float? Bearing { get; set; }
        public float? VerticalAccuracy { get; set; }
        public float? SpeedAccuracy { get; set; }
        public float? BearingAccuracy { get; set; }
        public string? Provider { get; set; }
    }
}
