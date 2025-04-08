using System;

using Optuna.Trial;

using Tunny.Core.Handler;
using Tunny.Core.Settings;
using Tunny.Core.TEnum;
using Tunny.Core.Util;
using Tunny.Eto.Common;

namespace Tunny.Process
{
    [LoggingAspect]
    public static class PruneProcess
    {
        private const string IntermediateValueKey = "intermediate_value_step_";
        internal const string PrunedTrialReportValueKey = "pruned_trial_report_value";

        public static void Progress(ProgressState pState, ref int step, ref DateTime timer)
        {
            if (CommonSharedItems.Instance.Settings.Pruner.IsEnabled
                && pState.Pruner.GetPrunerStatus() == PrunerStatus.Runnable
                && DateTime.Now - timer > TimeSpan.FromSeconds(pState.Pruner.EvaluateIntervalSeconds))
            {
                step = Report(pState.TrialWrapper, step, pState.Pruner);
                timer = DateTime.Now;
            }
        }

        private static int Report(TrialWrapper trial, int step, Pruner pruner)
        {
            PrunerReport report = pruner.Evaluate();
            if (report == null)
            {
                return step;
            }
            else
            {
                trial.Report(report.IntermediateValue, step);
                if (!string.IsNullOrEmpty(report.Attribute))
                {
                    trial.SetUserAttribute(IntermediateValueKey + step, report.Attribute);
                }

                if (trial.ShouldPrune())
                {
                    pruner.RunStopperProcess();
                    if (report.TrialTellValue.HasValue)
                    {
                        trial.SetUserAttribute(PrunedTrialReportValueKey, report.TrialTellValue.Value);
                    }
                }

                return step + 1;
            }
        }
    }
}
