using System;

using Optuna.Util;

using Python.Runtime;

namespace Optuna.Sampler.OptunaHub
{
    /// <summary>
    /// https://hub.optuna.org/samplers/ctpe/
    /// </summary>
    public class cTPESampler : HubSamplerBase
    {
        private const string Package = "samplers/ctpe";
        public bool ConsiderPrior { get; set; } = true;
        public double PriorWeight { get; set; } = 1.0;
        public double CategoricalPriorWeight { get; set; } = 0.2;
        public bool ConsiderMagicClip { get; set; } = true;
        public int NStartupTrials { get; set; } = 10;
        public int NEICandidates { get; set; } = 24;
        public bool Multivariate { get; set; } = true;
        public double MinBandWidthFactor { get; set; } = 0.01;
        public string GammaStrategy { get; set; } = "sqrt";
        public double GammaBeta { get; set; } = 0.25;
        public string BandwidthStrategy { get; set; } = "scott";
        public string WeightStrategy { get; set; } = "uniform";
        public bool UseMinBandWidthDiscrete { get; set; } = true;

        public cTPESampler()
            : base(ObjectiveNumberSupport.SingleObjective, ConstraintSupport.OnlyWithConstraint)
        {
        }

        public void ComputeAutoValue(int numberOfTrials)
        {
            if (NStartupTrials == -1)
            {
                NStartupTrials = Math.Max(10, Math.Min(100, numberOfTrials / 10));
            }
        }

        public dynamic ToPython(string refCommit, bool hasConstraints)
        {
            dynamic optunahub = Py.Import("optunahub");
            dynamic numpy = Py.Import("numpy");
            dynamic module = optunahub.load_module(package: Package, force_reload: ForceReload, @ref: refCommit);
            return module.cTPESampler(
                seed: Seed,
                constraints_func: hasConstraints ? ConstraintFunc() : null,
                n_startup_trials: NStartupTrials,
                n_ei_candidates: NEICandidates,
                consider_magic_clip: ConsiderMagicClip,
                consider_prior: ConsiderPrior,
                prior_weight: PriorWeight,
                categorical_prior_weight: CategoricalPriorWeight,
                multivariate: Multivariate,
                b_magic_exponent: numpy.inf,
                min_bandwidth_factor: MinBandWidthFactor,
                gamma_beta: GammaBeta,
                gamma_strategy: GammaStrategy,
                weight_strategy: WeightStrategy.Replace("_", "-"),
                bandwidth_strategy: BandwidthStrategy,
                use_min_bandwidth_discrete: UseMinBandWidthDiscrete
            );
        }
    }

    public enum GammaStrategy
    {
        sqrt,
        linear
    }

    public enum BandwidthStrategy
    {
        scott,
        optuna,
        hyperopt
    }

    public enum WeightStrategy
    {
        uniform,
        old_decay,
        old_drop
    }
}
