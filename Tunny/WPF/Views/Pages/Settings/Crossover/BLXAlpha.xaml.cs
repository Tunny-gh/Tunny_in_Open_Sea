using System.Windows;
using System.Windows.Controls;

using Tunny.Core.Input;

namespace Tunny.WPF.Views.Pages.Settings.Crossover
{
    public partial class BLXAlpha : Page
    {
        public BLXAlpha()
        {
            InitializeComponent();
        }

        private void AlphaTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string value = textBox.Text;
            textBox.Text = InputValidator.IsPositiveDouble(value, false) ? value : "0.5";
        }
    }
}
