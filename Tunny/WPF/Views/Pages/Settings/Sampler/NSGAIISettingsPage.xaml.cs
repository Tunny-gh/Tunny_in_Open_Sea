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
        }

        internal NSGAIISampler ToSettings()
        {
            double?[] crossoverParam = CrossoverSettings.Content == null
                ? null
                : ((ICrossoverParam)CrossoverSettings.Content).ToParameters();
            double mutationParam = MutationSettings.Content == null
                ? 0
                : ((IMutationParam)MutationSettings.Content).ToParameter();
            return new NSGAIISampler
            {
                Seed = NsgaiiSeedTextBox.Text == "AUTO"
                    ? null
                    : (int?)int.Parse(NsgaiiSeedTextBox.Text, CultureInfo.InvariantCulture),
                MutationProb = NsgaiiMutationProbabilityTextBox.Text == "AUTO"
                    ? null
                    : (double?)double.Parse(NsgaiiMutationProbabilityTextBox.Text, CultureInfo.InvariantCulture),
                CrossoverProb = double.Parse(NsgaiiCrossoverProbabilityTextBox.Text, CultureInfo.InvariantCulture),
                Crossover = ((NsgaCrossoverType)NsgaiiCrossoverComboBox.SelectedIndex).ToString(),
                CrossoverParam = crossoverParam,
                Mutation = ((NsgaMutationType)NsgaiiMutationComboBox.SelectedIndex).ToString(),
                MutationParam = mutationParam,
            };
        }

        internal static NSGAIISettingsPage FromSettings(TSettings settings)
        {
            NSGAIISampler nsgaii = settings.Optimize.Sampler.NsgaII;
            var page = new NSGAIISettingsPage();
            page.NsgaiiSeedTextBox.Text = nsgaii.Seed == null
                ? "AUTO"
                : nsgaii.Seed.Value.ToString(CultureInfo.InvariantCulture);
            page.NsgaiiMutationProbabilityTextBox.Text = nsgaii.MutationProb == null
                ? "AUTO"
                : nsgaii.MutationProb.Value.ToString(CultureInfo.InvariantCulture);
            page.NsgaiiMutationComboBox.SelectedIndex = string.IsNullOrEmpty(nsgaii.Mutation)
                ? 0 : (int)Enum.Parse(typeof(NsgaMutationType), nsgaii.Mutation);
            page.NsgaiiCrossoverProbabilityTextBox.Text = nsgaii.CrossoverProb.ToString(CultureInfo.InvariantCulture);
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
            NsgaiiMutationComboBox.SelectedIndex = 1;
            NsgaiiCrossoverComboBox.SelectedIndex = 1;
        }

        private void CrossoverChanged(object sender, SelectionChangedEventArgs e)
        {
            var crossoverType = (NsgaCrossoverType)Enum.Parse(typeof(NsgaCrossoverType), NsgaiiCrossoverComboBox.SelectedItem.ToString());
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
            var mutationType = (NsgaMutationType)Enum.Parse(typeof(NsgaMutationType), NsgaiiMutationComboBox.SelectedItem.ToString());
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
