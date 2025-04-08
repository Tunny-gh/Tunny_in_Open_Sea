using System.Collections.ObjectModel;
using System.Linq;

using Tunny.Core.Settings;
using Tunny.Core.Util;

namespace Tunny.WPF.ViewModels.Visualize
{
    [LoggingAspect]
    internal sealed class ParamImportancesViewModel : PlotSettingsViewModelBase
    {
        private ObservableCollection<string> _evaluatorItems;
        public ObservableCollection<string> EvaluatorItems { get => _evaluatorItems; set => SetProperty(ref _evaluatorItems, value); }
        private string _selectedEvaluator;
        public string SelectedEvaluator { get => _selectedEvaluator; set => SetProperty(ref _selectedEvaluator, value); }

        public ParamImportancesViewModel() : base()
        {
            EvaluatorItems = new ObservableCollection<string>()
            {
                "AUTO",
                "fAnova",
                "PedAnova",
                //"SHAP",
                //"MeanDecreaseImpurity"
            };
            SelectedEvaluator = EvaluatorItems[0];
        }

        public override bool TryGetPlotSettings(out PlotSettings plotSettings)
        {
            if (StudyNameItems.Count == 0 || SelectedStudyName == null ||
                VariableItems.Count == 0 || !VariableItems.Any(v => v.IsSelected))
            {
                plotSettings = null;
                return false;
            }

            plotSettings = new PlotSettings
            {
                TargetStudyName = SelectedStudyName.Name,
                PlotTypeName = "param importances",
                TargetObjectiveName = new string[] { SelectedObjective.Name },
                TargetObjectiveIndex = new int[] { ObjectiveItems.IndexOf(SelectedObjective) },
                TargetVariableName = VariableItems.Where(v => v.IsSelected).Select(v => v.Name).ToArray(),
                ImportanceEvaluator = SelectedEvaluator
            };
            return true;
        }
    }
}
