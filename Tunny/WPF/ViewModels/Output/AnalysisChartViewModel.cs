using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;

using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;

using Optuna.Study;
using Optuna.Trial;

using Prism.Commands;
using Prism.Mvvm;

using SkiaSharp;

using Tunny.Core.Util;
using Tunny.WPF.Common;

namespace Tunny.WPF.ViewModels.Output
{
    [LoggingAspect]
    internal sealed class AnalysisChartViewModel : BindableBase
    {
        private int _selectedStudyId;
        private string[] _metricNames;

        private string[] _targetItems;
        public string[] TargetItems { get => _targetItems; set => SetProperty(ref _targetItems, value); }
        private string _selectedTarget;
        public string SelectedTarget { get => _selectedTarget; set => SetProperty(ref _selectedTarget, value); }
        private ObservableCollection<string> _xAxisItems;
        public ObservableCollection<string> XAxisItems { get => _xAxisItems; set => SetProperty(ref _xAxisItems, value); }
        private string _selectedXAxis;
        public string SelectedXAxis
        {
            get => _selectedXAxis;
            set
            {
                SetProperty(ref _selectedXAxis, value);
                ChartXAxes = new ObservableCollection<ICartesianAxis>
                {
                    new Axis
                    {
                        Name = value,
                        NameTextSize = 12,
                        NamePaint = new SolidColorPaint(SKColors.Black),
                        LabelsPaint = new SolidColorPaint(SKColors.Black),
                        TextSize = 12,
                        SeparatorsPaint = new SolidColorPaint
                        {
                            Color = SKColors.Black.WithAlpha(100),
                            StrokeThickness = 1,
                        },
                        SubseparatorsPaint = new SolidColorPaint
                        {
                            Color = SKColors.Black.WithAlpha(50),
                            StrokeThickness = 0.5f
                        },
                    }
                };
            }
        }
        private ObservableCollection<string> _yAxisItems;
        public ObservableCollection<string> YAxisItems { get => _yAxisItems; set => SetProperty(ref _yAxisItems, value); }
        private string _selectedYAxis;
        public string SelectedYAxis
        {
            get => _selectedYAxis;
            set
            {
                SetProperty(ref _selectedYAxis, value);
                ChartYAxes = new ObservableCollection<ICartesianAxis>
                {
                    new Axis
                    {
                        Name = value,
                        NameTextSize = 12,
                        NamePaint = new SolidColorPaint(SKColors.Black),
                        LabelsPaint = new SolidColorPaint(SKColors.Black),
                        TextSize = 12,
                        SeparatorsPaint = new SolidColorPaint
                        {
                            Color = SKColors.Black.WithAlpha(100),
                            StrokeThickness = 1,
                        },
                        SubseparatorsPaint = new SolidColorPaint
                        {
                            Color = SKColors.Black.WithAlpha(50),
                            StrokeThickness = 0.5f
                        },
                    }
                };
            }
        }
        private object _outputChart;
        public object OutputChart { get => _outputChart; set => SetProperty(ref _outputChart, value); }

        internal AnalysisChartViewModel()
        {
            TargetItems = new string[] { "Listed Trials", "Target Trials" };
            SelectedTarget = TargetItems[0];
            XAxisItems = new ObservableCollection<string>();
            YAxisItems = new ObservableCollection<string>();
            SetStudyId(0);

            _chartPoints = new ObservableCollection<ObservablePoint>();
            ChartSeries = new ObservableCollection<ISeries>
            {
                new ScatterSeries<ObservablePoint>
                {
                    Values = _chartPoints,
                    GeometrySize = 5
                }
            };
            SelectedXAxis = XAxisItems[0];
            SelectedYAxis = YAxisItems.Count > 2 ? YAxisItems[1] : YAxisItems[0];
        }

