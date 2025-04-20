using System.Windows.Controls;

using Tunny.WPF.Common;

namespace Tunny.WPF.Views.Pages.Settings.Mutation
{
    public partial class Uniform : Page, IMutationParam
    {
        public Uniform()
        {
            InitializeComponent();
        }

        public double ToParameter()
        {
            return 0;
        }
    }
}
