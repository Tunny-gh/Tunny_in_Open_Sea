
using Optuna.Util;

using Python.Runtime;

namespace Optuna.Sampler.OptunaHub
{
    /// <summary>
    /// https://hub.optuna.org/samplers/hebo/
    /// </summary>
    public class HEBOSampler : HubSamplerBase
    {
        private const string Package = "samplers/hebo";

        public HEBOSampler()
            : base(ObjectiveNumberSupport.SingleObjective, ConstraintSupport.None)
        {
        }

        public dynamic ToPython(string refCommit)
        {
            dynamic optunahub = Py.Import("optunahub");
            dynamic module = optunahub.load_module(package: Package, force_reload: ForceReload, @ref: refCommit);
            return module.HEBOSampler(
                seed: Seed
            );
        }
    }
}
