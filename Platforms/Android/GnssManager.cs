/* Android implementation of IGnssManager.
Uses GnssLocationListener, GnssStatusListener, and GnssMeasurementListener
and passes Android's listener data to the shared IGnssManager interface. */

using Android.Locations;
using Android.OS;

namespace Ocad.Gnss.Platforms.Android
{
    public class GnssManager : Java.Lang.Object, IGnssManager
    {
        private LocationManager _locationManager;
        private GnssConfig _config;
        private IGnssFilter _filter;

        // Android listeners
        private GnssLocationListener? _locationListener;
        private GnssStatusListener? _statusListener;
        private GnssMeasurementListener? _measurementListener;

        // Latest data
        private GnssQualityStatus _currentQuality = new();
        private List<GnssSatelliteInfo> _lastSatellites = new();
        private GnssLocation? _lastLocation;

        // IGnssManager properties
        public bool IsRunning { get; private set; }
        public GnssQualityStatus CurrentQuality => _currentQuality;

        // IGnssManager events
        public event Action<GnssLocation>? LocationReceived;
        public event Action<GnssQualityStatus>? QualityChanged;
        public event Action<List<GnssSatelliteInfo>>? SatelliteStatusChanged;
        public event Action<List<GnssRawMeasurement>>? RawMeasurementsReceived;

        public GnssManager(LocationManager locationManager, GnssConfig? config = null)
        {
            _locationManager = locationManager;
            _config = config ?? new GnssConfig(); // Use default if none given
            _filter = config.Filter;
        }

        // Start GNSS listeners and begin receiving updates
        public void Start()
        {
            if (IsRunning)
                return;

            _filter = _config.Filter;
            _filter.Reset();

            _locationListener = new GnssLocationListener();
            _statusListener = new GnssStatusListener();
            _measurementListener = new GnssMeasurementListener();

            _locationListener.LocationReceived += OnLocationReceived;
            _statusListener.SatelliteStatusReceived += OnSatelliteStatusReceived;
            _measurementListener.RawMeasurementsReceived += OnRawMeasurementsReceived;

            // Handler for main thread
            var handler = new Handler(Looper.MainLooper!);

            // Register the listeners with Android's LocationManager
            _locationManager.RequestLocationUpdates(LocationManager.GpsProvider, _config.UpdateIntervalMs, 0f, _locationListener);
            _locationManager.RegisterGnssStatusCallback(_statusListener, handler);
            _locationManager.RegisterGnssMeasurementsCallback(_measurementListener, handler);

            IsRunning = true;
        }

        public void Stop()
        {
            if (!IsRunning)
                return;

            // Unsubscribe handlers, then unregister from Android
            if (_locationListener != null)
            {
                _locationListener.LocationReceived -= OnLocationReceived;
                _locationManager.RemoveUpdates(_locationListener);
            }

            if (_statusListener != null)
            {
                _statusListener.SatelliteStatusReceived -= OnSatelliteStatusReceived;
                _locationManager.UnregisterGnssStatusCallback(_statusListener);
            }

            if (_measurementListener != null)
            {
                _measurementListener.RawMeasurementsReceived -= OnRawMeasurementsReceived;
                _locationManager.UnregisterGnssMeasurementsCallback(_measurementListener);
            }

            _locationListener = null;
            _statusListener = null;
            _measurementListener = null;

            // Reset
            _lastLocation = null;
            _lastSatellites = new();
            _currentQuality = new GnssQualityStatus();
            IsRunning = false;
        }

        // New location arrived
        private void OnLocationReceived(GnssLocation location)
        {
            _lastLocation = _filter.Process(location);
            LocationReceived?.Invoke(_lastLocation);
            EvaluateQuality();
        }

        // New satellite data arrived
        private void OnSatelliteStatusReceived(List<GnssSatelliteInfo> satellites)
        {
            _lastSatellites = satellites;
            SatelliteStatusChanged?.Invoke(satellites);
            EvaluateQuality();
        }

        // Raw measurements arrived 
        private void OnRawMeasurementsReceived(List<GnssRawMeasurement> measurements)
        {
            RawMeasurementsReceived?.Invoke(measurements);
        }

        // Re-evaluate quality and fire the event
        private void EvaluateQuality()
        {
            _currentQuality = GnssQualityEvaluator.Evaluate(_lastSatellites, _lastLocation, _config);
            QualityChanged?.Invoke(_currentQuality);
        }
    }
}