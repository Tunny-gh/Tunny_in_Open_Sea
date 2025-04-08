using Tunny.Core.Settings;
using Tunny.Core.Util;

namespace Tunny.WPF.ViewModels.Visualize
{
    [LoggingAspect]
    internal sealed class TimelineViewModel : PlotSettingsViewModelBase
    {
        public TimelineViewModel() : base()
        {
        }

        public override bool TryGetPlotSettings(out PlotSettings plotSettings)
        {
            if (StudyNameItems.Count == 0 || SelectedStudyName == null)
            {
                plotSettings = null;
                return false;
            }

            plotSettings = new PlotSettings
            {
                PlotTypeName = "timeline",
                TargetStudyName = SelectedStudyName.Name,
            };
            return true;
        }
    }
}
