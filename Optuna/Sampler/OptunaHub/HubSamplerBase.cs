using Optuna.Util;

namespace Optuna.Sampler.OptunaHub
{
    public class HubSamplerBase : SamplerBase
    {
        public bool ForceReload { get; set; }

        public HubSamplerBase(ObjectiveNumberSupport objectiveNumberSupport, bool supportConstraint)
            : base(objectiveNumberSupport, supportConstraint, HumanInTheLoopSupport.NotRecommended)
        {
        }
    }
}
