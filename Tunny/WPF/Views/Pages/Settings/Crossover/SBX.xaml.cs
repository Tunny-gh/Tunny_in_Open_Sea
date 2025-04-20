using System.Windows;
using System.Windows.Controls;

using Tunny.Core.Input;

namespace Tunny.WPF.Views.Pages.Settings.Crossover
{
    public partial class SBX : Page
    {
        public SBX()
        {
            InitializeComponent();
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
