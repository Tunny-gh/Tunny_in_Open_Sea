using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

using Prism.Commands;
using Prism.Mvvm;

using Tunny.Core.Settings;
using Tunny.Core.TEnum;
using Tunny.Core.Util;
using Tunny.Eto.Common;
using Tunny.Eto.Message;
using Tunny.Eto.Views;
using Tunny.WPF.Common;
using Tunny.WPF.ViewModels.Optimize;
using Tunny.WPF.ViewModels.Visualize;
using Tunny.WPF.Views.Pages;
using Tunny.WPF.Views.Pages.Expert;
using Tunny.WPF.Views.Pages.Optimize;
using Tunny.WPF.Views.Pages.Output;
using Tunny.WPF.Views.Pages.Visualize;

namespace Tunny.WPF.ViewModels
{
    public class MainWindowViewModel : BindableBase, IDisposable
    {
        private readonly OptimizePage _optimizePage;
        private readonly OptimizeViewModel _optimizeViewModel;
        private readonly Lazy<VisualizePage> _visualizePage;
        private readonly Lazy<OutputPage> _outputPage;
        private readonly Lazy<HelpPage> _helpPage;
        private readonly Lazy<ExpertPage> _expertPage;
        private static SharedItems SharedItems => SharedItems.Instance;
        private static CommonSharedItems CoSharedItems => CommonSharedItems.Instance;
        private bool _isMultiObjective;
        public bool IsMultiObjective { get => _isMultiObjective; set => SetProperty(ref _isMultiObjective, value); }
        private bool _isHumanInTheLoop;
        public bool IsHumanInTheLoop { get => _isHumanInTheLoop; set => SetProperty(ref _isHumanInTheLoop, value); }
        private bool _hasConstraint;
        public bool HasConstraint { get => _hasConstraint; set => SetProperty(ref _hasConstraint, value); }
        private Page _mainWindowFrame;
        public Page MainWindowFrame { get => _mainWindowFrame; set => SetProperty(ref _mainWindowFrame, value); }
        private string _windowTitle;
        public string WindowTitle { get => _windowTitle; set => SetProperty(ref _windowTitle, value); }

        public MainWindowViewModel()
        {
            _visualizePage = new Lazy<VisualizePage>();
            _outputPage = new Lazy<OutputPage>();
            _helpPage = new Lazy<HelpPage>();
            _expertPage = new Lazy<ExpertPage>();
            IsMultiObjective = CoSharedItems.Component.GhInOut.IsMultiObjective;
            IsHumanInTheLoop = CoSharedItems.Component.GhInOut.IsHumanInTheLoop;
            HasConstraint = CoSharedItems.Component.GhInOut.HasConstraint;
            UpdateTitle();
            ReportProgress("Welcome 🐟Tunny🐟 The next-gen Grasshopper optimization tool ", 0);

            _optimizeViewModel = new OptimizeViewModel();
            _optimizePage = new OptimizePage()
            {
                DataContext = _optimizeViewModel
            };
            MainWindowFrame = _optimizePage;
            EnableMainFrame = true;
            SharedItems.UpdateStudySummaries();

            if (CoSharedItems.Component.GhInOut.IsHumanInTheLoop)
            {
                SharedItems.Settings.Optimize.SamplerType = CoSharedItems.Component.GhInOut.IsMultiObjective
                    ? SamplerType.TPE
                    : SamplerType.PreferentialGp;
            }
            _optimizeViewModel.ChangeTargetSampler(SharedItems.Settings.Optimize.SamplerType);

            CheckPruner();
            CheckPythonInstalled();
        }

        private void UpdateTitle()
        {
            string storagePath = SharedItems.Settings.Storage.Path;
            string modeText = IsHumanInTheLoop ? "Human-In-The-Loop: " : string.Empty;
            modeText += IsMultiObjective ? "Multi-Objective" : "Single-Objective";
            if (HasConstraint)
            {
                modeText += " with Constraint";
            }
            WindowTitle = $"Tunny v{TEnvVariables.Version.ToString(2)} - {storagePath} | {modeText} Mode";
        }

        private static void CheckPruner()
        {
            TLog.MethodStart();
            SharedItems.Settings.Pruner.CheckStatus();
            if (SharedItems.Settings.Pruner.GetPrunerStatus() == PrunerStatus.PathError)
            {
                TunnyMessageBox.Error_PrunerPath();
            }
        }

        private void CheckPythonInstalled()
        {
            TLog.MethodStart();
            string tunnyAssembleVersion = TEnvVariables.Version.ToString();
            TSettings settings = SharedItems.Settings;
            bool isPythonDllExist = File.Exists(TEnvVariables.PythonDllPath);
            if (settings.CheckPythonLibraries || settings.Version != tunnyAssembleVersion || isPythonDllExist == false)
            {
                InstallPython();
                settings.CheckPythonLibraries = false;
                settings.Version = tunnyAssembleVersion;
                settings.Serialize(TEnvVariables.OptimizeSettingsPath);
            }
        }

