using Optuna.Util;

using Python.Runtime;

namespace Optuna.Sampler.OptunaHub
{
    ///  <summary>
    /// https://optuna.readthedocs.io/en/stable/reference/generated/optuna.samplers.NSGAIISampler.html
    /// </summary>
    public class NSGAIISampler : GASamplerBase
    {
        private const string Package = "samplers/nsgaii_with_initial_trials";
        public bool ForceReload { get; set; }

        public NSGAIISampler()
            : base(ObjectiveNumberSupport.Any, ConstraintSupport.Supported, HumanInTheLoopSupport.NotRecommended)
        {
        }

        public dynamic ToPython(string refCommit, bool hasConstraints)
        {
            dynamic optunahub = Py.Import("optunahub");
            dynamic module = optunahub.load_module(package: Package, force_reload: ForceReload, @ref: refCommit);
            return module.NSGAIIwITSampler(
                population_size: PopulationSize,
                mutation_prob: MutationProb,
                crossover_prob: CrossoverProb,
                swapping_prob: 0.5,
                seed: Seed,
                crossover: SetCrossover(Crossover, CrossoverParam),
                mutation: SetMutation(Mutation, module, MutationParam),
                constraints_func: hasConstraints ? ConstraintFunc() : null
            );
        }
    }
}
