using System;
using System.Windows;
using System.Windows.Controls;

using Optuna.Sampler.OptunaHub;

using Tunny.Core.Input;
using Tunny.Core.Settings;
using Tunny.WPF.Common;
using Tunny.WPF.Views.Pages.Settings.Crossover;
using Tunny.WPF.Views.Pages.Settings.Mutation;

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
            NsgaiiMutationComboBox.ItemsSource = Enum.GetNames(typeof(NsgaMutationType));
            NsgaiiMutationComboBox.SelectedIndex = 0;
            CrossoverSettings.Content = new SBX();
            MutationSettings.Content = new Polynomial();
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
                Crossover = ((NsgaCrossoverType)NsgaiiCrossoverComboBox.SelectedIndex).ToString(),
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
            page.NsgaiiCrossoverComboBox.SelectedIndex = string.IsNullOrEmpty(nsgaii.Crossover)
                ? 0 : (int)Enum.Parse(typeof(NsgaCrossoverType), nsgaii.Crossover);
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

        private void WallaceiDefaultButton_Click(object sender, RoutedEventArgs e)
        {
            NsgaiiSeedTextBox.Text = "AUTO";
            NsgaiiMutationProbabilityTextBox.Text = "AUTO";
            NsgaiiCrossoverProbabilityTextBox.Text = "0.9";
            NsgaiiMutationComboBox.SelectedIndex = 1;
            NsgaiiCrossoverComboBox.SelectedIndex = 3;
        }

        private void DefaultButton_Click(object sender, RoutedEventArgs e)
        {
            NsgaiiSeedTextBox.Text = "AUTO";
            NsgaiiMutationProbabilityTextBox.Text = "AUTO";
            NsgaiiCrossoverProbabilityTextBox.Text = "0.9";
            NsgaiiCrossoverComboBox.SelectedIndex = 1;
        }
    }
}
