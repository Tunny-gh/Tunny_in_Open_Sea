using System.Linq;

using Tunny.Core.Settings;
using Tunny.Core.Util;

namespace Tunny.WPF.ViewModels.Visualize
{
    [LoggingAspect]
    internal sealed class HypervolumeViewModel : PlotSettingsViewModelBase
    {
        private string _referencePoint;
        public string ReferencePoint { get => _referencePoint; set => SetProperty(ref _referencePoint, value); }

        public HypervolumeViewModel() : base()
        {
        }

        public override bool TryGetPlotSettings(out PlotSettings plotSettings)
        {
            if (StudyNameItems.Count == 0 || SelectedStudyName == null)
            {
                plotSettings = null;
                return false;
            }

            double[] referencePoint = string.IsNullOrEmpty(ReferencePoint) || ReferencePoint.Equals("AUTO", System.StringComparison.OrdinalIgnoreCase)
                ? null
                : ReferencePoint.Split(',').Select(double.Parse).ToArray();
            plotSettings = new PlotSettings
            {
                TargetStudyName = SelectedStudyName.Name,
                PlotTypeName = "hypervolume",
                ReferencePoint = referencePoint
            };
            return true;
        }
    }
}
