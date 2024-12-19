﻿using System.Linq;

using Tunny.Core.Settings;

namespace Tunny.WPF.ViewModels.Visualize
{
    internal sealed class ContourViewModel : PlotSettingsViewModelBase
    {
        public ContourViewModel() : base()
        {
        }

        public override bool TryGetPlotSettings(out PlotSettings plotSettings)
        {
            if (StudyNameItems.Count == 0 || SelectedStudyName == null ||
                ObjectiveItems.Count == 0 ||
                VariableItems.Count < 2 || VariableItems.Where(v => v.IsSelected).Count() < 2)
            {
                plotSettings = null;
                return false;
            }

            plotSettings = new PlotSettings()
            {
                TargetStudyName = SelectedStudyName.Name,
                PlotTypeName = "contour",
                TargetObjectiveName = new string[] { SelectedObjective.Name },
                TargetObjectiveIndex = new int[] { ObjectiveItems.IndexOf(SelectedObjective) },
                TargetVariableName = VariableItems.Where(v => v.IsSelected).Select(v => v.Name).ToArray(),
            };
            return true;
        }
    }
}
