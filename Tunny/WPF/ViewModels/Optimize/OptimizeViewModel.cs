using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Prism.Commands;
using Prism.Mvvm;

using Rhino.Display;

using Tunny.Core.Handler;
using Tunny.Core.Settings;
using Tunny.Core.Storage;
using Tunny.Core.TEnum;
using Tunny.Core.Util;
using Tunny.Process;
using Tunny.WPF.Common;
using Tunny.WPF.Models;
using Tunny.WPF.Views.Pages.Optimize;
using Tunny.WPF.Views.Pages.Settings.Sampler;

namespace Tunny.WPF.ViewModels.Optimize
{
    internal sealed class OptimizeViewModel : BindableBase
    {
        private const string SamplerTypeLabelPrefix = "SamplerType: ";
        private const string TrialNumberLabelPrefix = "Trial: ";
        private static SharedItems SharedItems => SharedItems.Instance;
        private readonly TSettings _settings;
        private MainWindowViewModel _windowViewModel;
        private LiveChartPage _chart1;
        private LiveChartPage _chart2;
        private Lazy<TPESettingsPage> _tpePage;
        private Lazy<GPOptunaSettingsPage> _gpOptunaPage;
        private Lazy<GPBoTorchSettingsPage> _gpBoTorchPage;
        private Lazy<NSGAIISettingsPage> _nsgaiiPage;
        private Lazy<NSGAIIISettingsPage> _nsgaiiiPage;
        private Lazy<CmaEsSettingsPage> _cmaesPage;
        private Lazy<RandomSettingsPage> _randomPage;
        private Lazy<QmcSettingsPage> _qmcPage;
        private Lazy<BruteForceSettingsPage> _bruteForcePage;
        private Lazy<AutoSettingsPage> _autoPage;
        private Lazy<MOEADSettingsPage> _moeadPage;
        private Lazy<MoCmaEsSettingsPage> _moCmaEsPage;
        private Lazy<DESettingsPage> _dePage;

