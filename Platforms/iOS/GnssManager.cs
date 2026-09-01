/* iOS implementation of IGnssManager.
Uses Core Location via CLLocationManager and GnssLocationListener.
Core Location provides no satellite or raw data, so those events never fire
and the signal/satellite quality metrics report Unavailable. */

using CoreLocation;

namespace Ocad.Gnss.Platforms.iOS
{
    public class GnssManager : IGnssManager
    {
        private CLLocationManager _locationManager;
        private GnssConfig _config;
        private IGnssFilter _filter;

        private GnssLocationListener? _locationListener;
        private GnssQualityStatus _currentQuality = new();
        private GnssLocation? _lastLocation;

        // IGnssManager properties
        public bool IsRunning { get; private set; }
        public GnssQualityStatus CurrentQuality => _currentQuality;

        // IGnssManager events
        public event Action<GnssLocation>? LocationReceived;
        public event Action<GnssQualityStatus>? QualityChanged;

        // Never fire on iOS Core Location provides no satellite or raw data
        public event Action<List<GnssSatelliteInfo>>? SatelliteStatusChanged;
        public event Action<List<GnssRawMeasurement>>? RawMeasurementsReceived;

        public GnssManager(GnssConfig config)
        {
            _locationManager = new CLLocationManager();
            _config = config;
            _filter = config.Filter;
        }

        public void Start()
        {
            if (IsRunning) return;

            _filter = _config.Filter;
            _filter.Reset();

            _locationListener = new GnssLocationListener();
            _locationListener.LocationReceived += OnLocationReceived;

            _locationManager.Delegate = _locationListener;
            _locationManager.DesiredAccuracy = CLLocation.AccuracyBest;

            // Returns immediately result arrives via the delegate
            _locationManager.RequestWhenInUseAuthorization();
            _locationManager.StartUpdatingLocation();

            IsRunning = true;
        }

        public void Stop()
        {
            if (!IsRunning) return;

            _locationManager.StopUpdatingLocation();

            if (_locationListener != null)
                _locationListener.LocationReceived -= OnLocationReceived;

            _locationManager.Delegate = null;
            _locationListener = null;

            _lastLocation = null;
            IsRunning = false;
            _currentQuality = new GnssQualityStatus();
        }

        private void OnLocationReceived(GnssLocation location)
        {
            _lastLocation = _filter.Process(location);
            LocationReceived?.Invoke(_lastLocation);
            EvaluateQuality();
        }

        // iOS has no satellite data, so Signal and Satellites report Unavailable;
        // only the Accuracy metric is meaningful.
        private void EvaluateQuality()
        {
            _currentQuality = GnssQualityEvaluator.Evaluate(new List<GnssSatelliteInfo>(), _lastLocation, _config);
            QualityChanged?.Invoke(_currentQuality);
        }
    }
}