using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

using Tunny.Core.Input;
using Tunny.WPF.Common;

namespace Tunny.WPF.Views.Pages.Settings.Crossover
{
    public partial class SBX : Page, ICrossoverParam
    {
        public SBX()
        {
            InitializeComponent();
        }

        public double?[] ToParameters()
        {
            double? eta = EtaTextBox.Text.Equals("auto", StringComparison.OrdinalIgnoreCase)
                ? null
                : (double?)double.Parse(EtaTextBox.Text, CultureInfo.InvariantCulture);
            double uniformCrossoverProb = double.Parse(UniformCrossoverProbTextBox.Text, CultureInfo.InvariantCulture);
            double useGenChild = double.Parse(UseGenChildProbTextBox.Text, CultureInfo.InvariantCulture);
            return new double?[] { eta, uniformCrossoverProb, useGenChild };
        }

        private void EtaTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string value = textBox.Text;
            textBox.Text = InputValidator.IsAutoOrPositiveDouble(value, false) ? value : "AUTO";
        }

        private void UniformCrossoverProbTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string value = textBox.Text;
            textBox.Text = InputValidator.Is0to1(value, true) ? value : "0.0";
        }

        private void UseGenChildTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string value = textBox.Text;
            textBox.Text = InputValidator.Is0to1(value) ? value : "1.0";
        }
    }
}
