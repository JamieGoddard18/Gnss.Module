/* FOR TESTING */ 
/* Logic for main GNSS data page. */ 

namespace Ocad.Gnss
{
    public partial class MainPage : ContentPage
    {
        public static MainPage? Current { get; private set; }

        public MainPage()
        {
            InitializeComponent();
            Current = this;
        }

        public void UpdateQuality(GnssQualityStatus quality)
        {
            MainThread.BeginInvokeOnMainThread(() =>
                QualityLabel.Text =
                    $"Signal: {quality.Signal}\n" +
                    $"Satellites: {quality.Satellites}\n" +
                    $"Accuracy: {quality.Accuracy}");
        }

        public void UpdateLocation(GnssLocation loc)
        {
            MainThread.BeginInvokeOnMainThread(() =>
                LocationLabel.Text =
                    $"Lat: {loc.Latitude:F6}\n" +
                    $"Lng: {loc.Longitude:F6}\n" +
                    $"Altitude: {loc.Altitude?.ToString("F1") ?? "—"} m\n" +
                    $"H. Accuracy: {loc.HorizontalAccuracy:F1} m\n" +
                    $"V. Accuracy: {loc.VerticalAccuracy?.ToString("F1") ?? "—"} m\n" +
                    $"Speed: {loc.Speed?.ToString("F1") ?? "—"} m/s\n" +
                    $"Speed Accuracy: {loc.SpeedAccuracy?.ToString("F1") ?? "—"} m/s\n" +
                    $"Bearing: {loc.Bearing?.ToString("F1") ?? "—"}°\n" +
                    $"Bearing Accuracy: {loc.BearingAccuracy?.ToString("F1") ?? "—"}°\n" +
                    $"Provider: {loc.Provider ?? "—"}\n" +
                    $"Time: {loc.Timestamp:HH:mm:ss}");
        }

        public void UpdateSatellites(IReadOnlyList<GnssSatelliteInfo> sats)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var lines = sats.Select(s =>
                    $"SV {s.Svid} ({s.ConstellationType}) " +
                    $"C/N0 {s.Cn0DbHz:F1} " +
                    $"Elev {s.ElevationDegrees:F0}° " +
                    $"Az {s.AzimuthDegrees:F0}° " +
                    $"{(s.UsedInFix ? "✓ used" : "")}");

                SatelliteLabel.Text = $"In view: {sats.Count}\n" + string.Join("\n", lines);
            });
        }

        public void UpdateRaw(IReadOnlyList<GnssRawMeasurement> raw)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var lines = raw.Select(m =>
                    $"SV {m.Svid} ({m.ConstellationType}) " +
                    $"C/N0 {m.Cn0DbHz:F1} " +
                    $"AGC {m.AutomaticGainControlLevelDb:F1} " +
                    $"Doppler {m.PseudorangeRateMetersPerSecond:F1} m/s " +
                    $"ADR {m.AccumulatedDeltaRangeMeters?.ToString("F1") ?? "—"} " +
                    $"({(m.AccumulatedDeltaRangeValid ? "valid" : "invalid")})");

                RawLabel.Text = $"Count: {raw.Count}\n" + string.Join("\n", lines);
            });
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new SettingsPage());
        }
    }
}