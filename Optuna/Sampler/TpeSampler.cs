using System;

using Python.Runtime;

namespace Optuna.Sampler
{
    /// <summary>
    /// https://optuna.readthedocs.io/en/stable/reference/generated/optuna.samplers.TPESampler.html
    /// </summary>
    public class TpeSampler : SamplerBase
    {
        public bool ConsiderPrior { get; set; } = true;
        public double PriorWeight { get; set; } = 1.0;
        public bool ConsiderMagicClip { get; set; } = true;
        public bool ConsiderEndpoints { get; set; }
        public int NStartupTrials { get; set; } = 10;
        public int NEICandidates { get; set; } = 24;
        public int Gamma { get; set; } = 25;
        public bool Multivariate { get; set; }
        public bool Group { get; set; }
        public bool WarnIndependentSampling { get; set; } = true;
        public bool ConstantLiar { get; set; }

        public TpeSampler()
         : base(true, true, false)
        {
        }

        public void ComputeAutoValue(int numberOfTrials)
        {
            if (NStartupTrials == -1)
            {
                NStartupTrials = Math.Min(100, numberOfTrials / 10);
            }
        }

        public dynamic ToPython(bool hasConstraints)
        {
            dynamic optuna = Py.Import("optuna");
            PyModule ps = Py.CreateScope();
            ps.Exec(
                "def tunny_gamma(x: int) -> int:\n" +
                $"  import numpy as np\n" +
                $"  return min(int(np.ceil(0.1 * x)), {Gamma})"
            );
            dynamic gammaFunc = ps.Get("tunny_gamma");

            return optuna.samplers.TPESampler(
                seed: Seed,
                consider_prior: ConsiderPrior,
                prior_weight: PriorWeight,
                consider_magic_clip: ConsiderMagicClip,
                consider_endpoints: ConsiderEndpoints,
                n_startup_trials: NStartupTrials,
                n_ei_candidates: NEICandidates,
                gamma: gammaFunc,
                multivariate: Multivariate,
                group: Group,
                warn_independent_sampling: WarnIndependentSampling,
                constant_liar: ConstantLiar,
                constraints_func: hasConstraints ? ConstraintFunc() : null
            );
        }
    }
}
