using Newtonsoft.Json;

using Optuna.Util;

namespace Optuna.Sampler.OptunaHub
{
    public class HubSamplerBase : SamplerBase
    {
        [JsonIgnore]
        public bool ForceReload { get; set; }

        public HubSamplerBase(ObjectiveNumberSupport objectiveNumberSupport, ConstraintSupport constraintSupport)
            : base(objectiveNumberSupport, constraintSupport, HumanInTheLoopSupport.NotRecommended)
        {
        }
    }
}
