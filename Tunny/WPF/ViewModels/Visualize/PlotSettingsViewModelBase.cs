using System;
using System.Collections.ObjectModel;
using System.Linq;

using Optuna.Study;

using Prism.Mvvm;

using Tunny.Core.Settings;
using Tunny.Core.Util;
using Tunny.Eto.Message;
using Tunny.Eto.Models;
using Tunny.WPF.Common;
using Tunny.WPF.Models;

namespace Tunny.WPF.ViewModels.Visualize
{
    [LoggingAspect]
    internal abstract class PlotSettingsViewModelBase : BindableBase, IPlotSettings
    {
        public abstract bool TryGetPlotSettings(out PlotSettings plotSettings);

        private StudySummary[] _summaries;
        private ObservableCollection<NameComboBoxItem> _studyNameItems;
        public ObservableCollection<NameComboBoxItem> StudyNameItems { get => _studyNameItems; set => SetProperty(ref _studyNameItems, value); }
        private NameComboBoxItem _selectedStudyName;
        public NameComboBoxItem SelectedStudyName
        {
            get => _selectedStudyName;
            set
            {
                SetProperty(ref _selectedStudyName, value);
                UpdateObjectivesAndVariables();
            }
        }
        private ObservableCollection<VisualizeListItem> _objectiveItems;
        public ObservableCollection<VisualizeListItem> ObjectiveItems { get => _objectiveItems; set => SetProperty(ref _objectiveItems, value); }
        private VisualizeListItem _selectedObjective;
        public VisualizeListItem SelectedObjective { get => _selectedObjective; set => SetProperty(ref _selectedObjective, value); }
        private ObservableCollection<VisualizeListItem> _variableItems;
        public ObservableCollection<VisualizeListItem> VariableItems { get => _variableItems; set => SetProperty(ref _variableItems, value); }
        private VisualizeListItem _selectedVariable;
        public VisualizeListItem SelectedVariable { get => _selectedVariable; set => SetProperty(ref _selectedVariable, value); }

        public PlotSettingsViewModelBase()
        {
            _summaries = SharedItems.Instance.StudySummaries;
            ObjectiveItems = new ObservableCollection<VisualizeListItem>();
            VariableItems = new ObservableCollection<VisualizeListItem>();
            StudyNameItems = Eto.Common.Utils.StudyNamesFromStudySummaries(_summaries);
            SelectedStudyName = StudyNameItems.FirstOrDefault();
        }

        public void UpdateItems()
        {
            _summaries = SharedItems.Instance.StudySummaries;
            StudyNameItems = Eto.Common.Utils.StudyNamesFromStudySummaries(_summaries);
            SelectedStudyName = StudyNameItems.FirstOrDefault();
        }

        private void UpdateObjectivesAndVariables()
        {
            if (_summaries == null || _summaries.Length == 0)
            {
                return;
            }

            if (SelectedStudyName == null && StudyNameItems.Count > 0)
            {
                SelectedStudyName = StudyNameItems.FirstOrDefault();
                if (SelectedStudyName == null)
                {
                    TunnyMessageBox.Error_NoStudyNameSelected();
                    return;
                }
            }
            StudySummary targetStudySummary = _summaries.FirstOrDefault(s => s.StudyName == SelectedStudyName.Name);
            if (targetStudySummary != null)
            {
                SetVariableListItems(targetStudySummary);
                SetObjectiveListItems(targetStudySummary);
            }
        }

        private void SetObjectiveListItems(StudySummary visualizeStudySummary)
        {
            ObjectiveItems.Clear();

            string versionString = (visualizeStudySummary.UserAttrs["tunny_version"] as string[])[0];
            var version = new Version(versionString);
            string[] metricNames = version <= TEnvVariables.OldStorageVersion
                ? visualizeStudySummary.UserAttrs["objective_names"] as string[]
                : visualizeStudySummary.SystemAttrs["study:metric_names"] as string[];
            for (int i = 0; i < metricNames.Length; i++)
            {
                ObjectiveItems.Add(new VisualizeListItem()
                {
                    Name = metricNames[i]
                });
            }
            SelectedObjective = ObjectiveItems.FirstOrDefault();
        }

        private void SetVariableListItems(StudySummary visualizeStudySummary)
        {
            VariableItems.Clear();

            string[] variableNames = visualizeStudySummary.UserAttrs["variable_names"] as string[];
            for (int i = 0; i < variableNames.Length; i++)
            {
                VariableItems.Add(new VisualizeListItem()
                {
                    Name = variableNames[i]
                });
            }
            SelectedVariable = VariableItems.FirstOrDefault();
        }
    }
}