        private string _trialNumberParam1Label;
        public string TrialNumberParam1Label { get => _trialNumberParam1Label; set => SetProperty(ref _trialNumberParam1Label, value); }
        private string _trialNumberParam1;
        public string TrialNumberParam1 { get => _trialNumberParam1; set => SetProperty(ref _trialNumberParam1, value); }
        private Visibility _trialNumberParam2Visibility;
        public Visibility TrialNumberParam2Visibility { get => _trialNumberParam2Visibility; set => SetProperty(ref _trialNumberParam2Visibility, value); }
        private string _trialNumberParam2Label;
        public string TrialNumberParam2Label { get => _trialNumberParam2Label; set => SetProperty(ref _trialNumberParam2Label, value); }
        private string _trialNumberParam2;
        public string TrialNumberParam2 { get => _trialNumberParam2; set => SetProperty(ref _trialNumberParam2, value); }
        private string _timeout;
        public string Timeout { get => _timeout; set => SetProperty(ref _timeout, value); }
        private string _studyName;
        public string StudyName { get => _studyName; set => SetProperty(ref _studyName, value); }
        private bool? _enableStudyNameTextBox;
        public bool? EnableStudyNameTextBox { get => _enableStudyNameTextBox; set => SetProperty(ref _enableStudyNameTextBox, value); }
        private bool? _isInMemory;
        public bool? IsInMemory
        {
            get => _isInMemory;
            set
            {
                if (value.Value == true)
                {
                    if (IsContinue == true)
                    {
                        IsContinue = false;
                    }
                    if (IsCopy == true)
                    {
                        IsCopy = false;
                    }
                    if (EnableCopyCheckbox == null || EnableCopyCheckbox == true)
                    {
                        EnableCopyCheckbox = false;
                    }
                    if (EnableExistStudyComboBox == null || EnableExistStudyComboBox == true)
                    {
                        EnableExistStudyComboBox = false;
                    }
                }
                SetProperty(ref _isInMemory, value);
            }
        }
        private bool? _isContinue;
        public bool? IsContinue
        {
            get => _isContinue;
            set
            {
                EnableExistStudyComboBox = value;
                if (value.Value == true)
                {
                    if (IsInMemory == true)
                    {
                        IsInMemory = false;
                    }
                    if (EnableCopyCheckbox == false)
                    {
                        EnableCopyCheckbox = true;
                    }
                    if (EnableStudyNameTextBox == null || EnableStudyNameTextBox == true)
                    {
                        EnableStudyNameTextBox = false;
                    }
                }
                else
                {
                    if (EnableCopyCheckbox == null || EnableCopyCheckbox == true)
                    {
                        EnableCopyCheckbox = false;
                        IsCopy = false;
                    }
                    if (EnableStudyNameTextBox == false)
                    {
                        EnableStudyNameTextBox = true;
                    }
                }
                SetProperty(ref _isContinue, value);
            }
        }
        private bool? _isCopy;
        public bool? IsCopy
        {
            get => _isCopy;
            set
            {
                EnableStudyNameTextBox = value;
                SetProperty(ref _isCopy, value);
            }
        }
        private bool? _enableCopyCheckbox;
        public bool? EnableCopyCheckbox { get => _enableCopyCheckbox; set => SetProperty(ref _enableCopyCheckbox, value); }
        private bool? _enableIgnoreDuplicateSampling;
        public bool? EnableIgnoreDuplicateSampling { get => _enableIgnoreDuplicateSampling; set => SetProperty(ref _enableIgnoreDuplicateSampling, value); }
        private bool? _disableViewportUpdate;
        public bool? DisableViewportUpdate { get => _disableViewportUpdate; set => SetProperty(ref _disableViewportUpdate, value); }
        private bool? _minimizeRhinoWindow;
        public bool? MinimizeRhinoWindow { get => _minimizeRhinoWindow; set => SetProperty(ref _minimizeRhinoWindow, value); }
        private bool _enableRunOptimizeButton;
        public bool EnableRunOptimizeButton { get => _enableRunOptimizeButton; set => SetProperty(ref _enableRunOptimizeButton, value); }
        private bool _enableStopOptimizeButton;
        public bool EnableStopOptimizeButton { get => _enableStopOptimizeButton; set => SetProperty(ref _enableStopOptimizeButton, value); }
        private string _samplerTypeLabel;
        public string SamplerTypeLabel { get => _samplerTypeLabel; set => SetProperty(ref _samplerTypeLabel, value); }
        private Page _optimizeSettingsPage;
        public Page OptimizeSettingsPage { get => _optimizeSettingsPage; set => SetProperty(ref _optimizeSettingsPage, value); }
        private LiveChartPage _liveChart1;
        public LiveChartPage LiveChart1 { get => _liveChart1; set => SetProperty(ref _liveChart1, value); }
        private LiveChartPage _liveChart2;
        public LiveChartPage LiveChart2 { get => _liveChart2; set => SetProperty(ref _liveChart2, value); }
        private ObservableCollection<ObjectiveSettingItem> _objectiveSettingItems;
        public ObservableCollection<ObjectiveSettingItem> ObjectiveSettingItems { get => _objectiveSettingItems; set => SetProperty(ref _objectiveSettingItems, value); }
        private ObservableCollection<VariableSettingItem> _variableSettingItems;
        public ObservableCollection<VariableSettingItem> VariableSettingItems { get => _variableSettingItems; set => SetProperty(ref _variableSettingItems, value); }
        private NameComboBoxItem _selectedExistStudy;
        public NameComboBoxItem SelectedExistStudy { get => _selectedExistStudy; set => SetProperty(ref _selectedExistStudy, value); }
        private ObservableCollection<NameComboBoxItem> _existingStudies;
        public ObservableCollection<NameComboBoxItem> ExistingStudies { get => _existingStudies; set => SetProperty(ref _existingStudies, value); }
        private bool? _enableExistStudyComboBox;
        public bool? EnableExistStudyComboBox { get => _enableExistStudyComboBox; set => SetProperty(ref _enableExistStudyComboBox, value); }

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public OptimizeViewModel()
        {
            _settings = SharedItems.Settings;
            InitializeUIValues();
            InitializeChart();
            InitializeSamplerPage();
            ChangeTargetSampler(_settings.Optimize.SamplerType);
            InitializeObjectivesAndVariables();
        }

