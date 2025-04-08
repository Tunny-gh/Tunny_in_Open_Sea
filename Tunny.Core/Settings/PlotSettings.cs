using Tunny.Core.Util;

namespace Tunny.Core.Settings
{
    [LoggingAspect]
    public class PlotSettings
    {
        public string PlotTypeName { get; set; }
        public string TargetStudyName { get; set; }
        public string[] TargetObjectiveName { get; set; }
        public int[] TargetObjectiveIndex { get; set; }
        public string[] TargetVariableName { get; set; }
        public int[] TargetVariableIndex { get; set; }
        public int ClusterCount { get; set; }
        public bool IncludeDominatedTrials { get; set; }
        public double[] ReferencePoint { get; set; }
        public string ImportanceEvaluator { get; set; }
    }
}
