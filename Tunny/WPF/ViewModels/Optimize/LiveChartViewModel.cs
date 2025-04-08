using System.Collections.ObjectModel;

using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;

using Prism.Mvvm;

using SkiaSharp;

using Tunny.Core.Util;

namespace Tunny.WPF.ViewModels.Optimize
{
    [LoggingAspect]
    public class LiveChartViewModel : BindableBase
    {

        private string _selectedXTarget;
        public string SelectedXTarget
        {
            get => _selectedXTarget;
            set
            {
                SetProperty(ref _selectedXTarget, value);
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
        private string _selectedYTarget;
        public string SelectedYTarget
        {
            get => _selectedYTarget;
            set
            {
                SetProperty(ref _selectedYTarget, value);
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
        private ObservableCollection<string> _xTarget;
        public ObservableCollection<string> XTarget { get => _xTarget; set => SetProperty(ref _xTarget, value); }

        private ObservableCollection<string> _yTarget;
        public ObservableCollection<string> YTarget { get => _yTarget; set => SetProperty(ref _yTarget, value); }

        private ObservableCollection<ObservablePoint> _chartPoints;
        public ObservableCollection<ObservablePoint> ChartPoints { get => _chartPoints; set => SetProperty(ref _chartPoints, value); }

        private ObservableCollection<ISeries> _chartSeries;
        public ObservableCollection<ISeries> ChartSeries { get => _chartSeries; set => SetProperty(ref _chartSeries, value); }

        private ObservableCollection<ICartesianAxis> _chartXAxes;
        public ObservableCollection<ICartesianAxis> ChartXAxes { get => _chartXAxes; set => SetProperty(ref _chartXAxes, value); }

        private ObservableCollection<ICartesianAxis> _chartYAxes;
        public ObservableCollection<ICartesianAxis> ChartYAxes { get => _chartYAxes; set => SetProperty(ref _chartYAxes, value); }

        private bool _chartEnable;
        public bool ChartEnable { get => _chartEnable; set => SetProperty(ref _chartEnable, value); }

        public LiveChartViewModel()
        {
            ChartEnable = true;
            _chartPoints = new ObservableCollection<ObservablePoint>();
            ChartSeries = new ObservableCollection<ISeries>
            {
                new ScatterSeries<ObservablePolarPoint>
                {
                    Values = _chartPoints,
                    GeometrySize =5
                }
            };

            ChartXAxes = new ObservableCollection<ICartesianAxis>();
            ChartYAxes = new ObservableCollection<ICartesianAxis>();
        }
    }
}
