using Optuna.Sampler.OptunaHub;

using Python.Runtime;

namespace Optuna.Sampler
{
    ///  <summary>
    /// https://optuna.readthedocs.io/en/stable/reference/generated/optuna.samplers.NSGAIIISampler.html
    /// </summary>
    public class NSGAIIISampler : NSGAIISampler
    {
        public double[] ReferencePoints { get; set; }
        public int DividingParameter { get; set; } = 3;

        public dynamic ToPython(bool hasConstraints)
        {
            dynamic optuna = Py.Import("optuna");
            return optuna.samplers.NSGAIIISampler(
                population_size: PopulationSize,
                mutation_prob: MutationProb,
                crossover_prob: CrossoverProb,
                swapping_prob: 0.5,
                seed: Seed,
                crossover: SetCrossover(Crossover, CrossoverParam),
                constraints_func: hasConstraints ? ConstraintFunc() : null,
                reference_points: ReferencePoints == null || ReferencePoints.Length == 0 ? null : ReferencePoints,
                dividing_parameter: DividingParameter
            );
        }
    }
}
