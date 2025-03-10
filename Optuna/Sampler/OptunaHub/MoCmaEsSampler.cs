using Optuna.Util;

using Python.Runtime;

namespace Optuna.Sampler.OptunaHub
{
    public class MoCmaEsSampler : HubSamplerBase
    {
        private const string Package = "samplers/mocma";
        public int? PopulationSize { get; set; }

        public MoCmaEsSampler()
            : base(ObjectiveNumberSupport.MultiObjective, false)
        {
        }

        public dynamic ToPython()
        {
            dynamic optunahub = Py.Import("optunahub");
            dynamic module = optunahub.load_module(package: Package, force_reload: ForceReload);
            return module.MoCmaSampler(
                popsize: PopulationSize,
                seed: Seed
            );
        }
    }
}
