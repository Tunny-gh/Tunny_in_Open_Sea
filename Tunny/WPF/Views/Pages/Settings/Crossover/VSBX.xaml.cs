using System.Windows;
using System.Windows.Controls;

using Tunny.Core.Input;

namespace Tunny.WPF.Views.Pages.Settings.Crossover
{
    public partial class VSBX : Page
    {
        public VSBX()
        {
            InitializeComponent();
        }

        private void EtaTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string value = textBox.Text;
            textBox.Text = InputValidator.IsAutoOrPositiveDouble(value, false) ? value : "AUTO";
        }
    }
}
