/* Main interface for the module.
The point where the app can interact with the GNSS API.
Platform specific implementations found in Platforms/Android and Platforms/iOS folders. */ 

namespace Ocad.Gnss
{
    public interface IGnssManager
    {
        // Start receiving GNSS updates
        void Start();

        // Stop receiving GNSS updates
        void Stop();

        // Whether the service is currently running
        bool IsRunning {get;}

        // Latest evaluated quality
        GnssQualityStatus CurrentQuality {get;} 

        // Fired when a new location is available
        event Action<GnssLocation> LocationReceived;

        // Fired when quality is re-evaluated
        event Action<GnssQualityStatus> QualityChanged;

        // Fired when satellite view changes
        event Action<List<GnssSatelliteInfo>> SatelliteStatusChanged;

        // Fired when new raw measurement values are available
        event Action<List<GnssRawMeasurement>> RawMeasurementsReceived;
    }
}