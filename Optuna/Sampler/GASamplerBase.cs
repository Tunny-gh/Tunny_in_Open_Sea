using Optuna.Util;

using Python.Runtime;

namespace Optuna.Sampler
{
    public class GASamplerBase : SamplerBase
    {
        public string Crossover { get; set; } = "BLXAlpha";
        public double CrossoverProb { get; set; } = 0.9;
        public string Mutation { get; set; } = "Uniform";
        public double? MutationProb { get; set; }
        public int PopulationSize { get; set; } = 50;
        public double?[] CrossoverParam { get; set; }
        public double MutationParam { get; set; }

        public GASamplerBase(ObjectiveNumberSupport objectiveNumberSupport, ConstraintSupport constraintSupport, HumanInTheLoopSupport humanInTheLoopSupport)
         : base(objectiveNumberSupport, constraintSupport, humanInTheLoopSupport)
        {
        }

        protected static dynamic SetCrossover(string crossover, double?[] param)
        {
            dynamic optuna = Py.Import("optuna");
            switch (crossover)
            {
                case "Uniform":
                    return optuna.samplers.nsgaii.UniformCrossover(swapping_prob: param[0]);
                case "BLXAlpha":
                    return optuna.samplers.nsgaii.BLXAlphaCrossover(alpha: param[0]);
                case "SPX":
                    return optuna.samplers.nsgaii.SPXCrossover(epsilon: param[0]);
                case "SBX":
                    return optuna.samplers.nsgaii.SBXCrossover(eta: param[0], uniform_crossover_prob: param[1], use_child_gene_prob: param[2]);
                case "VSBX":
                    return optuna.samplers.nsgaii.VSBXCrossover(eta: param[0]);
                case "UNDX":
                    return optuna.samplers.nsgaii.UNDXCrossover(sigma_xi: param[0], sigma_eta: param[1]);
                default:
                    return optuna.samplers.nsgaii.UniformCrossover(swapping_prob: param[0]);
            }
        }

        protected static dynamic SetMutation(string mutation, dynamic module, double param)
        {
            switch (mutation)
            {
                case "Uniform":
                    return module.UniformMutation();
                case "Polynomial":
                    return module.PolynomialMutation(eta: param);
                case "Gaussian":
                    return module.GaussianMutation(sigma_factor: param);
                default:
                    return module.UniformMutation();
            }
        }
    }
}