        private DelegateCommand<VisualizeType?> _selectVisualizeTypeCommand;
        public ICommand SelectVisualizeTypeCommand
        {
            get
            {
                if (_selectVisualizeTypeCommand == null)
                {
                    _selectVisualizeTypeCommand = new DelegateCommand<VisualizeType?>(SelectVisualizeType);
                }
                return _selectVisualizeTypeCommand;
            }
        }

        private void SelectVisualizeType(VisualizeType? visualizeType)
        {
            if (visualizeType == null)
            {
                return;
            }
            var viewModel = (VisualizeViewModel)_visualizePage.Value.DataContext;
            viewModel.SetTargetVisualizeType(visualizeType.Value);
            MainWindowFrame = _visualizePage.Value;
        }

        private DelegateCommand _registerOptunaHubSamplerCommand;
        public ICommand RegisterOptunaHubSamplerCommand
        {
            get
            {
                if (_registerOptunaHubSamplerCommand == null)
                {
                    _registerOptunaHubSamplerCommand = new DelegateCommand(RegisterOptunaHubSampler);
                }
                return _registerOptunaHubSamplerCommand;
            }
        }
        private void RegisterOptunaHubSampler()
        {
        }

        private DelegateCommand<SelectSamplerType?> _selectSamplerCommand;
        public ICommand SelectSamplerCommand
        {
            get
            {
                if (_selectSamplerCommand == null)
                {
                    _selectSamplerCommand = new DelegateCommand<SelectSamplerType?>(SelectSampler);
                }
                return _selectSamplerCommand;
            }
        }

        private void SelectSampler(SelectSamplerType? selectSamplerType)
        {
            SamplerType samplerType;
            switch (selectSamplerType)
            {
                case null:
                    return;
                case SelectSamplerType.TPE:
                    samplerType = SamplerType.TPE;
                    break;
                case SelectSamplerType.GpOptuna:
                    samplerType = SamplerType.GP;
                    break;
                case SelectSamplerType.GpBoTorch:
                    samplerType = SamplerType.BoTorch;
                    break;
                case SelectSamplerType.PreferentialGp:
                    samplerType = SamplerType.PreferentialGp;
                    break;
                case SelectSamplerType.NSGAII:
                    samplerType = SamplerType.NSGAII;
                    break;
                case SelectSamplerType.NSGAIII:
                    samplerType = SamplerType.NSGAIII;
                    break;
                case SelectSamplerType.CmaEs:
                    samplerType = SamplerType.CmaEs;
                    break;
                case SelectSamplerType.Random:
                    samplerType = SamplerType.Random;
                    break;
                case SelectSamplerType.QMC:
                    samplerType = SamplerType.QMC;
                    break;
                case SelectSamplerType.BruteForce:
                    samplerType = SamplerType.BruteForce;
                    break;
                case SelectSamplerType.AUTO:
                    samplerType = SamplerType.AUTO;
                    break;
                case SelectSamplerType.MOEAD:
                    samplerType = SamplerType.MOEAD;
                    break;
                case SelectSamplerType.MoCmaEs:
                    samplerType = SamplerType.MoCmaEs;
                    break;
                case SelectSamplerType.DE:
                    samplerType = SamplerType.DE;
                    break;
                default:
                    samplerType = SamplerType.TPE;
                    break;
            }
            _optimizeViewModel.ChangeTargetSampler(samplerType);
            SharedItems.Settings.Optimize.SamplerType = samplerType;
            MainWindowFrame = _optimizePage;
        }

        private DelegateCommand<HelpType?> _openHelpPageCommand;
        public ICommand OpenHelpPageCommand
        {
            get
            {
                if (_openHelpPageCommand == null)
                {
                    _openHelpPageCommand = new DelegateCommand<HelpType?>(OpenHelpPage);
                }
                return _openHelpPageCommand;
            }
        }

        private void OpenHelpPage(HelpType? helpType)
        {
            _helpPage.Value.OpenSite(helpType);
            MainWindowFrame = _helpPage.Value;
        }

        private DelegateCommand _settingsSaveCommand;
        public ICommand SettingsSaveCommand
        {
            get
            {
                if (_settingsSaveCommand == null)
                {
                    _settingsSaveCommand = new DelegateCommand(SaveSettingsFile);
                }
                return _settingsSaveCommand;
            }
        }

        internal void SaveSettingsFile()
        {
            SharedItems.Settings.Optimize = _optimizeViewModel.GetCurrentSettings();
            SharedItems.Settings.Serialize(TEnvVariables.OptimizeSettingsPath);
        }

