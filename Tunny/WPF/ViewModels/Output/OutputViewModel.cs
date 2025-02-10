using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Input;

using Optuna.Study;
using Optuna.Trial;

using Prism.Commands;
using Prism.Mvvm;

using Tunny.CommonUI;
using Tunny.CommonUI.Message;
using Tunny.Component.Optimizer;
using Tunny.Core.Handler;
using Tunny.Core.Util;
using Tunny.Process;
using Tunny.WPF.Common;
using Tunny.WPF.Models;
using Tunny.WPF.Views.Pages.Output;

namespace Tunny.WPF.ViewModels.Output
{
    internal sealed class OutputViewModel : BindableBase
    {
        public OutputViewModel()
        {
            StudySummary[] summaries = SharedItems.Instance.StudySummaries;
            StudyNameItems = Utils.StudyNamesFromStudySummaries(summaries);
            SelectedStudyName = StudyNameItems.FirstOrDefault();

            AnalysisChart = new AnalysisChartPage();
            AnalysisTable = new AnalysisTablePage();
        }

        private string _targetTrialNumber;
        public string TargetTrialNumber { get => _targetTrialNumber; set => SetProperty(ref _targetTrialNumber, value); }
        private ObservableCollection<NameComboBoxItem> _studyNameItems;
        public ObservableCollection<NameComboBoxItem> StudyNameItems { get => _studyNameItems; set => SetProperty(ref _studyNameItems, value); }
        private NameComboBoxItem _selectedStudyName;
        public NameComboBoxItem SelectedStudyName
        {
            get => _selectedStudyName;
            set
            {
                SetProperty(ref _selectedStudyName, value);
                if (StudyNameItems == null || StudyNameItems.Count == 0)
                {
                    return;
                }
                OutputListedItems = GetOutputViewListItem("Listed", value.Id);
                OutputTargetItems = GetOutputViewListItem("Target", value.Id);
                if (AnalysisTable != null)
                {
                    var tableViewModel = AnalysisTable.DataContext as AnalysisTableViewModel;
                    tableViewModel.SelectedStudyId = value.Id;
                }
                if (AnalysisChart != null)
                {
                    var chartViewModel = AnalysisChart.DataContext as AnalysisChartViewModel;
                    chartViewModel.SetStudyId(value.Id);
                }
            }
        }
        private ObservableCollection<OutputTrialItem> _outputListedItems;
        public ObservableCollection<OutputTrialItem> OutputListedItems
        {
            get => _outputListedItems ?? GetOutputViewListItem("Listed", SelectedStudyName?.Id ?? 0);
            set => SetProperty(ref _outputListedItems, value);
        }

        private ObservableCollection<OutputTrialItem> _outputTargetItems;
        public ObservableCollection<OutputTrialItem> OutputTargetItems
        {
            get => _outputTargetItems ?? GetOutputViewListItem("Target", SelectedStudyName?.Id ?? 0);
            set => SetProperty(ref _outputTargetItems, value);
        }

        private static ObservableCollection<OutputTrialItem> GetOutputViewListItem(string type, int id)
        {
            Dictionary<int, ObservableCollection<OutputTrialItem>> dict = type == "Target"
                ? SharedItems.Instance.OutputTargetTrialDict
                : SharedItems.Instance.OutputListedTrialDict;
            if (!dict.TryGetValue(id, out ObservableCollection<OutputTrialItem> item))
            {
                item = new ObservableCollection<OutputTrialItem>();
                dict.Add(id, item);
            }
            return item;
        }

        private DelegateCommand _addToListCommand;
        public ICommand AddToListCommand
        {
            get
            {
                if (_addToListCommand == null)
                {
                    _addToListCommand = new DelegateCommand(AddToList);
                }
                return _addToListCommand;
            }
        }
        private void AddToList()
        {
            if (string.IsNullOrEmpty(TargetTrialNumber))
            {
                return;
            }

            if (int.TryParse(TargetTrialNumber, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
            {
                AddOutputListedItems(result);
            }
        }

        private DelegateCommand<string> _loadSelectionCommand;
        public ICommand LoadSelectionCommand
        {
            get
            {
                if (_loadSelectionCommand == null)
                {
                    _loadSelectionCommand = new DelegateCommand<string>(LoadSelection);
                }
                return _loadSelectionCommand;
            }
        }
        private void LoadSelection(string param)
        {
            CsvType type = param == "Dashboard" ? CsvType.Dashboard : CsvType.DesignExplorer;
            LoadCsv(type);
        }

        private void LoadCsv(CsvType type)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                FileName = "",
                DefaultExt = "csv",
                Filter = @"selection csv(*.csv)|*.csv|All Files (*.*)|*.*",
                Title = @"Set Selection File Path",
            };

            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                var reader = new CsvReader(dialog.FileName);
                int[] indices = reader.ReadSelectionCsv(type);
                foreach (int index in indices)
                {
                    AddOutputListedItems(index);
                }
            }
        }

