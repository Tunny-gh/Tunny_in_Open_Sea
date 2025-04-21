using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

using Optuna.Sampler;

using Tunny.Core.Input;
using Tunny.Core.Settings;
using Tunny.WPF.Common;
using Tunny.WPF.Views.Pages.Settings.Crossover;
using Tunny.WPF.Views.Pages.Settings.Mutation;

namespace Tunny.WPF.Views.Pages.Settings.Sampler
{
    public partial class NSGAIIISettingsPage : Page, ITrialNumberParam
    {
        public string Param1Label { get; set; } = "Number of Generation";
        public string Param2Label { get; set; } = "Population Size";
        public Visibility Param2Visibility { get; set; } = Visibility.Visible;

        public NSGAIIISettingsPage()
        {
            InitializeComponent();
            CrossoverComboBox.ItemsSource = Enum.GetNames(typeof(NsgaCrossoverType));
            CrossoverComboBox.SelectedIndex = 0;
            CrossoverSettings.Content = new Crossover.Uniform();
            MutationComboBox.ItemsSource = new string[]
            {
                NsgaMutationType.Uniform.ToString(),
            };
            MutationComboBox.SelectedIndex = 0;
            MutationSettings.Content = new Mutation.Uniform();
        }

        internal NSGAIIISampler ToSettings()
        {
            double?[] crossoverParam = CrossoverSettings.Content == null
                ? null
                :((ICrossoverParam)CrossoverSettings.Content).ToParameters();
            double mutationParam = MutationSettings.Content == null
                ? 0
                :((IMutationParam)MutationSettings.Content).ToParameter();
            return new NSGAIIISampler
            {
                Seed = NsgaiiiSeedTextBox.Text == "AUTO"
                    ? null
                    : (int?)int.Parse(NsgaiiiSeedTextBox.Text, CultureInfo.InvariantCulture),
                MutationProb = NsgaiiiMutationProbabilityTextBox.Text == "AUTO"
                    ? null
                    : (double?)double.Parse(NsgaiiiMutationProbabilityTextBox.Text, CultureInfo.InvariantCulture),
                CrossoverProb = double.Parse(NsgaiiiCrossoverProbabilityTextBox.Text, CultureInfo.InvariantCulture),
                Crossover = ((NsgaCrossoverType)CrossoverComboBox.SelectedIndex).ToString(),
                CrossoverParam = crossoverParam,
                Mutation = ((NsgaMutationType)MutationComboBox.SelectedIndex).ToString(),
                MutationParam = mutationParam,
            };
        }

        internal static NSGAIIISettingsPage FromSettings(TSettings settings)
        {
            NSGAIIISampler nsgaiii = settings.Optimize.Sampler.NsgaIII;
            var page = new NSGAIIISettingsPage();
            page.NsgaiiiSeedTextBox.Text = nsgaiii.Seed == null
                ? "AUTO"
                : nsgaiii.Seed.Value.ToString(CultureInfo.InvariantCulture);
            page.NsgaiiiMutationProbabilityTextBox.Text = nsgaiii.MutationProb == null
                ? "AUTO"
                : nsgaiii.MutationProb.Value.ToString(CultureInfo.InvariantCulture);
            page.NsgaiiiCrossoverProbabilityTextBox.Text = nsgaiii.CrossoverProb.ToString(CultureInfo.InvariantCulture);
            page.CrossoverComboBox.SelectedIndex = string.IsNullOrEmpty(nsgaiii.Crossover)
                ? 0 : (int)Enum.Parse(typeof(NsgaCrossoverType), nsgaiii.Crossover);
            page.MutationComboBox.SelectedIndex = string.IsNullOrEmpty(nsgaiii.Mutation)
                ? 0 : (int)Enum.Parse(typeof(NsgaMutationType), nsgaiii.Mutation);
            return page;
        }

        private void NsgaiiiSeedTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string value = textBox.Text;
            textBox.Text = InputValidator.IsAutoOrInt(value) ? value : "AUTO";
        }

        private void NsgaiiiMutationProbabilityTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string value = textBox.Text;
            textBox.Text = InputValidator.IsAutoOr0to1(value) ? value : "AUTO";
        }

        private void NsgaiiiCrossoverProbabilityTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string value = textBox.Text;
            textBox.Text = InputValidator.Is0to1(value) ? value : "0.9";
        }

        private void DefaultButton_Click(object sender, RoutedEventArgs e)
        {
            NsgaiiiSeedTextBox.Text = "AUTO";
            NsgaiiiMutationProbabilityTextBox.Text = "AUTO";
            NsgaiiiCrossoverProbabilityTextBox.Text = "0.9";
            CrossoverComboBox.SelectedIndex = 1;
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
            CrossoverSettings.UpdateLayout();
            CrossoverSettings.InvalidateVisual();
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
