using System.Linq;

using Tunny.Core.Settings;
using Tunny.Core.Util;

namespace Tunny.WPF.ViewModels.Visualize
{
    [LoggingAspect]
    internal sealed class SliceViewModel : PlotSettingsViewModelBase
    {
        public SliceViewModel() : base()
        {
        }

        public override bool TryGetPlotSettings(out PlotSettings plotSettings)
        {
            if (StudyNameItems.Count == 0 || SelectedStudyName == null ||
                ObjectiveItems.Count == 0 ||
                VariableItems.Count == 0 || !VariableItems.Any(v => v.IsSelected))
            {
                plotSettings = null;
                return false;
            }

            plotSettings = new PlotSettings
            {
                TargetStudyName = SelectedStudyName.Name,
                PlotTypeName = "slice",
                TargetObjectiveName = new string[] { SelectedObjective.Name },
                TargetObjectiveIndex = new int[] { ObjectiveItems.IndexOf(SelectedObjective) },
                TargetVariableName = VariableItems.Where(v => v.IsSelected).Select(v => v.Name).ToArray(),
            };
            return true;
        }
    }
}
