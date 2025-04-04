using System;

using Optuna.Util;

namespace Optuna.Sampler
{
    public class GASamplerBase : SamplerBase
    {
        public GASamplerBase(ObjectiveNumberSupport objectiveNumberSupport, ConstraintSupport constraintSupport, HumanInTheLoopSupport humanInTheLoopSupport)
         : base(objectiveNumberSupport, constraintSupport, humanInTheLoopSupport)
        {
        }

        protected static dynamic SetCrossover(dynamic optuna, string crossover)
        {
            switch (crossover)
            {
                case "Uniform":
                    return optuna.samplers.nsgaii.UniformCrossover();
                case "BLXAlpha":
                    return optuna.samplers.nsgaii.BLXAlphaCrossover();
                case "SPX":
                    return optuna.samplers.nsgaii.SPXCrossover();
                case "SBX":
                    return optuna.samplers.nsgaii.SBXCrossover();
                case "VSBX":
                    return optuna.samplers.nsgaii.VSBXCrossover();
                case "UNDX":
                    return optuna.samplers.nsgaii.UNDXCrossover();
                case "":
                    return null;
                default:
                    throw new ArgumentException("Unexpected crossover setting");
            }
        }
    }
}
