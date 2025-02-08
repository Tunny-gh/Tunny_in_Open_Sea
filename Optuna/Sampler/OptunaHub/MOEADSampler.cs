using Python.Runtime;

namespace Optuna.Sampler.OptunaHub
{
    /// <summary>
    /// https://hub.optuna.org/samplers/moead/
    /// </summary>
    public class MOEADSampler : GASamplerBase
    {
        private const string Package = "samplers/moead";
        public double? MutationProb { get; set; }
        public int PopulationSize { get; set; } = 50;
        public string Crossover { get; set; } = "BLXAlpha";
        public double CrossoverProb { get; set; } = 0.9;
        public double SwappingProb { get; set; } = 0.5;
        public ScalarAggregationType ScalarAggregation { get; set; } = ScalarAggregationType.tchebycheff;
        public int NumNeighbors { get; set; } = -1;
        public bool ForceReload { get; set; }

        public dynamic ToPython()
        {
            dynamic optuna = Py.Import("optuna");
            dynamic optunahub = Py.Import("optunahub");

            if (NumNeighbors <= 0)
            {
                NumNeighbors = PopulationSize / 5;
            }
            dynamic module = optunahub.load_module(package: Package, force_reload: ForceReload);
            return module.MOEADSampler(
                population_size: PopulationSize,
                mutation_prob: MutationProb,
                crossover_prob: CrossoverProb,
                swapping_prob: SwappingProb,
                seed: Seed,
                crossover: SetCrossover(optuna, Crossover),
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
