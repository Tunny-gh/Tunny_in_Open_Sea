using Optuna.Util;

using Python.Runtime;

namespace Optuna.Sampler.OptunaHub
{
    public class MoCmaEsSampler : HubSamplerBase
    {
        private const string Package = "samplers/mocma";
        public int? PopulationSize { get; set; }

        public MoCmaEsSampler()
            : base(ObjectiveNumberSupport.MultiObjective, ConstraintSupport.None)
        {
        }

        public dynamic ToPython(string refCommit)
        {
            dynamic optunahub = Py.Import("optunahub");
            dynamic module = optunahub.load_module(package: Package, force_reload: ForceReload, @ref: refCommit);
            return module.MoCmaSampler(
                popsize: PopulationSize,
                seed: Seed
            );
        }
    }
}
