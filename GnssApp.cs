/* Holder for Gnss config and manager so test app can access both */

namespace Ocad.Gnss
{
    public static class GnssApp
    {
        public static GnssConfig Config { get; set; } = new GnssConfig();
        public static IGnssManager? Manager { get; set; }
    }
}