        private DelegateCommand _openOptunaDashboardTrialSelectionCommand;
        public ICommand OpenOptunaDashboardTrialSelectionCommand
        {
            get
            {
                if (_openOptunaDashboardTrialSelectionCommand == null)
                {
                    _openOptunaDashboardTrialSelectionCommand = new DelegateCommand(OpenOptunaDashboardTrialSelection);
                }
                return _openOptunaDashboardTrialSelectionCommand;
            }
        }
        private void OpenOptunaDashboardTrialSelection()
        {
            TLog.MethodStart();
            if (!File.Exists(SharedItems.Instance.Settings.Storage.Path))
            {
                TunnyMessageBox.Error_ResultFileNotExist();
                return;
            }
            string dashboardPath = Path.Combine(TEnvVariables.TunnyEnvPath, "python", "Scripts", "optuna-dashboard.exe");
            string storagePath = SharedItems.Instance.Settings.Storage.Path;

            var dashboard = new Optuna.Dashboard.Handler(dashboardPath, storagePath);
            string path = Path.Combine("dashboard", "studies", SelectedStudyName.Id.ToString(CultureInfo.InvariantCulture), "trialCompare");
            dashboard.Run(true, path);
        }

        private DelegateCommand _reinstateCommand;
        public ICommand ReinstateCommand
        {
            get
            {
                if (_reinstateCommand == null)
                {
                    _reinstateCommand = new DelegateCommand(Reinstate);
                }
                return _reinstateCommand;
            }
        }
        private void Reinstate()
        {
            TLog.MethodStart();
            OutputProcess.StudyName = SelectedStudyName.Name;
            OutputProcess.Indices = new[] { int.Parse(TargetTrialNumber, NumberStyles.Integer, CultureInfo.InvariantCulture) };
            OutputProcess.Run();

            OptimizeComponentBase component = CommonSharedItems.Instance.Component;
            component.GhInOut.NewSolution(component.Fishes[0].GetParameterClassFormatVariables());
        }

        private DelegateCommand<string> _removeFromListCommand;
        public ICommand RemoveFromListCommand
        {
            get
            {
                if (_removeFromListCommand == null)
                {
                    _removeFromListCommand = new DelegateCommand<string>(RemoveFromList);
                }
                return _removeFromListCommand;
            }
        }
        private void RemoveFromList(string target)
        {
            if (target == "ListedTrials")
            {
                var removedList = OutputListedItems.Where(i => i.IsSelected == true).ToList();
                foreach (OutputTrialItem item in removedList)
                {
                    OutputListedItems.Remove(item);
                }
            }
            else if (target == "OutputTargetTrials")
            {
                var removedList = OutputTargetItems.Where(i => i.IsSelected == true).ToList();
                foreach (OutputTrialItem item in removedList)
                {
                    if (item.IsSelected)
                    {
                        item.IsSelected = false;
                        OutputListedItems.Add(item);
                        OutputTargetItems.Remove(item);
                    }
                }
            }
        }

        private DelegateCommand<string> _clearListCommand;
        public ICommand ClearListCommand
        {
            get
            {
                if (_clearListCommand == null)
                {
                    _clearListCommand = new DelegateCommand<string>(ClearList);
                }
                return _clearListCommand;
            }
        }
        private void ClearList(string target)
        {
            if (target == "ListedTrials")
            {
                OutputListedItems.Clear();
            }
            else if (target == "OutputTargetTrials")
            {
                foreach (OutputTrialItem item in OutputTargetItems)
                {
                    item.IsSelected = false;
                    OutputListedItems.Add(item);
                }
                OutputTargetItems.Clear();
            }
        }

        private DelegateCommand _addOutputTargetListCommand;
        public ICommand AddOutputTargetListCommand
        {
            get
            {
                if (_addOutputTargetListCommand == null)
                {
                    _addOutputTargetListCommand = new DelegateCommand(AddOutputTargetList);
                }
                return _addOutputTargetListCommand;
            }
        }
        private void AddOutputTargetList()
        {
            var selected = OutputListedItems.Where(x => x.IsSelected).ToList();
            foreach (OutputTrialItem item in selected)
            {
                OutputListedItems.Remove(item);
                OutputTargetItems.Add(item);
            }
        }

        private DelegateCommand _addAllCommand;
        public ICommand AddAllCommand
        {
            get
            {
                if (_addAllCommand == null)
                {
                    _addAllCommand = new DelegateCommand(AddAll);
                }
                return _addAllCommand;
            }
        }
        private void AddAll()
        {
            IEnumerable<int> trialIds = SharedItems.Instance.Trials[SelectedStudyName.Id].Select(x => x.Number);
            foreach (int trialId in trialIds)
            {
                AddOutputListedItems(trialId);
            }
        }

