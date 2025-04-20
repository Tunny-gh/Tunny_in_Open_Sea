using Optuna.Util;

using Python.Runtime;

namespace Optuna.Sampler.OptunaHub
{
    /// <summary>
    /// https://hub.optuna.org/samplers/moead/
    /// </summary>
    public class MOEADSampler : GASamplerBase
    {
        private const string Package = "samplers/moead";
        public ScalarAggregationType ScalarAggregation { get; set; } = ScalarAggregationType.tchebycheff;
        public int NumNeighbors { get; set; } = -1;
        public bool ForceReload { get; set; }

        public MOEADSampler()
            : base(ObjectiveNumberSupport.MultiObjective, ConstraintSupport.None, HumanInTheLoopSupport.NotRecommended)
        {
        }

        public dynamic ToPython(string refCommit)
        {
            dynamic optunahub = Py.Import("optunahub");

            if (NumNeighbors <= 0)
            {
                NumNeighbors = PopulationSize / 5;
            }
            dynamic module = optunahub.load_module(package: Package, force_reload: ForceReload, @ref: refCommit);
            return module.MOEADSampler(
                population_size: PopulationSize,
                mutation_prob: MutationProb,
                crossover_prob: CrossoverProb,
                swapping_prob: 0.5,
                seed: Seed,
                crossover: SetCrossover(Crossover, CrossoverParam),
                scalar_aggregation_func: ScalarAggregation.ToString(),
                n_neighbors: NumNeighbors
            );
        }
    }

    public enum ScalarAggregationType
    {
        weighted_sum,
        tchebycheff
    }
}
