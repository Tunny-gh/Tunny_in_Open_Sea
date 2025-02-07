using System;
using System.Collections.Generic;

using Tunny.Input;

namespace Tunny.PostProcess
{
    public class TrialGrasshopperItems
    {
        public Objective Objectives { get; set; }
        public Dictionary<string, List<string>> Attribute { get; set; }
        public Artifact Artifacts { get; set; }

        public TrialGrasshopperItems()
        {
        }

        public TrialGrasshopperItems(double[] values)
        {
            Objectives = new Objective(values);
            Attribute = new Dictionary<string, List<string>>();
            Artifacts = new Artifact();
        }
    }
}
