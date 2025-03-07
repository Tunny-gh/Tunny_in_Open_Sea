using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Input;

using Optuna.Study;
using Optuna.Trial;

using Prism.Commands;
using Prism.Mvvm;

using Tunny.WPF.Common;

namespace Tunny.WPF.ViewModels.Output
{
    internal sealed class AnalysisTableViewModel : BindableBase
    {
        internal int SelectedStudyId { get; set; }

        private string[] _targetItems;
        public string[] TargetItems { get => _targetItems; set => SetProperty(ref _targetItems, value); }
        private string _selectedTarget;
        public string SelectedTarget { get => _selectedTarget; set => SetProperty(ref _selectedTarget, value); }
        private DataView _trialGridView;
        public DataView TrialDataView { get => _trialGridView; set => SetProperty(ref _trialGridView, value); }

        public AnalysisTableViewModel()
        {
            TargetItems = new string[] { "Listed Trials", "Target Trials" };
            SelectedTarget = TargetItems[0];
        }

        private DelegateCommand _drawTableCommand;
        public ICommand DrawTableCommand
        {
            get
            {
                if (_drawTableCommand == null)
                {
                    _drawTableCommand = new DelegateCommand(DrawTable);
                }
                return _drawTableCommand;
            }
        }
        private void DrawTable()
        {
            var table = new DataTable();
            table.Columns.Add("ID", typeof(int));

            StudySummary study = SharedItems.Instance.StudySummaries[SelectedStudyId];
            study.SystemAttrs.TryGetValue("study:metric_names", out object metricNameObjs);
            string[] metricNames = metricNameObjs as string[] ?? Array.Empty<string>();
            if (metricNames.Length == 0)
            {
                metricNames = study.Directions.Select((_, i) => $"Objective{i}").ToArray();
            }
            foreach (string metricName in metricNames)
            {
                table.Columns.Add(metricName, typeof(double));
            }

            Trial[] trials = SharedItems.Instance.Trials[SelectedStudyId];
            Dictionary<string, object>.KeyCollection valueKeys = trials[0].Params.Keys;
            foreach (string k in valueKeys)
            {
                table.Columns.Add($"Param: {k}", typeof(double));
            }
            Dictionary<string, object>.KeyCollection attrKeys = trials[0].UserAttrs.Keys;
            foreach (string k in attrKeys)
            {
                object value = trials[0].UserAttrs[k];
                if (value is double[] values)
                {
                    for (int i = 0; i < values.Length; i++)
                    {
                        table.Columns.Add($"Attr: {k}___{i}", typeof(double));
                    }
                }
                else if (value is string[] strings)
                {
                    for (int i = 0; i < strings.Length; i++)
                    {
                        table.Columns.Add($"Attr: {k}___{i}", typeof(string));
                    }
                }
                else
                {
                    table.Columns.Add($"Attr: {k}", typeof(object));
                }
            }

            IEnumerable<int> targetIds = SelectedTarget == TargetItems[0]
                ? SharedItems.Instance.OutputListedTrialDict[SelectedStudyId].Select(t => t.Id)
                : SharedItems.Instance.OutputTargetTrialDict[SelectedStudyId].Select(t => t.Id);
            foreach (Trial trial in trials)
            {
                if (!targetIds.Contains(trial.Number))
                {
                    continue;
                }
                DataRow row = table.NewRow();
                row["ID"] = trial.Number;
                for (int i = 0; i < metricNames.Length; i++)
                {
                    string metricName = metricNames[i];
                    row[metricName] = trial.Values[i];
                }
                foreach (string k in valueKeys)
                {
                    row[$"Param: {k}"] = (double)trial.Params[k];
                }
                foreach (string k in attrKeys)
                {
                    object value = trial.UserAttrs[k];
                    if (value is double[] values)
                    {
                        for (int i = 0; i < values.Length; i++)
                        {
                            row[$"Attr: {k}___{i}"] = values[i];
                        }
                    }
                    else if (value is string[] strings)
                    {
                        for (int i = 0; i < strings.Length; i++)
                        {
                            row[$"Attr: {k}___{i}"] = strings[i];
                        }
                    }
                    else
                    {
                        row[$"Attr: {k}"] = value;
                    }
                }
                table.Rows.Add(row);
            }
            TrialDataView = new DataView(table);
        }
    }
}