        private void InitializeUIValues()
        {
            Timeout = _settings.Optimize.Timeout.ToString(CultureInfo.InvariantCulture);
            StudyName = _settings.Optimize.StudyName;

            IsInMemory = _settings.Storage.Type == StorageType.InMemory;
            IsContinue = _settings.Optimize.ContinueStudy;
            IsCopy = _settings.Optimize.CopyStudy;
            EnableStudyNameTextBox = !IsContinue;

            EnableIgnoreDuplicateSampling = _settings.Optimize.IgnoreDuplicateSampling;
            DisableViewportUpdate = _settings.Optimize.DisableViewportDrawing;
            MinimizeRhinoWindow = _settings.Optimize.MinimizeRhinoWindow;

            EnableRunOptimizeButton = true;
        }

        private void InitializeChart()
        {
            _chart1 = new LiveChartPage();
            LiveChart1 = _chart1;
            _chart2 = new LiveChartPage(2);
            LiveChart2 = _chart2;
        }

        private void InitializeSamplerPage()
        {
            _tpePage = new Lazy<TPESettingsPage>(() => TPESettingsPage.FromSettings(_settings));
            _gpOptunaPage = new Lazy<GPOptunaSettingsPage>(() => GPOptunaSettingsPage.FromSettings(_settings));
            _gpBoTorchPage = new Lazy<GPBoTorchSettingsPage>(() => GPBoTorchSettingsPage.FromSettings(_settings));
            _nsgaiiPage = new Lazy<NSGAIISettingsPage>(() => NSGAIISettingsPage.FromSettings(_settings));
            _nsgaiiiPage = new Lazy<NSGAIIISettingsPage>(() => NSGAIIISettingsPage.FromSettings(_settings));
            _cmaesPage = new Lazy<CmaEsSettingsPage>(() => CmaEsSettingsPage.FromSettings(_settings));
            _randomPage = new Lazy<RandomSettingsPage>(() => RandomSettingsPage.FromSettings(_settings));
            _qmcPage = new Lazy<QmcSettingsPage>(() => QmcSettingsPage.FromSettings(_settings));
            _bruteForcePage = new Lazy<BruteForceSettingsPage>(() => BruteForceSettingsPage.FromSettings(_settings));
            _autoPage = new Lazy<AutoSettingsPage>(() => AutoSettingsPage.FromSettings(_settings));
            _moeadPage = new Lazy<MOEADSettingsPage>(() => MOEADSettingsPage.FromSettings(_settings));
            _moCmaEsPage = new Lazy<MoCmaEsSettingsPage>(() => MoCmaEsSettingsPage.FromSettings(_settings));
            _dePage = new Lazy<DESettingsPage>(() => DESettingsPage.FromSettings(_settings));
        }

