using System.Linq;

using Tunny.Core.Settings;
using Tunny.Core.Util;

namespace Tunny.WPF.ViewModels.Visualize
{
    [LoggingAspect]
    internal sealed class ParetoFrontViewModel : PlotSettingsViewModelBase
    {
        private bool? _includeDominatedTrials;
        public bool? IncludeDominatedTrials { get => _includeDominatedTrials; set => SetProperty(ref _includeDominatedTrials, value); }

        public ParetoFrontViewModel() : base()
        {
            IncludeDominatedTrials = true;
        }

        public override bool TryGetPlotSettings(out PlotSettings plotSettings)
        {
            if (StudyNameItems.Count == 0 || SelectedStudyName == null ||
                ObjectiveItems.Count == 0 || ObjectiveItems.Where(o => o.IsSelected).Count() < 2 || ObjectiveItems.Where(o => o.IsSelected).Count() > 3)
            {
                plotSettings = null;
                return false;
            }

            plotSettings = new PlotSettings
            {
                TargetStudyName = SelectedStudyName.Name,
                PlotTypeName = "pareto front",
                IncludeDominatedTrials = IncludeDominatedTrials.Value,
                TargetObjectiveName = ObjectiveItems.Where(o => o.IsSelected).Select(o => o.Name).ToArray(),
                TargetObjectiveIndex = ObjectiveItems.Where(o => o.IsSelected).Select(o => ObjectiveItems.IndexOf(o)).ToArray()
            };
            return true;
        }
    }
}
