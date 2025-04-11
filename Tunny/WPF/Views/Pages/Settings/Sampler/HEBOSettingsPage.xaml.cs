using System.Globalization;
using System.Windows;
using System.Windows.Controls;

using Optuna.Sampler.OptunaHub;

using Tunny.Core.Input;
using Tunny.Core.Settings;
using Tunny.WPF.Common;

namespace Tunny.WPF.Views.Pages.Settings.Sampler
{
    public partial class HEBOSettingsPage : Page, ITrialNumberParam
    {
        public string Param1Label { get; set; } = "Number of Trials";
        public string Param2Label { get; set; } = "";
        public Visibility Param2Visibility { get; set; } = Visibility.Hidden;

        public HEBOSettingsPage()
        {
            InitializeComponent();
        }

        internal AutoSampler ToSettings()
        {
            return new AutoSampler
            {
                Seed = HEBOSeedTextBox.Text == "AUTO"
                    ? null
                    : (int?)int.Parse(HEBOSeedTextBox.Text, CultureInfo.InvariantCulture),
                ForceReload = HEBOForceReloadCheckBox.IsChecked == true
            };
        }

        internal static HEBOSettingsPage FromSettings(TSettings settings)
        {
            HEBOSampler auto = settings.Optimize.Sampler.HEBO;
            var page = new HEBOSettingsPage();
            page.HEBOSeedTextBox.Text = auto.Seed == null
                ? "AUTO"
                : auto.Seed.Value.ToString(CultureInfo.InvariantCulture);
            return page;
        }

        private void HEBOSeedTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string value = textBox.Text;
            textBox.Text = InputValidator.IsAutoOrInt(value) ? value : "AUTO";
        }

        private void DefaultButton_Click(object sender, RoutedEventArgs e)
        {
            HEBOSeedTextBox.Text = "AUTO";
            HEBOForceReloadCheckBox.IsChecked = false;
        }
    }
}
