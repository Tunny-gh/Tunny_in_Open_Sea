using System.Windows;
using System.Windows.Controls;

using Tunny.Core.Input;

namespace Tunny.WPF.Views.Pages.Settings.Crossover
{
    public partial class UNDX : Page
    {
        public UNDX()
        {
            InitializeComponent();
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