        public void ChangeTargetSampler(SamplerType samplerType)
        {
            ITrialNumberParam param;
            switch (samplerType)
            {
                case SamplerType.TPE:
                    param = _tpePage.Value;
                    OptimizeSettingsPage = _tpePage.Value;
                    break;
                case SamplerType.GP:
                    param = _gpOptunaPage.Value;
                    OptimizeSettingsPage = _gpOptunaPage.Value;
                    break;
                case SamplerType.BoTorch:
                    param = _gpBoTorchPage.Value;
                    OptimizeSettingsPage = _gpBoTorchPage.Value;
                    break;
                case SamplerType.NSGAII:
                    param = _nsgaiiPage.Value;
                    OptimizeSettingsPage = _nsgaiiPage.Value;
                    break;
                case SamplerType.NSGAIII:
                    param = _nsgaiiiPage.Value;
                    OptimizeSettingsPage = _nsgaiiiPage.Value;
                    break;
                case SamplerType.CmaEs:
                    param = _cmaesPage.Value;
                    OptimizeSettingsPage = _cmaesPage.Value;
                    break;
                case SamplerType.Random:
                    param = _randomPage.Value;
                    OptimizeSettingsPage = _randomPage.Value;
                    break;
                case SamplerType.QMC:
                    param = _qmcPage.Value;
                    OptimizeSettingsPage = _qmcPage.Value;
                    break;
                case SamplerType.BruteForce:
                    param = _bruteForcePage.Value;
                    OptimizeSettingsPage = _bruteForcePage.Value;
                    break;
                case SamplerType.AUTO:
                    param = _autoPage.Value;
                    OptimizeSettingsPage = _autoPage.Value;
                    break;
                case SamplerType.MOEAD:
                    param = _moeadPage.Value;
                    OptimizeSettingsPage = _moeadPage.Value;
                    break;
                case SamplerType.MoCmaEs:
                    param = _moCmaEsPage.Value;
                    OptimizeSettingsPage = _moCmaEsPage.Value;
                    break;
                case SamplerType.DE:
                    param = _dePage.Value;
                    OptimizeSettingsPage = _dePage.Value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(samplerType), samplerType, null);
            }
            SamplerTypeLabel = SamplerTypeLabelPrefix + samplerType;
            TrialNumberParam1Label = param.Param1Label;
            TrialNumberParam2Label = param.Param2Label;
            TrialNumberParam2Visibility = param.Param2Visibility;

            if (samplerType == SamplerType.NSGAII || samplerType == SamplerType.NSGAIII || samplerType == SamplerType.MOEAD)
            {
                int total = _settings.Optimize.NumberOfTrials;
                int populationSize = _settings.Optimize.Sampler.NsgaII.PopulationSize > 2
                    ? _settings.Optimize.Sampler.NsgaII.PopulationSize
                    : 2;
                int numGeneration = total / populationSize;
                TrialNumberParam1 = numGeneration.ToString(CultureInfo.InvariantCulture);
                TrialNumberParam2 = populationSize.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                TrialNumberParam1 = _settings.Optimize.NumberOfTrials.ToString(CultureInfo.InvariantCulture);
            }
        }

        private void InitializeObjectivesAndVariables()
        {
            Util.GrasshopperInOut ghIO = SharedItems.Component.GhInOut;
            ObjectiveSettingItems = new ObservableCollection<ObjectiveSettingItem>();
            foreach (string item in ghIO.Objectives.GetNickNames())
            {
                ObjectiveSettingItems.Add(new ObjectiveSettingItem
                {
                    Name = item,
                    Minimize = true
                });
            }
            VariableSettingItems = new ObservableCollection<VariableSettingItem>();
            foreach (Core.Input.VariableBase item in ghIO.Variables)
            {
                if (item is Core.Input.NumberVariable numVariable)
                {
                    VariableSettingItems.Add(new VariableSettingItem
                    {
                        Name = numVariable.NickName,
                        Low = numVariable.LowerBond,
                        High = numVariable.UpperBond,
                        Step = numVariable.Epsilon,
                        IsLogScale = false
                    });
                }
            }
        }

        private DelegateCommand _runOptimize;
        public ICommand RunOptimize
        {
            get
            {
                if (_runOptimize == null)
                {
                    _runOptimize = new DelegateCommand(PerformRunOptimize);
                }
                return _runOptimize;
            }
        }
        private async void PerformRunOptimize()
        {
            TLog.MethodStart();
            InitializePerformRunOptimize();

            try
            {
                if (!CheckContinueAndCopy())
                {
                    return;
                }
                await OptimizeProcess.RunAsync(this);
            }
            finally
            {
                FinalizeWindow();
            }
        }

