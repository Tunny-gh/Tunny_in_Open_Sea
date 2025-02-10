namespace Optuna.Sampler.OptunaHub
{
    public class HubSamplerBase : SamplerBase
    {
        public bool ForceReload { get; set; }

        public HubSamplerBase(bool supportMultiObjective, bool supportConstraint, bool isSinglePurposeRestricted)
            : base(supportMultiObjective, supportConstraint, isSinglePurposeRestricted)
        {
        }
    }
}
