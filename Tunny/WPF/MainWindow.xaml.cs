using System.IO;
using System.Windows.Controls.Ribbon;

using Tunny.Component.Optimizer;
using Tunny.Core.Settings;
using Tunny.Core.Util;
using Tunny.Eto.Common;
using Tunny.Eto.Message;
using Tunny.WPF.Common;
using Tunny.WPF.ViewModels;

namespace Tunny.WPF
{
    public partial class MainWindow : RibbonWindow
    {
        private static SharedItems SharedItems => SharedItems.Instance;
        private static CommonSharedItems CoSharedItems => CommonSharedItems.Instance;

        public MainWindow(OptimizeComponentBase component)
        {
            TLog.MethodStart();
            SharedItems.TunnyWindow = this;
            CoSharedItems.GH_DocumentEditor.DisableUI();
            CoSharedItems.Component = component;

            if (!TSettings.TryLoadFromJson(out TSettings settings))
            {
                TunnyMessageBox.Warn_SettingsJsonFileLoadFail();
            }
            else
            {
                if (component.GhInOut.IsHumanInTheLoop && component.GhInOut.IsMultiObjective)
                {
                    settings.Optimize.Sampler.Tpe.ConstantLiar = true;
                }
                SharedItems.Settings = settings;
                CoSharedItems.Settings = settings;
            }

            if (File.Exists(TEnvVariables.QuitFishingPath))
            {
                File.Delete(TEnvVariables.QuitFishingPath);
            }

            try
            {
                InitializeComponent();
            }
            catch (System.Exception)
            {
                TunnyMessageBox.Error_InitializeTunnyUI();
                CoSharedItems.GH_DocumentEditor.EnableUI();
                Close();
                return;
            }
            Closing += ClosingEventHandler;
        }

        private void ClosingEventHandler(object sender, System.ComponentModel.CancelEventArgs e)
        {
            TLog.MethodStart();
            ((MainWindowViewModel)DataContext).SaveSettingsFile();
            CoSharedItems.GH_DocumentEditor.EnableUI();

            SharedItems.Clear();
            CoSharedItems.Clear();
        }
    }
}
