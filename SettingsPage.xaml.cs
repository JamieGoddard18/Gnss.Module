/* FOR TESTING */  
/* Settings page logic for configurable GNSS values */

namespace Ocad.Gnss
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();
            LoadCurrentConfig();
        }

        // Populate fields from the current shared config
        private void LoadCurrentConfig()
        {
            var config = GnssApp.Config;
            UpdateIntervalEntry.Text = config.UpdateIntervalMs.ToString();
            SatGoodEntry.Text = config.SatelliteCountGood.ToString();
            SatExcellentEntry.Text = config.SatelliteCountExcellent.ToString();
            Cn0WeakEntry.Text = config.Cn0Weak.ToString();
            Cn0ModerateEntry.Text = config.Cn0Moderate.ToString();
            Cn0StrongEntry.Text = config.Cn0Strong.ToString();
            AccuracyExcellentEntry.Text = config.AccuracyExcellent.ToString();
            AccuracyGoodEntry.Text = config.AccuracyGood.ToString();
            AccuracyEntry.Text = config.AccuracyLow.ToString();
            FilterPicker.SelectedIndex = config.Filter is SmoothingFilter ? 1 : 0;
            AlphaEntry.Text = config.SmoothingAlpha.ToString();
        }

        // Save fields into the config and restart the manager to apply
        private async void OnApplyClicked(object sender, EventArgs e)
        {
            var config = GnssApp.Config;

            if (int.TryParse(UpdateIntervalEntry.Text, out var interval)) config.UpdateIntervalMs = interval;
            if (double.TryParse(AlphaEntry.Text, out var alpha)) config.SmoothingAlpha = alpha;
            config.Filter = FilterPicker.SelectedIndex == 1 ? new SmoothingFilter(config.SmoothingAlpha) : new NoFilter();
            if (int.TryParse(SatGoodEntry.Text, out var satGood)) config.SatelliteCountGood = satGood;
            if (int.TryParse(SatExcellentEntry.Text, out var satExc)) config.SatelliteCountExcellent = satExc;
            if (float.TryParse(Cn0WeakEntry.Text, out var weak)) config.Cn0Weak = weak;
            if (float.TryParse(Cn0ModerateEntry.Text, out var mod)) config.Cn0Moderate = mod;
            if (float.TryParse(Cn0StrongEntry.Text, out var strong)) config.Cn0Strong = strong;
            if (float.TryParse(AccuracyExcellentEntry.Text, out var accEx)) config.AccuracyExcellent = accEx;
            if (float.TryParse(AccuracyGoodEntry.Text, out var accGd)) config.AccuracyGood = accGd;
            if (float.TryParse(AccuracyEntry.Text, out var acc)) config.AccuracyLow = acc;

            GnssApp.Manager?.Stop();
            GnssApp.Manager?.Start();

            await DisplayAlert("Settings", "Configuration applied", "OK");
        }
    }
}