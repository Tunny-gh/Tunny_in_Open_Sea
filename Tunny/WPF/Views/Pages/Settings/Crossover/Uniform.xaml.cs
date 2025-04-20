using System.Windows;
using System.Windows.Controls;

using Tunny.Core.Input;

namespace Tunny.WPF.Views.Pages.Settings.Crossover
{
    public partial class Uniform : Page
    {
        public Uniform()
        {
            InitializeComponent();
        }

        private void SwappingProbTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string value = textBox.Text;
            textBox.Text = InputValidator.Is0to1(value) ? value : "0.5";
        }
    }
}
