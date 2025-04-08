using System.Collections.Generic;

using Tunny.Core.Util;
using Tunny.Input;

namespace Tunny.PostProcess
{
    [LoggingAspect]
    public class TrialGrasshopperItems
    {
        public Objective Objectives { get; set; }
        public Dictionary<string, List<string>> Attributes { get; set; }
        public Artifact Artifacts { get; set; }

        public TrialGrasshopperItems()
        {
        }

        public TrialGrasshopperItems(double[] values)
        {
            Objectives = new Objective(values);
            Attributes = new Dictionary<string, List<string>>();
            Artifacts = new Artifact();
        }
    }
}
