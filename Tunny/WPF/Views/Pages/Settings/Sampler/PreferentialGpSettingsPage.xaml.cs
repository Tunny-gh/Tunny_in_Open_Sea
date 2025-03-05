using System.Globalization;
using System.Windows;
using System.Windows.Controls;

using Optuna.Sampler.Dashboard;

using Tunny.Core.Input;
using Tunny.Core.Settings;
using Tunny.WPF.Common;

namespace Tunny.WPF.Views.Pages.Settings.Sampler
{
    public partial class PreferentialGpSettingsPage : Page, ITrialNumberParam
    {
        public string Param1Label { get; set; } = "Number of Generate";
        public string Param2Label { get; set; } = "";
        public Visibility Param2Visibility { get; set; } = Visibility.Hidden;

        public PreferentialGpSettingsPage()
        {
            InitializeComponent();
        }

        internal PreferentialGpSampler ToSettings()
        {
            return new PreferentialGpSampler
            {
                Seed = PreferentialGpSeedTextBox.Text == "AUTO"
                    ? null
                    : (int?)int.Parse(PreferentialGpSeedTextBox.Text, CultureInfo.InvariantCulture),
            };
        }

        internal static PreferentialGpSettingsPage FromSettings(TSettings settings)
        {
            PreferentialGpSampler random = settings.Optimize.Sampler.PreferentialGp;
            var page = new PreferentialGpSettingsPage();
            page.PreferentialGpSeedTextBox.Text = random.Seed == null
                ? "AUTO"
                : random.Seed.Value.ToString(CultureInfo.InvariantCulture);
            return page;
        }

        private void PreferentialGpSeedTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string value = textBox.Text;
            textBox.Text = InputValidator.IsAutoOrInt(value) ? value : "AUTO";
        }

        private void DefaultButton_Click(object sender, RoutedEventArgs e)
        {
            PreferentialGpSeedTextBox.Text = "AUTO";
        }
    }
}
