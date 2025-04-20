using System.Windows;
using System.Windows.Controls;

using Tunny.Core.Input;
using Tunny.WPF.Common;

namespace Tunny.WPF.Views.Pages.Settings.Mutation
{
    public partial class Polynomial : Page, IMutationParam
    {
        public Polynomial()
        {
            InitializeComponent();
        }

        public double ToParameter()
        {
            double eta = InputValidator.IsPositiveDouble(EtaTextBox.Text, false)
                ? double.Parse(EtaTextBox.Text, System.Globalization.CultureInfo.InvariantCulture)
                : 20;
            return eta;
        }

        private void EtaTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string value = textBox.Text;
            textBox.Text = InputValidator.IsPositiveDouble(value, false) ? value : "20";
        }
    }
}
