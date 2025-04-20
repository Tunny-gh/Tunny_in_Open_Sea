using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

using Tunny.Core.Input;
using Tunny.WPF.Common;

namespace Tunny.WPF.Views.Pages.Settings.Crossover
{
    public partial class VSBX : Page, ICrossoverParam
    {
        public VSBX()
        {
            InitializeComponent();
        }

        public double?[] ToParameters()
        {
            double? eta = EtaTextBox.Text.Equals("auto", StringComparison.OrdinalIgnoreCase)
                ? null
                : (double?)double.Parse(EtaTextBox.Text, CultureInfo.InvariantCulture);
            return new double?[] { eta };
        }

        private void EtaTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string value = textBox.Text;
            textBox.Text = InputValidator.IsAutoOrPositiveDouble(value, false) ? value : "AUTO";
        }
    }
}
