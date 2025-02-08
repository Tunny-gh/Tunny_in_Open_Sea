using System;
using System.Windows;
using System.Windows.Controls;

using Optuna.Sampler.OptunaHub;

using Tunny.Core.Input;
using Tunny.Core.Settings;
using Tunny.WPF.Common;

namespace Tunny.WPF.Views.Pages.Settings.Sampler
{
    public partial class NSGAIISettingsPage : Page, ITrialNumberParam
    {
        public string Param1Label { get; set; } = "Number of Generation";
        public string Param2Label { get; set; } = "Population Size";
        public Visibility Param2Visibility { get; set; } = Visibility.Visible;

        public NSGAIISettingsPage()
        {
            InitializeComponent();
            NsgaiiCrossoverComboBox.ItemsSource = Enum.GetNames(typeof(NsgaCrossoverType));
            NsgaiiCrossoverComboBox.SelectedIndex = 0;
        }

        internal NSGAIISampler ToSettings()
        {
            return new NSGAIISampler
            {
                Seed = NsgaiiSeedTextBox.Text == "AUTO"
                    ? null
                    : (int?)int.Parse(NsgaiiSeedTextBox.Text, System.Globalization.CultureInfo.InvariantCulture),
                MutationProb = NsgaiiMutationProbabilityTextBox.Text == "AUTO"
                    ? null
                    : (double?)double.Parse(NsgaiiMutationProbabilityTextBox.Text, System.Globalization.CultureInfo.InvariantCulture),
                CrossoverProb = double.Parse(NsgaiiCrossoverProbabilityTextBox.Text, System.Globalization.CultureInfo.InvariantCulture),
                SwappingProb = double.Parse(NsgaiiSwappingProbabilityTextBox.Text, System.Globalization.CultureInfo.InvariantCulture),
                Crossover = ((NsgaCrossoverType)NsgaiiCrossoverComboBox.SelectedIndex).ToString(),
                ForceReload = NsgaiiForceReloadCheckBox.IsChecked == true
            };
        }

        internal static NSGAIISettingsPage FromSettings(TSettings settings)
        {
            NSGAIISampler nsgaii = settings.Optimize.Sampler.NsgaII;
            var page = new NSGAIISettingsPage();
            page.NsgaiiSeedTextBox.Text = nsgaii.Seed == null
                ? "AUTO"
                : nsgaii.Seed.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            page.NsgaiiMutationProbabilityTextBox.Text = nsgaii.MutationProb == null
                ? "AUTO"
                : nsgaii.MutationProb.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            page.NsgaiiCrossoverProbabilityTextBox.Text = nsgaii.CrossoverProb.ToString(System.Globalization.CultureInfo.InvariantCulture);
            page.NsgaiiSwappingProbabilityTextBox.Text = nsgaii.SwappingProb.ToString(System.Globalization.CultureInfo.InvariantCulture);
            page.NsgaiiCrossoverComboBox.SelectedIndex = string.IsNullOrEmpty(nsgaii.Crossover)
                ? 0 : (int)Enum.Parse(typeof(NsgaCrossoverType), nsgaii.Crossover);
            page.NsgaiiForceReloadCheckBox.IsChecked = false;
            return page;
        }

        private void NsgaiiSeedTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string value = textBox.Text;
            textBox.Text = InputValidator.IsAutoOrInt(value) ? value : "AUTO";
        }
        private void NsgaiiMutationProbabilityTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string value = textBox.Text;
            textBox.Text = InputValidator.IsAutoOr0to1(value) ? value : "AUTO";
        }

        private void NsgaiiCrossoverProbabilityTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string value = textBox.Text;
            textBox.Text = InputValidator.Is0to1(value) ? value : "0.9";
        }

        private void NsgaiiSwappingProbabilityTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string value = textBox.Text;
            textBox.Text = InputValidator.Is0to1(value) ? value : "0.5";
        }

        private void DefaultButton_Click(object sender, RoutedEventArgs e)
        {
            NsgaiiSeedTextBox.Text = "AUTO";
            NsgaiiMutationProbabilityTextBox.Text = "AUTO";
            NsgaiiCrossoverProbabilityTextBox.Text = "0.9";
            NsgaiiSwappingProbabilityTextBox.Text = "0.5";
            NsgaiiCrossoverComboBox.SelectedIndex = 1;
            NsgaiiForceReloadCheckBox.IsChecked = false;
        }
    }
}
