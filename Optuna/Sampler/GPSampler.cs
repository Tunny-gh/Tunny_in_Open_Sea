using System;

using Python.Runtime;

namespace Optuna.Sampler
{
    /// <summary>
    /// https://optuna.readthedocs.io/en/stable/reference/generated/optuna.samplers.GPSampler.html
    /// </summary>
    public class GPSampler : SamplerBase
    {
        public int NStartupTrials { get; set; } = 10;
        public bool DeterministicObjective { get; set; } = true;

        public GPSampler()
         : base(false, true, false, false)
        {
        }

        public void ComputeAutoValue(int numberOfTrials)
        {
            if (NStartupTrials == -1)
            {
                NStartupTrials = Math.Min(50, numberOfTrials / 10);
            }
        }

        public dynamic ToPython(bool hasConstraints)
        {
            dynamic optuna = Py.Import("optuna");
            return optuna.samplers.GPSampler(
                seed: Seed,
                n_startup_trials: NStartupTrials,
                deterministic_objective: DeterministicObjective,
                constraints_func: hasConstraints ? ConstraintFunc() : null
            );
        }
    }
}
