/* Status Listener to receive satellite status updates from Android's GnssStatus API.
Maps satellite data into a list of GnssSatelliteInfo objects. */

using Android.Locations;
using AndroidConstellationType = Android.Locations.GnssConstellationType; 

namespace Ocad.Gnss.Platforms.Android
{
    public class GnssStatusListener : GnssStatus.Callback
    {
        public event Action<List<GnssSatelliteInfo>>? SatelliteStatusReceived;

        // Called by Android whenever satellite visibility changes
        public override void OnSatelliteStatusChanged(GnssStatus status)
        {
            var satellites = new List<GnssSatelliteInfo>();

            for (int i = 0; i < status.SatelliteCount; i++)
            {
                satellites.Add(new GnssSatelliteInfo
                {
                    Svid = status.GetSvid(i),
                    ConstellationType = MapConstellationType(status.GetConstellationType(i)),
                    Cn0DbHz = status.GetCn0DbHz(i),
                    ElevationDegrees = status.GetElevationDegrees(i),
                    AzimuthDegrees = status.GetAzimuthDegrees(i),
                    UsedInFix = status.UsedInFix(i)
                });
            }

            SatelliteStatusReceived?.Invoke(satellites);
        }

        // Maps Android's constellation value to ours
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
                                              _  => GnssConstellationType.Unknown
            };
        }
    }
}