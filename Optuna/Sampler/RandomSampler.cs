using Optuna.Util;

using Python.Runtime;

namespace Optuna.Sampler
{
    /// <summary>
    /// https://optuna.readthedocs.io/en/stable/reference/generated/optuna.samplers.RandomSampler.html#optuna.samplers.RandomSampler
    /// </summary>
    public class RandomSampler : SamplerBase
    {
        public RandomSampler()
         : base(ObjectiveNumberSupport.Any, false, HumanInTheLoopSupport.NotRecommended)
        {
        }

        public dynamic ToPython()
        {
            dynamic optuna = Py.Import("optuna");
            return optuna.samplers.RandomSampler(
                seed: Seed
            );
        }
    }
}
