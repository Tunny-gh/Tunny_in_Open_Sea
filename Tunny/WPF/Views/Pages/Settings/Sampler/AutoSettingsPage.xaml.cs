using System.Globalization;
using System.Windows;
using System.Windows.Controls;

using Optuna.Sampler.OptunaHub;

using Tunny.Core.Input;
using Tunny.Core.Settings;
using Tunny.WPF.Common;

namespace Tunny.WPF.Views.Pages.Settings.Sampler
{
    public partial class AutoSettingsPage : Page, ITrialNumberParam
    {
        public string Param1Label { get; set; } = "Number of Trials";
        public string Param2Label { get; set; } = "";
        public Visibility Param2Visibility { get; set; } = Visibility.Hidden;

        public AutoSettingsPage()
        {
            InitializeComponent();
        }

        internal AutoSampler ToSettings()
        {
            return new AutoSampler
            {
                Seed = AutoSeedTextBox.Text == "AUTO"
                    ? null
                    : (int?)int.Parse(AutoSeedTextBox.Text, CultureInfo.InvariantCulture),
                ForceReload = AutoForceReloadCheckBox.IsChecked == true
            };
        }

        internal static AutoSettingsPage FromSettings(TSettings settings)
        {
            AutoSampler auto = settings.Optimize.Sampler.Auto;
            var page = new AutoSettingsPage();
            page.AutoSeedTextBox.Text = auto.Seed == null
                ? "AUTO"
                : auto.Seed.Value.ToString(CultureInfo.InvariantCulture);
            return page;
        }

        private void AutoSeedTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string value = textBox.Text;
            textBox.Text = InputValidator.IsAutoOrInt(value) ? value : "AUTO";
        }

        private void DefaultButton_Click(object sender, RoutedEventArgs e)
        {
            AutoSeedTextBox.Text = "AUTO";
            AutoForceReloadCheckBox.IsChecked = false;
        }
    }
}
