using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

using Optuna.Sampler.OptunaHub;

using Tunny.Core.Input;
using Tunny.Core.Settings;
using Tunny.WPF.Common;

namespace Tunny.WPF.Views.Pages.Settings.Sampler
{
    public partial class MOEADSettingsPage : Page, ITrialNumberParam
    {
        public string Param1Label { get; set; } = "Number of Generation";
        public string Param2Label { get; set; } = "Population Size";
        public Visibility Param2Visibility { get; set; } = Visibility.Visible;

        public MOEADSettingsPage()
        {
            InitializeComponent();
            MoeadCrossoverComboBox.ItemsSource = Enum.GetNames(typeof(NsgaCrossoverType));
            MoeadCrossoverComboBox.SelectedIndex = 0;
            MoeadScalarAggregationComboBox.ItemsSource = Enum.GetNames(typeof(ScalarAggregationType));
            MoeadScalarAggregationComboBox.SelectedIndex = 0;
        }

        internal MOEADSampler ToSettings()
        {
            return new MOEADSampler
            {
                Seed = MoeadSeedTextBox.Text == "AUTO"
                    ? null
                    : (int?)int.Parse(MoeadSeedTextBox.Text, CultureInfo.InvariantCulture),
                MutationProb = MoeadMutationProbabilityTextBox.Text == "AUTO"
                    ? null
                    : (double?)double.Parse(MoeadMutationProbabilityTextBox.Text, CultureInfo.InvariantCulture),
                CrossoverProb = double.Parse(MoeadCrossoverProbabilityTextBox.Text, CultureInfo.InvariantCulture),
                SwappingProb = double.Parse(MoeadSwappingProbabilityTextBox.Text, CultureInfo.InvariantCulture),
                Crossover = ((NsgaCrossoverType)MoeadCrossoverComboBox.SelectedIndex).ToString(),
                NumNeighbors = MoeadNeighborsTextBox.Text == "AUTO"
                    ? -1
                    : int.Parse(MoeadNeighborsTextBox.Text, CultureInfo.InvariantCulture),
                ScalarAggregation = (ScalarAggregationType)MoeadScalarAggregationComboBox.SelectedIndex,
                ForceReload = MoeadForceReloadCheckBox.IsChecked == true
            };
        }

        internal static MOEADSettingsPage FromSettings(TSettings settings)
        {
            MOEADSampler moead = settings.Optimize.Sampler.MOEAD;
            var page = new MOEADSettingsPage();
            page.MoeadSeedTextBox.Text = moead.Seed == null
                ? "AUTO"
                : moead.Seed.Value.ToString(CultureInfo.InvariantCulture);
            page.MoeadMutationProbabilityTextBox.Text = moead.MutationProb == null
                ? "AUTO"
                : moead.MutationProb.Value.ToString(CultureInfo.InvariantCulture);
            page.MoeadCrossoverProbabilityTextBox.Text = moead.CrossoverProb.ToString(CultureInfo.InvariantCulture);
            page.MoeadSwappingProbabilityTextBox.Text = moead.SwappingProb.ToString(CultureInfo.InvariantCulture);
            page.MoeadCrossoverComboBox.SelectedIndex = string.IsNullOrEmpty(moead.Crossover)
                ? 0 : (int)Enum.Parse(typeof(NsgaCrossoverType), moead.Crossover);
            page.MoeadScalarAggregationComboBox.SelectedIndex = (int)moead.ScalarAggregation;
            page.MoeadNeighborsTextBox.Text = moead.NumNeighbors == -1
                ? "AUTO"
                : moead.NumNeighbors.ToString(CultureInfo.InvariantCulture);
            page.MoeadForceReloadCheckBox.IsChecked = false;
            return page;
        }

        private void MoeadSeedTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string value = textBox.Text;
            textBox.Text = InputValidator.IsAutoOrInt(value) ? value : "AUTO";
        }

        private void MoeadMutationProbabilityTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string value = textBox.Text;
            textBox.Text = InputValidator.IsAutoOr0to1(value) ? value : "AUTO";
        }

        private void MoeadCrossoverProbabilityTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string value = textBox.Text;
            textBox.Text = InputValidator.Is0to1(value) ? value : "0.9";
        }

        private void MoeadSwappingProbabilityTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string value = textBox.Text;
            textBox.Text = InputValidator.Is0to1(value) ? value : "0.5";
        }

        private void MoeadNeighborsTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string value = textBox.Text;
            textBox.Text = InputValidator.IsAutoOrPositiveInt(value, false) ? value : "AUTO";
        }

        private void DefaultButton_Click(object sender, RoutedEventArgs e)
        {
            MoeadSeedTextBox.Text = "AUTO";
            MoeadMutationProbabilityTextBox.Text = "AUTO";
            MoeadCrossoverProbabilityTextBox.Text = "0.9";
            MoeadSwappingProbabilityTextBox.Text = "0.5";
            MoeadCrossoverComboBox.SelectedIndex = 1;
            MoeadNeighborsTextBox.Text = "AUTO";
            MoeadScalarAggregationComboBox.SelectedIndex = 1;
            MoeadForceReloadCheckBox.IsChecked = false;
        }
    }
}
