using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

using Optuna.Sampler;
using Optuna.Sampler.OptunaHub;

using Tunny.Core.Input;
using Tunny.Core.Settings;
using Tunny.WPF.Common;

namespace Tunny.WPF.Views.Pages.Settings.Sampler
{
    public partial class cTPESettingsPage : Page, ITrialNumberParam
    {
        public string Param1Label { get; set; } = "Number of Trials";
        public string Param2Label { get; set; } = "";
        public Visibility Param2Visibility { get; set; } = Visibility.Hidden;

        public cTPESettingsPage()
        {
            InitializeComponent();
            CTpeBandWidthStrategyComboBox.ItemsSource = Enum.GetValues(typeof(BandwidthStrategy));
            CTpeWeightStrategyComboBox.ItemsSource = Enum.GetValues(typeof(WeightStrategy));
            CTpeGammaStrategyComboBox.ItemsSource = Enum.GetValues(typeof(GammaStrategy));
            CTpeBandWidthStrategyComboBox.SelectedIndex = 0;
            CTpeWeightStrategyComboBox.SelectedIndex = 0;
            CTpeGammaStrategyComboBox.SelectedIndex = 0;
        }

        internal cTPESampler ToSettings()
        {
            return new cTPESampler
            {
                Seed = CTpeSeedTextBox.Text == "AUTO"
                    ? null
                    : (int?)int.Parse(CTpeSeedTextBox.Text, CultureInfo.InvariantCulture),
                NStartupTrials = CTpeStartupTrialsTextBox.Text == "AUTO"
                    ? -1
                    : int.Parse(CTpeStartupTrialsTextBox.Text, CultureInfo.InvariantCulture),
                NEICandidates = int.Parse(CTpeEICandidateTextBox.Text, CultureInfo.InvariantCulture),
                PriorWeight = double.Parse(CTpePriorWeightTextBox.Text, CultureInfo.InvariantCulture),
                ConsiderPrior = CTpeConsiderPriorCheckBox.IsChecked ?? false,
                ConsiderMagicClip = CTpeConsiderMagicClipCheckBox.IsChecked ?? false,
                Multivariate = CTpeMultivariateCheckBox.IsChecked ?? false,
                UseMinBandWidthDiscrete = CTpeUseMinBandCheckBox.IsChecked ?? false,
                BandwidthStrategy = CTpeBandWidthStrategyComboBox.SelectedItem.ToString(),
                WeightStrategy = CTpeWeightStrategyComboBox.SelectedItem.ToString(),
                GammaStrategy = CTpeGammaStrategyComboBox.SelectedItem.ToString(),
                GammaBeta = double.Parse(CTpeGammaBetaTextBox.Text, CultureInfo.InvariantCulture)
            };
        }

        internal static cTPESettingsPage FromSettings(TSettings settings)
        {
            cTPESampler ctpe = settings.Optimize.Sampler.cTPE;
            var page = new cTPESettingsPage();
            page.CTpeSeedTextBox.Text = ctpe.Seed == null
                ? "AUTO"
                : ctpe.Seed.Value.ToString(CultureInfo.InvariantCulture);
            page.CTpeStartupTrialsTextBox.Text = ctpe.NStartupTrials == -1
                ? "AUTO"
                : ctpe.NStartupTrials.ToString(CultureInfo.InvariantCulture);
            page.CTpeEICandidateTextBox.Text = ctpe.NEICandidates.ToString(CultureInfo.InvariantCulture);
            page.CTpeGammaBetaTextBox.Text = ctpe.GammaBeta.ToString(CultureInfo.InvariantCulture);
            page.CTpePriorWeightTextBox.Text = ctpe.PriorWeight.ToString(CultureInfo.InvariantCulture);
            page.CTpeConsiderPriorCheckBox.IsChecked = ctpe.ConsiderPrior;
            page.CTpeConsiderMagicClipCheckBox.IsChecked = ctpe.ConsiderMagicClip;
            page.CTpeMultivariateCheckBox.IsChecked = ctpe.Multivariate;
            page.CTpeUseMinBandCheckBox.IsChecked = ctpe.UseMinBandWidthDiscrete;
            page.CTpeGammaStrategyComboBox.SelectedIndex = string.IsNullOrEmpty(ctpe.GammaStrategy)
                ? 0 : (int)Enum.Parse(typeof(GammaStrategy), ctpe.GammaStrategy);
            page.CTpeBandWidthStrategyComboBox.SelectedIndex = string.IsNullOrEmpty(ctpe.BandwidthStrategy)
                ? 0 : (int)Enum.Parse(typeof(BandwidthStrategy), ctpe.BandwidthStrategy);
            page.CTpeWeightStrategyComboBox.SelectedIndex = string.IsNullOrEmpty(ctpe.WeightStrategy)
                ? 0 : (int)Enum.Parse(typeof(WeightStrategy), ctpe.WeightStrategy.Replace("-", "_"));
            return page;
        }

        private void CTpeSeedTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string value = textBox.Text;
            textBox.Text = InputValidator.IsAutoOrInt(value) ? value : "AUTO";
        }

        private void CTpeStartupTrialsTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string value = textBox.Text;
            textBox.Text = InputValidator.IsAutoOrPositiveInt(value, false) ? value : "AUTO";
        }

        private void CTpeEICandidateTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string value = textBox.Text;
            textBox.Text = InputValidator.IsPositiveInt(value, false) ? value : "24";
        }

        private void CTpeGammaTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string value = textBox.Text;
            textBox.Text = InputValidator.IsPositiveInt(value, false) ? value : "0.25";
        }

        private void CTpePriorWeightTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string value = textBox.Text;
            textBox.Text = InputValidator.IsPositiveDouble(value, false) ? value : "1.0";
        }

        private void DefaultButton_Click(object sender, RoutedEventArgs e)
        {
            CTpeSeedTextBox.Text = "AUTO";
            CTpeStartupTrialsTextBox.Text = "AUTO";
            CTpeEICandidateTextBox.Text = "24";
            CTpeGammaBetaTextBox.Text = "0.25";
            CTpePriorWeightTextBox.Text = "1.0";
            CTpeConsiderPriorCheckBox.IsChecked = true;
            CTpeConsiderMagicClipCheckBox.IsChecked = true;
            CTpeMultivariateCheckBox.IsChecked = true;
            CTpeUseMinBandCheckBox.IsChecked = true;
            CTpeBandWidthStrategyComboBox.SelectedIndex = 0;
            CTpeWeightStrategyComboBox.SelectedIndex = 0;
            CTpeGammaStrategyComboBox.SelectedIndex = 0;
        }
    }
}
