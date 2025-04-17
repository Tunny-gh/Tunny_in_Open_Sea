using System.Windows;
using System.Windows.Controls;

using Tunny.Core.Input;

namespace Tunny.WPF.Views.Pages.Settings.Mutation
{
    public partial class Polynomial : Page
    {
        public Polynomial()
        {
            InitializeComponent();
        }

        private void EtaTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string value = textBox.Text;
            textBox.Text = InputValidator.IsPositiveDouble(value, false) ? value : "20";
        }
    }
}
