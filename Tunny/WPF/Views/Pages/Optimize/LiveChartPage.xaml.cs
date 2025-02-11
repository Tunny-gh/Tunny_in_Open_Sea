using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

using LiveChartsCore.Defaults;

using Tunny.Component.Optimizer;
using Tunny.Eto.Common;
using Tunny.WPF.Common;
using Tunny.WPF.ViewModels.Optimize;

namespace Tunny.WPF.Views.Pages.Optimize
{
    public partial class LiveChartPage : Page
    {
        private readonly LiveChartViewModel _viewModel;

        public LiveChartPage()
        {
            InitializeComponent();
            _viewModel = new LiveChartViewModel();
            DataContext = _viewModel;
            Loaded += (_, e) => SetTargetComboBoxItems(1, e);
        }

        public LiveChartPage(int yIndex)
        {
            InitializeComponent();
            _viewModel = new LiveChartViewModel();
            DataContext = _viewModel;

            Loaded += (_, e) => SetTargetComboBoxItems(yIndex, e);
        }

        private void SetTargetComboBoxItems(object sender, RoutedEventArgs e)
        {
            int yIndex = (int)sender;
            OptimizeComponentBase component = CommonSharedItems.Instance.Component;
            string[] metricNames = component.GhInOut.Objectives.GetNickNames();
            _viewModel.XTarget = new ObservableCollection<string> { "Trial Number" };
            _viewModel.YTarget = new ObservableCollection<string> { "Trial Number" };
            foreach (string s in metricNames)
            {
                _viewModel.XTarget.Add(s);
                _viewModel.YTarget.Add(s);
            }
            ChartXTargetComboBox.SelectedIndex = 0;
            ChartYTargetComboBox.SelectedIndex = ChartYTargetComboBox.Items.Count > yIndex ? yIndex : 1;
        }

        public void AddPoint(int trialNumber, double[] objectives)
        {
            if (_viewModel.ChartEnable)
            {
                double? x = GetChartValue(trialNumber, objectives, ChartXTargetComboBox.SelectedIndex);
                double? y = GetChartValue(trialNumber, objectives, ChartYTargetComboBox.SelectedIndex);
                if (x == null || y == null)
                {
                    return;
                }
                _viewModel.ChartPoints.Add(new ObservablePoint(x, y));
            }
        }

        private static double? GetChartValue(int trialNumber, double[] objectives, int index)
        {
            double? value = index == 0
                ? trialNumber
                : index <= objectives.Length ? objectives[index - 1] : (double?)null;
            return value;
        }

        internal void ClearPoints()
        {
            _viewModel.ChartPoints.Clear();
        }
    }
}
