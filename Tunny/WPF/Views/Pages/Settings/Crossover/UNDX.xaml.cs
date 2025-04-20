using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

using Tunny.Core.Input;
using Tunny.WPF.Common;

namespace Tunny.WPF.Views.Pages.Settings.Crossover
{
    public partial class UNDX : Page, ICrossoverParam
    {
        public UNDX()
        {
            InitializeComponent();
        }

        public double?[] ToParameters()
        {
            double sigmaXi = double.Parse(SigmaXiTextBox.Text, CultureInfo.InvariantCulture);
            double? sigmaEta = SigmaEtaTextBox.Text.Equals("auto", StringComparison.OrdinalIgnoreCase)
                ? null
                : (double?)double.Parse(SigmaEtaTextBox.Text, CultureInfo.InvariantCulture);
            return new double?[] { sigmaXi, sigmaEta };
        }

        private void SigmaXiTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string value = textBox.Text;
            textBox.Text = InputValidator.IsPositiveDouble(value, false) ? value : "0.5";
        }

        private void SigmaEtaTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string value = textBox.Text;
            textBox.Text = InputValidator.IsAutoOrPositiveDouble(value, false) ? value : "AUTO";
        }
    }
}
