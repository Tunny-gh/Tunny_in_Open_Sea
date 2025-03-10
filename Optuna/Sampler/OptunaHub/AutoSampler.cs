using Optuna.Util;

using Python.Runtime;

namespace Optuna.Sampler.OptunaHub
{
    /// <summary>
    /// https://hub.optuna.org/samplers/auto_sampler/
    /// </summary>
    public class AutoSampler : HubSamplerBase
    {
        private const string Package = "samplers/auto_sampler";

        public AutoSampler()
            : base(ObjectiveNumberSupport.Any, true)
        {
        }

        public dynamic ToPython(bool hasConstraints)
        {
            dynamic optunahub = Py.Import("optunahub");
            dynamic module = optunahub.load_module(package: Package, force_reload: ForceReload);
            return module.AutoSampler(
                seed: Seed,
                constraints_func: hasConstraints ? ConstraintFunc() : null
            );
        }
    }
}
