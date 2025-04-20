using System.Windows;
using System.Windows.Controls;

using Tunny.Core.Input;
using Tunny.WPF.Common;

namespace Tunny.WPF.Views.Pages.Settings.Crossover
{
    public partial class SPX : Page, ICrossoverParam
    {
        public SPX()
        {
            InitializeComponent();
        }

        public double?[] ToParameters()
        {
            double? epsilon = InputValidator.IsAutoOrPositiveDouble(EpsilonTextBox.Text, false)
                ? double.Parse(EpsilonTextBox.Text, System.Globalization.CultureInfo.InvariantCulture)
                : null;
            return new double?[] { epsilon };
        }

        private void EpsilonTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string value = textBox.Text;
            textBox.Text = InputValidator.IsAutoOrPositiveDouble(value, false) ? value : "AUTO";
        }
    }
}
