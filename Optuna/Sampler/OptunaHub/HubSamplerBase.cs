using Optuna.Util;

namespace Optuna.Sampler.OptunaHub
{
    public class HubSamplerBase : SamplerBase
    {
        public bool ForceReload { get; set; }

        public HubSamplerBase(ObjectiveNumberSupport objectiveNumberSupport, ConstraintSupport constraintSupport)
            : base(objectiveNumberSupport, constraintSupport, HumanInTheLoopSupport.NotRecommended)
        {
        }
    }
}
