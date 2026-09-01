using Android.App;
using Android.Content.PM;
using Android.Locations;
using Android.OS;

namespace Ocad.Gnss
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true,
              ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation |
              ConfigChanges.UiMode | ConfigChanges.ScreenLayout |
              ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]

    public class MainActivity : MauiAppCompatActivity
    {
        private IGnssManager? _gnssManager;

        // Called by android 
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestPermissions([Android.Manifest.Permission.AccessFineLocation], 0);
        }

        // Called after the user responds to the permission dialog
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                StartGnss();
        }

        // Create manager, subscribe to its events, and start it
        private void StartGnss()
        {
            var locationManager = (LocationManager?)GetSystemService(LocationService);

            if (locationManager == null) 
                return;
            
            _gnssManager = new Platforms.Android.GnssManager(locationManager, GnssApp.Config);
            GnssApp.Manager = _gnssManager;

            _gnssManager.LocationReceived += OnLocationReceived;
            _gnssManager.QualityChanged += OnQualityChanged;
            _gnssManager.SatelliteStatusChanged += OnSatelliteStatusChanged;
            _gnssManager.RawMeasurementsReceived += OnRawMeasurementsReceived;

            _gnssManager.Start();
        }

        // Called when the manager fires a new location
        private void OnLocationReceived(GnssLocation location)
        {
            MainPage.Current?.UpdateLocation(location);
        }

        // Called when the manager re-evaluates quality
        private void OnQualityChanged(GnssQualityStatus quality)
        {
            MainPage.Current?.UpdateQuality(quality);
        }

        // Called when the manager fires new satellite data
        private void OnSatelliteStatusChanged(List<GnssSatelliteInfo> satellites)
        {
            MainPage.Current?.UpdateSatellites(satellites);
        }

        private void OnRawMeasurementsReceived(List<GnssRawMeasurement> measurements)
        {
            MainPage.Current?.UpdateRaw(measurements);
        }

        // Clean up when the app closes
        protected override void OnDestroy()
        {
            base.OnDestroy();
            _gnssManager?.Stop();
        }
    }
}