        internal void SetStudyId(int studyId)
        {
            _selectedStudyId = studyId;
            XAxisItems.Clear();
            YAxisItems.Clear();
            XAxisItems.Add("ID");
            YAxisItems.Add("ID");

            if (SharedItems.Instance.StudySummaries == null || SharedItems.Instance.StudySummaries.Length == 0)
            {
                return;
            }

            StudySummary study = SharedItems.Instance.StudySummaries[_selectedStudyId];
            study.SystemAttrs.TryGetValue("study:metric_names", out object metricNameObjs);
            _metricNames = metricNameObjs as string[] ?? Array.Empty<string>();
            if (_metricNames.Length == 0)
            {
                _metricNames = study.Directions.Select((_, i) => $"Objective{i}").ToArray();
            }
            foreach (string metricName in _metricNames)
            {
                XAxisItems.Add(metricName);
                YAxisItems.Add(metricName);
            }

            Trial[] trials = SharedItems.Instance.Trials[_selectedStudyId];
            Dictionary<string, object>.KeyCollection valueKeys = trials[0].Params.Keys;
            foreach (string k in valueKeys)
            {
                string item = $"Param: {k}";
                XAxisItems.Add(item);
                YAxisItems.Add(item);
            }
            Dictionary<string, object>.KeyCollection attrKeys = trials[0].UserAttrs.Keys;
            foreach (string k in attrKeys)
            {
                object value = trials[0].UserAttrs[k];
                if (value is double[] values)
                {
                    for (int i = 0; i < values.Length; i++)
                    {
                        string item = $"Attr: {k}_{i}";
                        XAxisItems.Add(item);
                        YAxisItems.Add(item);
                    }
                }
                else if (value is string[] strings)
                {
                    for (int i = 0; i < strings.Length; i++)
                    {
                        string item = $"Attr: {k}_{i}";
                        XAxisItems.Add(item);
                        YAxisItems.Add(item);
                    }
                }
                else
                {
                    string item = $"Attr: {k}";
                    XAxisItems.Add(item);
                    YAxisItems.Add(item);
                }
            }
            SelectedXAxis = XAxisItems[0];
            SelectedYAxis = YAxisItems[1];
        }

        private DelegateCommand _drawChartCommand;
        public ICommand DrawChartCommand
        {
            get
            {
                if (_drawChartCommand == null)
                {
                    _drawChartCommand = new DelegateCommand(DrawChart);
                }
                return _drawChartCommand;
            }
        }
        private void DrawChart()
        {
            _chartPoints.Clear();
            Trial[] trials = SharedItems.Instance.Trials[_selectedStudyId];
            IEnumerable<int> targetIds = SelectedTarget == "Listed Trials"
                ? SharedItems.Instance.OutputListedTrialDict[_selectedStudyId].Select(t => t.Id)
                : SharedItems.Instance.OutputTargetTrialDict[_selectedStudyId].Select(t => t.Id);
            foreach (Trial trial in trials)
            {
                if (!targetIds.Contains(trial.TrialId))
                {
                    continue;
                }
                double x = GetTargetValue(trial, SelectedXAxis);
                double y = GetTargetValue(trial, SelectedYAxis);
                _chartPoints.Add(new ObservablePoint(x, y));
            }
        }

        private double GetTargetValue(Trial trial, string target)
        {
            if (target == "ID")
            {
                return trial.TrialId;
            }
            else if (target.StartsWith("Param: ", StringComparison.InvariantCulture))
            {
                string key = target.Substring(7);
                return (double)trial.Params[key];
            }
            else if (target.StartsWith("Attr: ", StringComparison.InvariantCulture))
            {
                Dictionary<string, object>.KeyCollection attrKeys = trial.UserAttrs.Keys;
                foreach (string k in attrKeys)
                {
                    object value = trial.UserAttrs[k];
                    if (value is double[] values)
                    {
                        for (int i = 0; i < values.Length; i++)
                        {
                            if (target == $"Attr: {k}_{i}")
                            {
                                return values[i];
                            }
                        }
                    }
                    else if (value is string[] strings)
                    {
                        for (int i = 0; i < strings.Length; i++)
                        {
                            if (target == $"Attr: {k}_{i}")
                            {
                                if (double.TryParse(strings[i], NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
                                {
                                    return result;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (target == $"Attr: {k}" && double.TryParse(value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
                        {
                            return result;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < _metricNames.Length; i++)
                {
                    string metricName = _metricNames[i];
                    if (target == metricName)
                    {
                        return trial.Values[i];
                    }
                }
            }
            throw new ArgumentException("Invalid target.");
        }

        private ObservableCollection<ObservablePoint> _chartPoints;
        public ObservableCollection<ObservablePoint> ChartPoints { get => _chartPoints; set => SetProperty(ref _chartPoints, value); }
        private ObservableCollection<ISeries> _chartSeries;
        public ObservableCollection<ISeries> ChartSeries { get => _chartSeries; set => SetProperty(ref _chartSeries, value); }
        private ObservableCollection<ICartesianAxis> _chartXAxes;
        public ObservableCollection<ICartesianAxis> ChartXAxes { get => _chartXAxes; set => SetProperty(ref _chartXAxes, value); }
        private ObservableCollection<ICartesianAxis> _chartYAxes;
        public ObservableCollection<ICartesianAxis> ChartYAxes { get => _chartYAxes; set => SetProperty(ref _chartYAxes, value); }
    }
}
