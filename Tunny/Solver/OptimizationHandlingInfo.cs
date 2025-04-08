using System.Collections.Generic;

using Optuna.Dashboard.HumanInTheLoop;
using Optuna.Study;

using Tunny.Core.Util;
using Tunny.Type;

namespace Tunny.Solver
{
    [LoggingAspect]
    sealed internal class OptimizationHandlingInfo
    {
        public int NTrials { get; set; }
        public double Timeout { get; set; }
        public StudyWrapper Study { get; set; }
        public dynamic Storage { get; set; }
        public dynamic ArtifactBackend { get; set; }
        public List<FishEgg> EnqueueItems { get; set; }
        public string[] ObjectiveNames { get; set; }
        public HumanSliderInput HumanSliderInput { get; set; }
        public Preferential Preferential { get; set; }

        public OptimizationHandlingInfo(int nTrials, double timeout, StudyWrapper study, dynamic storage,
                                        dynamic artifactBackend, List<FishEgg> enqueueItems,
                                        string[] objectiveNames)
        {
            NTrials = nTrials;
            Timeout = timeout;
            Study = study;
            Storage = storage;
            ArtifactBackend = artifactBackend;
            EnqueueItems = enqueueItems;
            ObjectiveNames = objectiveNames;
        }
    }
}
