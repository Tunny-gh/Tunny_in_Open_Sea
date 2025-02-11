using System.Collections.ObjectModel;

using Tunny.Core.Settings;
using Tunny.Eto.Models;

namespace Tunny.WPF.ViewModels.Visualize
{
    internal sealed class OptimizationHistoryViewModel : PlotSettingsViewModelBase
    {
        private ObservableCollection<NameComboBoxItem> _compareStudyNameItems;
        public ObservableCollection<NameComboBoxItem> CompareStudyNameItems { get => _compareStudyNameItems; set => SetProperty(ref _compareStudyNameItems, value); }
        private bool? _showErrorBar;
        public bool? ShowErrorBar { get => _showErrorBar; set => SetProperty(ref _showErrorBar, value); }

        public OptimizationHistoryViewModel() : base()
        {
            ShowErrorBar = false;
        }

        public override bool TryGetPlotSettings(out PlotSettings plotSettings)
        {
            if (StudyNameItems.Count == 0 || SelectedStudyName == null ||
                ObjectiveItems.Count == 0 || SelectedObjective == null)
            {
                plotSettings = null;
                return false;
            }

            plotSettings = new PlotSettings
            {
                TargetStudyName = SelectedStudyName.Name,
                PlotTypeName = "optimization history",
                TargetObjectiveName = new string[] { SelectedObjective.Name },
                TargetObjectiveIndex = new int[] { ObjectiveItems.IndexOf(SelectedObjective) },
            };
            return true;
        }
    }
}
