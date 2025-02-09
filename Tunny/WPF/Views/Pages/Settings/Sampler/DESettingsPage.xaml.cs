using System.Globalization;
using System.Windows;
using System.Windows.Controls;

using Optuna.Sampler.OptunaHub;

using Tunny.Core.Input;
using Tunny.Core.Settings;
using Tunny.WPF.Common;

namespace Tunny.WPF.Views.Pages.Settings.Sampler
{
    public partial class DESettingsPage : Page, ITrialNumberParam
    {
        public string Param1Label { get; set; } = "Number of Trials";
        public string Param2Label { get; set; } = "";
        public Visibility Param2Visibility { get; set; } = Visibility.Hidden;

        public DESettingsPage()
        {
            InitializeComponent();
        }

        internal DESampler ToSettings()
        {
            return new DESampler
            {
                Seed = DESeedTextBox.Text == "AUTO"
                    ? null
                    : (int?)int.Parse(DESeedTextBox.Text, CultureInfo.InvariantCulture),
                ForceReload = DEForceReloadCheckBox.IsChecked == true,
                DEFactor = double.Parse(DEFactorTextBox.Text, CultureInfo.InvariantCulture),
                CrossOverRate = double.Parse(DECrossoverRatioTextBox.Text, CultureInfo.InvariantCulture),
                PopulationSize = null
            };
        }

        internal static DESettingsPage FromSettings(TSettings settings)
        {
            DESampler de = settings.Optimize.Sampler.DE;
            var page = new DESettingsPage();
            page.DESeedTextBox.Text = de.Seed == null
                ? "AUTO"
                : de.Seed.Value.ToString(CultureInfo.InvariantCulture);
            page.DEFactorTextBox.Text = de.DEFactor.ToString(CultureInfo.InvariantCulture);
            page.DECrossoverRatioTextBox.Text = de.CrossOverRate.ToString(CultureInfo.InvariantCulture);
            page.DEForceReloadCheckBox.IsChecked = false;
            return page;
        }

        private void DESeedTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string value = textBox.Text;
            textBox.Text = InputValidator.IsAutoOrInt(value) ? value : "AUTO";
        }

        private void DefaultButton_Click(object sender, RoutedEventArgs e)
        {
            DESeedTextBox.Text = "AUTO";
            DEForceReloadCheckBox.IsChecked = false;
        }
    }
}
