using System;

namespace Optuna.Sampler
{
    /// <summary>
    /// https://optuna.readthedocs.io/en/stable/reference/generated/optuna.integration.BoTorchSampler.html
    /// </summary>
    public class BoTorchSampler : SamplerBase
    {
        public int NStartupTrials { get; set; } = 10;

        public void ComputeAutoValue(int numberOfTrials)
        {
            if (NStartupTrials == -1)
            {
                NStartupTrials = Math.Min(10, numberOfTrials / 10);
            }
        }

        public dynamic ToPython(dynamic optuna, bool hasConstraints)
        {
            return optuna.integration.BoTorchSampler(
                seed: Seed,
                n_startup_trials: NStartupTrials,
                constraints_func: hasConstraints ? ConstraintFunc() : null
            );
        }
    }
}
