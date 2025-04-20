using System.Windows;
using System.Windows.Controls;

using Tunny.Core.Input;
using Tunny.WPF.Common;

namespace Tunny.WPF.Views.Pages.Settings.Crossover
{
    public partial class Uniform : Page, ICrossoverParam
    {
        public Uniform()
        {
            InitializeComponent();
        }

        public double?[] ToParameters()
        {
            double? swappingProb = InputValidator.Is0to1(SwappingProbTextBox.Text)
                ? double.Parse(SwappingProbTextBox.Text, System.Globalization.CultureInfo.InvariantCulture)
                : 0.5;
            return new double?[] { swappingProb };
        }

        private void SwappingProbTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string value = textBox.Text;
            textBox.Text = InputValidator.Is0to1(value) ? value : "0.5";
        }
    }
}
