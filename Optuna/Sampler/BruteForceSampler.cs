using Python.Runtime;

namespace Optuna.Sampler
{
    /// <summary>
    /// https://optuna.readthedocs.io/en/stable/reference/samplers/generated/optuna.samplers.BruteForceSampler.html
    /// </summary>
    public class BruteForceSampler : SamplerBase
    {
        public BruteForceSampler()
          : base(true, false, false)
        {
        }

        public dynamic ToPython()
        {
            dynamic optuna = Py.Import("optuna");
            return optuna.samplers.BruteForceSampler(
                seed: Seed
            );
        }
    }
}
