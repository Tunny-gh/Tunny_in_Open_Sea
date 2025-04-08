using System;
using System.Collections.Generic;

using Optuna.Trial;

using Tunny.Core.Input;
using Tunny.Core.Settings;
using Tunny.Core.Util;

namespace Tunny.Core.Handler
{
    [LoggingAspect]
    public class ProgressState
    {
        public IList<Parameter> Parameter { get; set; }
        public int TrialNumber { get; set; }
        public int ObjectiveNum { get; set; }
        public double[][] BestValues { get; set; }
        public double HypervolumeRatio { get; set; }
        public TimeSpan EstimatedTimeRemaining { get; set; }
        public bool IsReportOnly { get; set; }
        public TrialWrapper TrialWrapper { get; set; }
        public Pruner Pruner { get; set; }
        public int PercentComplete { get; set; }

        public ProgressState()
        {
        }

        public ProgressState(IList<Parameter> parameters)
        {
            Parameter = parameters;
        }

        public ProgressState(IList<Parameter> parameters, bool isReportOnly)
        {
            Parameter = parameters;
            IsReportOnly = isReportOnly;
        }
    }
}