        private void InitializePerformRunOptimize()
        {
            _windowViewModel = SharedItems.TunnyWindow.DataContext as MainWindowViewModel;
            if (!CheckPythonDllExistence())
            {
                return;
            }

            EnableRunOptimizeButton = false;
            EnableStopOptimizeButton = true;
            _chart1.ClearPoints();
            _chart2.ClearPoints();

            _settings.Optimize = GetCurrentSettings(true);
            SetupWindow();

            _windowViewModel.ReportProgress("Start Optimizing...", 0);
            InitializeOptimizeProcess();
        }

        private bool CheckPythonDllExistence()
        {
            if (File.Exists(TEnvVariables.PythonDllPath) == false)
            {
                MessageBoxResult result = TunnyMessageBox.Error_PythonDllNotFound();
                if (result == MessageBoxResult.Yes)
                {
                    _windowViewModel.InstallPython();
                }
                return false;
            }
            return true;
        }

        private bool CheckContinueAndCopy()
        {
            bool result = true;
            if (IsContinue == true)
            {
                result = CheckContinueStudy();
            }
            if (result && IsCopy == true)
            {
                result = CheckCopyStudy();
            }
            return result;
        }

        private bool CheckContinueStudy()
        {
            if (SelectedExistStudy == null)
            {
                TunnyMessageBox.Error_NoSourceStudySelected();
                return false;
            }
            _settings.Optimize.StudyName = SelectedExistStudy.Name;
            return true;
        }

        private bool CheckCopyStudy()
        {
            if (IsCopy == true)
            {
                if (SelectedExistStudy == null)
                {
                    TunnyMessageBox.Error_NoSourceStudySelected();
                    return false;
                }
                if (StudyName == SelectedExistStudy.Name)
                {
                    TunnyMessageBox.Error_CopySourceAndDestinationAreSame();
                    return false;
                }

                _windowViewModel.ReportProgress("Copying study...", 0);
                if (StudyName.Equals("AUTO", StringComparison.OrdinalIgnoreCase))
                {
                    _settings.Optimize.StudyName = "no-name-" + Guid.NewGuid().ToString("D");
                }
                new StorageHandler().DuplicateStudyInStorage(SelectedExistStudy.Name, _settings.Optimize.StudyName, _settings.Storage);
            }
            return true;
        }

        private void InitializeOptimizeProcess()
        {
            Progress<ProgressState> progress = CreateProgressAction();
            SharedItems.Settings = _settings;
            SharedItems.AddProgress(progress);
        }

        private Progress<ProgressState> CreateProgressAction()
        {
            return new Progress<ProgressState>(value =>
            {
                TLog.MethodStart();
                SharedItems.Component.UpdateGrasshopper(value);
                _windowViewModel.ReportProgress("Running optimization", $"{TrialNumberLabelPrefix}{value.TrialNumber + 1}", value.PercentComplete);

                if (!value.IsReportOnly)
                {
                    Input.Objective objectives = SharedItems.Component.GhInOut.Objectives;
                    _chart1.AddPoint(value.TrialNumber + 1, objectives.Numbers);
                    _chart2.AddPoint(value.TrialNumber + 1, objectives.Numbers);
                }
            });
        }

        private void SetupWindow()
        {
            RhinoView.EnableDrawing = !_settings.Optimize.DisableViewportDrawing;

            if (_settings.Optimize.MinimizeRhinoWindow)
            {
                RhinoWindowHandle(6);
            }
        }
        private void FinalizeWindow()
        {
            EnableRunOptimizeButton = true;
            EnableStopOptimizeButton = false;
            RhinoView.EnableDrawing = true;

            if (_settings.Optimize.MinimizeRhinoWindow)
            {
                RhinoWindowHandle(9);
            }
            _windowViewModel.ReportProgress("🐟Finish🐟", 100);
            SharedItems.UpdateStudySummaries();
        }

