using System;
using System.Globalization;
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
    public partial class MOEADSettingsPage : Page, ITrialNumberParam
    {
        public string Param1Label { get; set; } = "Number of Generation";
        public string Param2Label { get; set; } = "Population Size";
        public Visibility Param2Visibility { get; set; } = Visibility.Visible;

        public MOEADSettingsPage()
        {
            InitializeComponent();
            CrossoverComboBox.ItemsSource = Enum.GetNames(typeof(NsgaCrossoverType));
            CrossoverComboBox.SelectedIndex = 0;
            MutationComboBox.ItemsSource = new string[]
            {
                NsgaMutationType.Uniform.ToString(),
            };
            MutationComboBox.SelectedIndex = 0;
            MoeadScalarAggregationComboBox.ItemsSource = Enum.GetNames(typeof(ScalarAggregationType));
            MoeadScalarAggregationComboBox.SelectedIndex = 0;
        }

        internal MOEADSampler ToSettings()
        {
            double?[] crossoverParam = CrossoverSettings.Content == null
                ? null
                : ((ICrossoverParam)CrossoverSettings.Content).ToParameters();
            double mutationParam = MutationSettings.Content == null
                ? 0
                : ((IMutationParam)MutationSettings.Content).ToParameter();
            return new MOEADSampler
            {
                Seed = MoeadSeedTextBox.Text == "AUTO"
                    ? null
                    : (int?)int.Parse(MoeadSeedTextBox.Text, CultureInfo.InvariantCulture),
                MutationProb = MoeadMutationProbabilityTextBox.Text == "AUTO"
                    ? null
                    : (double?)double.Parse(MoeadMutationProbabilityTextBox.Text, CultureInfo.InvariantCulture),
                CrossoverProb = double.Parse(MoeadCrossoverProbabilityTextBox.Text, CultureInfo.InvariantCulture),
                Crossover = ((NsgaCrossoverType)CrossoverComboBox.SelectedIndex).ToString(),
                NumNeighbors = MoeadNeighborsTextBox.Text == "AUTO"
                    ? -1
                    : int.Parse(MoeadNeighborsTextBox.Text, CultureInfo.InvariantCulture),
                ScalarAggregation = (ScalarAggregationType)MoeadScalarAggregationComboBox.SelectedIndex,
                ForceReload = MoeadForceReloadCheckBox.IsChecked == true,
                CrossoverParam = crossoverParam,
                Mutation = ((NsgaMutationType)MutationComboBox.SelectedIndex).ToString(),
                MutationParam = mutationParam,
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
            page.CrossoverComboBox.SelectedIndex = string.IsNullOrEmpty(moead.Crossover)
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
            CrossoverComboBox.SelectedIndex = 1;
            MoeadNeighborsTextBox.Text = "AUTO";
            MoeadScalarAggregationComboBox.SelectedIndex = 1;
            MoeadForceReloadCheckBox.IsChecked = false;
        }

        private void CrossoverChanged(object sender, SelectionChangedEventArgs e)
        {
            var crossoverType = (NsgaCrossoverType)Enum.Parse(typeof(NsgaCrossoverType), CrossoverComboBox.SelectedItem.ToString());
            switch (crossoverType)
            {
                case NsgaCrossoverType.Uniform:
                    CrossoverSettings.Content = new Crossover.Uniform();
                    break;
                case NsgaCrossoverType.BLXAlpha:
                    CrossoverSettings.Content = new BLXAlpha();
                    break;
                case NsgaCrossoverType.SPX:
                    CrossoverSettings.Content = new SPX();
                    break;
                case NsgaCrossoverType.SBX:
                    CrossoverSettings.Content = new SBX();
                    break;
                case NsgaCrossoverType.VSBX:
                    CrossoverSettings.Content = new VSBX();
                    break;
                case NsgaCrossoverType.UNDX:
                    CrossoverSettings.Content = new UNDX();
                    break;
                default:
                    CrossoverSettings.Content = new SBX();
                    break;
            }
        }

        private void MutationChanged(object sender, SelectionChangedEventArgs e)
        {
            var mutationType = (NsgaMutationType)Enum.Parse(typeof(NsgaMutationType), MutationComboBox.SelectedItem.ToString());
            switch (mutationType)
            {
                case NsgaMutationType.Uniform:
                    MutationSettings.Content = new Mutation.Uniform();
                    break;
                case NsgaMutationType.Polynomial:
                    MutationSettings.Content = new Polynomial();
                    break;
                case NsgaMutationType.Gaussian:
                    MutationSettings.Content = new Gaussian();
                    break;
                default:
                    MutationSettings.Content = new Mutation.Uniform();
                    break;
            }
        }
    }
}
