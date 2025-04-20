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
            double? eta = InputValidator.IsAutoOrPositiveDouble(EtaTextBox.Text, false)
                ? double.Parse(EtaTextBox.Text, System.Globalization.CultureInfo.InvariantCulture)
                : null;
            double uniformCrossoverProb = InputValidator.IsAutoOr0to1(UniformCrossoverProbTextBox.Text, true)
                ? double.Parse(UniformCrossoverProbTextBox.Text, System.Globalization.CultureInfo.InvariantCulture)
                : 0.0;
            double useGenChild = InputValidator.IsAutoOr0to1(UseGenChildProbTextBox.Text)
                ? double.Parse(UseGenChildProbTextBox.Text, System.Globalization.CultureInfo.InvariantCulture)
                : 1.0;
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
            textBox.Text = InputValidator.IsAutoOr0to1(value, true) ? value : "0.0";
        }

        private void UseGenChildTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string value = textBox.Text;
            textBox.Text = InputValidator.IsAutoOr0to1(value) ? value : "1.0";
        }
    }
}