        private DelegateCommand _addParetoFrontCommand;
        public ICommand AddParetoFrontCommand
        {
            get
            {
                if (_addParetoFrontCommand == null)
                {
                    _addParetoFrontCommand = new DelegateCommand(AddParetoFront);
                }
                return _addParetoFrontCommand;
            }
        }
        private void AddParetoFront()
        {
            Trial[] trials = SharedItems.Instance.Trials[SelectedStudyName.Id];
            Trial[] feasibleTrials = GetFeasibleTrials(trials);
            StudyDirection[] directions = SharedItems.Instance.StudySummaries.FirstOrDefault(x => x.StudyId == SelectedStudyName.Id).Directions;
            if (directions.Length == 1)
            {
                IOrderedEnumerable<Trial> orderedTrials = feasibleTrials.OrderBy(x => x.Values[0]);
                int trialId = directions[0] == StudyDirection.Maximize
                    ? orderedTrials.Last().Number
                    : orderedTrials.First().Number;
                AddOutputListedItems(trialId);
            }
            else
            {
                Trial[] pareto = MultiObjective.GetParetoFrontTrials(feasibleTrials.ToList(), directions);

                IEnumerable<int> trialIds = pareto.Select(x => x.Number);
                foreach (int trialId in trialIds)
                {
                    AddOutputListedItems(trialId);
                }
            }
        }

        private DelegateCommand _addFeasibleCommand;
        public ICommand AddFeasibleCommand
        {
            get
            {
                if (_addFeasibleCommand == null)
                {
                    _addFeasibleCommand = new DelegateCommand(AddFeasible);
                }
                return _addFeasibleCommand;
            }
        }
        private void AddFeasible()
        {
            Trial[] trials = SharedItems.Instance.Trials[SelectedStudyName.Id];
            Trial[] feasibleTrials = GetFeasibleTrials(trials);

            IEnumerable<int> trialIds = feasibleTrials.Select(x => x.Number);
            foreach (int trialId in trialIds)
            {
                AddOutputListedItems(trialId);
            }
        }

        private static Trial[] GetFeasibleTrials(Trial[] trials)
        {
            var feasibleTrials = new List<Trial>();
            foreach (Trial trial in trials)
            {
                trial.SystemAttrs.TryGetValue("constraints", out object constraints);
                if (constraints is string[] c)
                {
                    if (c.All(x => double.Parse(x, NumberStyles.Float, CultureInfo.InvariantCulture) <= 0))
                    {
                        feasibleTrials.Add(trial);
                    }
                }
                else if (constraints == null)
                {
                    feasibleTrials.Add(trial);
                }
            }
            return feasibleTrials.ToArray();
        }

        private void AddOutputListedItems(int trialId)
        {
            if (!OutputListedItems.Any(x => x.Id == trialId))
            {
                OutputTrialItem outputTrialItem = SharedItems.Instance.GetOutputTrial(SelectedStudyName.Id, trialId);
                OutputListedItems.Add(outputTrialItem);
            }
        }

        private DelegateCommand _openDesignExplorerSelectionCommand;
        public ICommand OpenDesignExplorerSelectionCommand
        {
            get
            {
                if (_openDesignExplorerSelectionCommand == null)
                {
                    _openDesignExplorerSelectionCommand = new DelegateCommand(OpenDesignExplorerSelection);
                }
                return _openDesignExplorerSelectionCommand;
            }
        }
        private void OpenDesignExplorerSelection()
        {
            var designExplorer = new DesignExplorer(SelectedStudyName.Name, SharedItems.Instance.Settings.Storage);
            designExplorer.Run();
        }

        private DelegateCommand<string> _checkAllCommand;
        public ICommand CheckAllCommand
        {
            get
            {
                if (_checkAllCommand == null)
                {
                    _checkAllCommand = new DelegateCommand<string>(CheckAll);
                }
                return _checkAllCommand;
            }
        }
        private void CheckAll(string target)
        {
            if (target == "ListedTrials")
            {
                foreach (OutputTrialItem item in OutputListedItems)
                {
                    item.IsSelected = IsListedAllChecked ?? false;
                }
            }
            else if (target == "OutputTargetTrials")
            {
                foreach (OutputTrialItem item in OutputTargetItems)
                {
                    item.IsSelected = IsTargetsAllChecked ?? false;
                }
            }
        }

        private bool? _isTargetsAllChecked;
        public bool? IsTargetsAllChecked { get => _isTargetsAllChecked; set => SetProperty(ref _isTargetsAllChecked, value); }
        private bool? _isListedAllChecked;
        public bool? IsListedAllChecked { get => _isListedAllChecked; set => SetProperty(ref _isListedAllChecked, value); }

        private DelegateCommand _outputSelectionCommand;
        public ICommand OutputSelectionCommand
        {
            get
            {
                if (_outputSelectionCommand == null)
                {
                    _outputSelectionCommand = new DelegateCommand(OutputSelection);
                }
                return _outputSelectionCommand;
            }
        }
        private void OutputSelection()
        {
            TLog.MethodStart();
            OutputProcess.StudyName = SelectedStudyName.Name;
            OutputProcess.Indices = OutputTargetItems.Where(t => t.IsSelected).Select(t => t.Id).ToArray();
            OutputProcess.Run();
        }

        private AnalysisTablePage _analysisTable;
        public AnalysisTablePage AnalysisTable { get => _analysisTable; set => SetProperty(ref _analysisTable, value); }
        private AnalysisChartPage _analysisChart;
        public AnalysisChartPage AnalysisChart { get => _analysisChart; set => SetProperty(ref _analysisChart, value); }
    }
}
