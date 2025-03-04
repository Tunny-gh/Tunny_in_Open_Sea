using System;
using System.IO;

using Optuna.Sampler;
using Optuna.Sampler.OptunaHub;

using Python.Runtime;

using Tunny.Core.TEnum;
using Tunny.Core.Util;

namespace Tunny.Core.Settings
{
    public class Sampler
    {
        // OptunaHub
        public AutoSampler Auto { get; set; } = new AutoSampler();
        public MOEADSampler MOEAD { get; set; } = new MOEADSampler();
        public MoCmaEsSampler MoCmaEs { get; set; } = new MoCmaEsSampler();
        public DESampler DE { get; set; } = new DESampler();

        // Optuna
        public RandomSampler Random { get; set; } = new RandomSampler();
        public TpeSampler Tpe { get; set; } = new TpeSampler();
        public CmaEsSampler CmaEs { get; set; } = new CmaEsSampler();
        public NSGAIISampler NsgaII { get; set; } = new NSGAIISampler();
        public NSGAIIISampler NsgaIII { get; set; } = new NSGAIIISampler();
        public QMCSampler QMC { get; set; } = new QMCSampler();
        public BoTorchSampler BoTorch { get; set; } = new BoTorchSampler();
        public GPSampler GP { get; set; } = new GPSampler();
        public BruteForceSampler BruteForce { get; set; } = new BruteForceSampler();

        public dynamic ToPython(SamplerType type, string storagePath, bool hasConstraints, PyDict cmaEsX0)
        {
            TLog.MethodStart();
            dynamic optunaSampler;
            dynamic os = Py.Import("os");
            os.environ["OPTUNAHUB_CACHE_HOME"] = new PyString(Path.Combine(TEnvVariables.TunnyEnvPath, "optunahub"));
            switch (type)
            {
                case SamplerType.TPE:
                    optunaSampler = Tpe.ToPython(hasConstraints);
                    break;
                case SamplerType.GP:
                    optunaSampler = GP.ToPython(hasConstraints);
                    break;
                case SamplerType.BoTorch:
                    optunaSampler = BoTorch.ToPython(hasConstraints);
                    break;
                case SamplerType.NSGAII:
                    optunaSampler = NsgaII.ToPython(hasConstraints);
                    break;
                case SamplerType.NSGAIII:
                    optunaSampler = NsgaIII.ToPython(hasConstraints);
                    break;
                case SamplerType.CmaEs:
                    optunaSampler = CmaEs.ToPython(storagePath, cmaEsX0);
                    break;
                case SamplerType.QMC:
                    optunaSampler = QMC.ToPython();
                    break;
                case SamplerType.Random:
                    optunaSampler = Random.ToPython();
                    break;
                case SamplerType.BruteForce:
                    optunaSampler = BruteForce.ToPython();
                    break;
                case SamplerType.AUTO:
                    optunaSampler = Auto.ToPython(hasConstraints);
                    break;
                case SamplerType.MOEAD:
                    optunaSampler = MOEAD.ToPython();
                    break;
                case SamplerType.MoCmaEs:
                    optunaSampler = MoCmaEs.ToPython();
                    break;
                case SamplerType.DE:
                    optunaSampler = DE.ToPython();
                    break;
                default:
                    throw new ArgumentException("Invalid sampler type.");
            }
            return optunaSampler;
        }
    }
}
