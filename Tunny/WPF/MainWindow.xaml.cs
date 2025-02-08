using System.Windows.Controls.Ribbon;

using Tunny.Component.Optimizer;
using Tunny.Core.Settings;
using Tunny.Core.Util;
using Tunny.WPF.Common;
using Tunny.WPF.ViewModels;

namespace Tunny.WPF
{
    public partial class MainWindow : RibbonWindow
    {
        private static SharedItems SharedItems => SharedItems.Instance;

        public MainWindow(OptimizeComponentBase component)
        {
            TLog.MethodStart();
            SharedItems.TunnyWindow = this;
            SharedItems.GH_DocumentEditor.DisableUI();
            SharedItems.Component = component;

            if (!TSettings.TryLoadFromJson(out TSettings settings))
            {
                TunnyMessageBox.Warn_SettingsJsonFileLoadFail();
            }
            else
            {
                SharedItems.Settings = settings;
            }

            InitializeComponent();
            Closing += ClosingEventHandler;
        }

        private void ClosingEventHandler(object sender, System.ComponentModel.CancelEventArgs e)
        {
            TLog.MethodStart();
            ((MainWindowViewModel)DataContext).SaveSettingsFile();
            SharedItems.GH_DocumentEditor.EnableUI();

            SharedItems.Clear();
        }
    }
}
