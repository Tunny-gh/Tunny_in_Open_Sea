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
            double sigmaXi = InputValidator.IsPositiveDouble(SigmaXiTextBox.Text, false)
                ? double.Parse(SigmaXiTextBox.Text, System.Globalization.CultureInfo.InvariantCulture)
                : 0.5;
            double? sigmaEta = InputValidator.IsAutoOrPositiveDouble(SigmaEtaTextBox.Text, false)
                ? double.Parse(SigmaEtaTextBox.Text, System.Globalization.CultureInfo.InvariantCulture)
                : null;
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
