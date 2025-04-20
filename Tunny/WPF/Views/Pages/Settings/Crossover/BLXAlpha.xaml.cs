using System.Globalization;
using System.Windows;
using System.Windows.Controls;

using Tunny.Core.Input;
using Tunny.WPF.Common;

namespace Tunny.WPF.Views.Pages.Settings.Crossover
{
    public partial class BLXAlpha : Page, ICrossoverParam
    {
        public BLXAlpha()
        {
            InitializeComponent();
        }

        public double?[] ToParameters()
        {
            double alpha = double.Parse(AlphaTextBox.Text, CultureInfo.InvariantCulture);
            return new double?[] { alpha };
        }

        private void AlphaTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string value = textBox.Text;
            textBox.Text = InputValidator.IsPositiveDouble(value, false) ? value : "0.5";
        }
    }
}