        private static void RhinoWindowHandle(int status)
        {
            IntPtr rhinoWindow = Rhino.RhinoApp.MainWindowHandle();
            ShowWindow(rhinoWindow, status);
        }

        private DelegateCommand _stopOptimize;
        public ICommand StopOptimize
        {
            get
            {
                if (_stopOptimize == null)
                {
                    _stopOptimize = new DelegateCommand(PerformStopOptimize);
                }

                return _stopOptimize;
            }
        }

        private void PerformStopOptimize()
        {
            TLog.MethodStart();
            OptimizeProcess.IsForcedStopOptimize = true;
            FileStream fs = null;
            try
            {
                fs = File.Create(TEnvVariables.QuitFishingPath);
            }
            finally
            {
                fs?.Dispose();
            }
        }

        internal Core.Settings.Optimize GetCurrentSettings(bool computeAutoValue = false)
        {
            TLog.MethodStart();
            var sampler = new Sampler
            {
                Tpe = _tpePage.Value.ToSettings(),
                GP = _gpOptunaPage.Value.ToSettings(),
                BoTorch = _gpBoTorchPage.Value.ToSettings(),
                NsgaII = _nsgaiiPage.Value.ToSettings(),
                NsgaIII = _nsgaiiiPage.Value.ToSettings(),
                CmaEs = _cmaesPage.Value.ToSettings(),
                Random = _randomPage.Value.ToSettings(),
                QMC = _qmcPage.Value.ToSettings(),
                BruteForce = _bruteForcePage.Value.ToSettings(),
                Auto = _autoPage.Value.ToSettings(),
                MOEAD = _moeadPage.Value.ToSettings(),
                MoCmaEs = _moCmaEsPage.Value.ToSettings(),
                DE = _dePage.Value.ToSettings()
            };

            int param1 = int.Parse(TrialNumberParam1, CultureInfo.InvariantCulture);
            bool result = int.TryParse(TrialNumberParam2, NumberStyles.Integer, CultureInfo.InvariantCulture, out int param2);
            if (!result)
            {
                param2 = 0;
            }
            SamplerType type = _settings.Optimize.SamplerType;
            int numOfTrials = type == SamplerType.NSGAII || type == SamplerType.NSGAIII || type == SamplerType.MOEAD
                ? param1 * param2
                : param1;
            sampler.NsgaII.PopulationSize = param2;
            sampler.NsgaIII.PopulationSize = param2;
            sampler.MOEAD.PopulationSize = param2;
            sampler.DE.PopulationSize = param2;

            var settings = new Core.Settings.Optimize
            {
                StudyName = string.IsNullOrWhiteSpace(StudyName) ? "AUTO" : StudyName,
                Sampler = sampler,
                NumberOfTrials = numOfTrials,
                ContinueStudy = IsContinue == true,
                CopyStudy = IsCopy == true,
                SamplerType = _settings.Optimize.SamplerType,
                Timeout = double.Parse(Timeout, CultureInfo.InvariantCulture),
                GcAfterTrial = GcAfterTrial.HasGeometry,
                ShowRealtimeResult = false,
                IgnoreDuplicateSampling = EnableIgnoreDuplicateSampling == true,
                DisableViewportDrawing = DisableViewportUpdate == true,
                MinimizeRhinoWindow = MinimizeRhinoWindow == true
            };

            if (computeAutoValue)
            {
                settings.ComputeAutoValue();
            }

            return settings;
        }

        internal void UpdateExistStudySummaries()
        {
            ExistingStudies = Utils.StudyNamesFromStudySummaries(SharedItems.StudySummaries);
        }
    }
}
