using Optuna.Util;

using Python.Runtime;

namespace Optuna.Sampler.Dashboard
{
    public class PreferentialGpSampler : SamplerBase
    {
        public PreferentialGpSampler()
         : base(ObjectiveNumberSupport.SingleObjective, ConstraintSupport.None, HumanInTheLoopSupport.OnlyHumanInTheLoop)
        {
        }

        public dynamic ToPython()
        {
            PyModule sampler = Py.CreateScope();
            sampler.Exec("from optuna_dashboard.preferential.samplers.gp import PreferentialGPSampler");
            dynamic preferentialGPSampler = sampler.Get("PreferentialGPSampler");
            return preferentialGPSampler(
                seed: Seed
            );
        }
    }
}