        private DelegateCommand _quickAccessNewStorageFileCommand;
        public ICommand QuickAccessNewStorageFileCommand
        {
            get
            {
                if (_quickAccessNewStorageFileCommand == null)
                {
                    _quickAccessNewStorageFileCommand = new DelegateCommand(QuickAccessNewStorageFile);
                }
                return _quickAccessNewStorageFileCommand;
            }
        }
        private void QuickAccessNewStorageFile()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "fish.log",
                DefaultExt = "log",
                Filter = @"Journal Storage(*.log)|*.log|SQLite Storage(*.db,*.sqlite)|*.db;*.sqlite|All Files (*.*)|*.*",
                Title = @"Set Tunny Result File Path",
            };

            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                SharedItems.Settings.Storage.Path = dialog.FileName;
                if (File.Exists(dialog.FileName) == false)
                {
                    SharedItems.Settings.Storage.CreateNewOptunaStorage(true);
                    SharedItems.UpdateStudySummaries();
                }
                UpdateTitle();
            }
        }

        private DelegateCommand _quickAccessFileOpenCommand;
        public ICommand QuickAccessStorageFileOpenCommand
        {
            get
            {
                if (_quickAccessFileOpenCommand == null)
                {
                    _quickAccessFileOpenCommand = new DelegateCommand(QuickAccessStorageFileOpen);
                }
                return _quickAccessFileOpenCommand;
            }
        }
        private void QuickAccessStorageFileOpen()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                FileName = "fish.log",
                DefaultExt = "log",
                Filter = @"Journal Storage(*.log)|*.log|SQLite Storage(*.db,*.sqlite)|*.db;*.sqlite|All Files (*.*)|*.*",
                Title = @"Set Tunny Result File Path",
            };

            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                SharedItems.Settings.Storage.Path = dialog.FileName;
                SharedItems.UpdateStudySummaries();
                UpdateTitle();
            }
        }

        private DelegateCommand _runOptunaDashboardCommand;
        public ICommand RunOptunaDashboardCommand
        {
            get
            {
                if (_runOptunaDashboardCommand == null)
                {
                    _runOptunaDashboardCommand = new DelegateCommand(RunOptunaDashboard);
                }

                return _runOptunaDashboardCommand;
            }
        }
        private void RunOptunaDashboard()
        {
            TLog.MethodStart();
            if (File.Exists(SharedItems.Settings.Storage.Path) == false)
            {
                TunnyMessageBox.Error_ResultFileNotExist();
                return;
            }
            string dashboardPath = Path.Combine(TEnvVariables.TunnyEnvPath, "python", "Scripts", "optuna-dashboard.exe");
            string storagePath = SharedItems.Settings.Storage.Path;

            var dashboard = new Optuna.Dashboard.Handler(dashboardPath, storagePath);
            dashboard.Run(true);
        }

        private DelegateCommand _runDesignExplorerCommand;
        public ICommand RunDesignExplorerCommand
        {
            get
            {
                if (_runDesignExplorerCommand == null)
                {
                    _runDesignExplorerCommand = new DelegateCommand(RunDesignExplorer);
                }
                return _runDesignExplorerCommand;
            }
        }
        private void RunDesignExplorer()
        {
            TLog.MethodStart();
            var selector = new TargetStudyNameSelector();
            selector.ShowModal();
        }

        public void Dispose()
        {
            _helpPage.Value.Dispose();
            GC.SuppressFinalize(this);
        }

        private DelegateCommand _installPythonCommand;
        public ICommand InstallPythonCommand
        {
            get
            {
                if (_installPythonCommand == null)
                {
                    _installPythonCommand = new DelegateCommand(InstallPython);
                }
                return _installPythonCommand;
            }
        }

        internal async void InstallPython()
        {
            TLog.MethodStart();
            var installer = new PythonInstaller(this);
            EnableMainFrame = false;
            _ = Task.Run(() => TunnyMessageBox.Info_PythonInstallStart());
            await installer.RunAsync();
            EnableMainFrame = true;
        }

        private string _statusBarNotifyText;
        public string StatusBarNotifyText { get => _statusBarNotifyText; set => SetProperty(ref _statusBarNotifyText, value); }
        private string _statusBarTrialNum;
        public string StatusBarTrialNum { get => _statusBarTrialNum; set => SetProperty(ref _statusBarTrialNum, value); }
        private string _statusBarETA;
        public string StatusBarETA { get => _statusBarETA; set => SetProperty(ref _statusBarETA, value); }
        private double _statusBarProgress;
        public double StatusBarProgress { get => _statusBarProgress; set => SetProperty(ref _statusBarProgress, value); }

        public void ReportProgress(string notifyText, double progress)
        {
            StatusBarNotifyText = notifyText;
            StatusBarProgress = progress;
        }

        public void ReportProgress(string notifyText, string trialNum, double progress)
        {
            StatusBarNotifyText = notifyText;
            StatusBarTrialNum = trialNum;
            StatusBarProgress = progress;
        }

        private DelegateCommand _listOutputCommand;
        public ICommand ListOutputCommand
        {
            get
            {
                if (_listOutputCommand == null)
                {
                    _listOutputCommand = new DelegateCommand(ListOutput);
                }
                return _listOutputCommand;
            }
        }

        private void ListOutput()
        {
            MainWindowFrame = _outputPage.Value;
        }

        private bool _enableMainFrame;
        public bool EnableMainFrame { get => _enableMainFrame; set => SetProperty(ref _enableMainFrame, value); }

        internal void UpdateExistStudySummaries()
        {
            _optimizeViewModel.UpdateExistStudySummaries();
            if (_visualizePage.IsValueCreated)
            {
                var viewModel = (VisualizeViewModel)_visualizePage.Value.DataContext;
                viewModel.UpdateExistStudySummaries();
            }
        }
    }
}
