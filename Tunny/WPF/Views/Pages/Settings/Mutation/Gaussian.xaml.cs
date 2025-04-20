using System.Windows;
using System.Windows.Controls;

using Tunny.Core.Input;
using Tunny.WPF.Common;

namespace Tunny.WPF.Views.Pages.Settings.Mutation
{
    public partial class Gaussian : Page, IMutationParam
    {
        public Gaussian()
        {
            InitializeComponent();
        }

        public double ToParameter()
        {
            double sigmaFactor = InputValidator.IsPositiveDouble(SigmaFactorTextBox.Text, false)
                ? double.Parse(SigmaFactorTextBox.Text, System.Globalization.CultureInfo.InvariantCulture)
                : 0.033;
            return sigmaFactor;
        }

        private void SigmaFactorTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string value = textBox.Text;
            textBox.Text = InputValidator.IsPositiveDouble(value, false) ? value : "0.033";
        }
    }
}
