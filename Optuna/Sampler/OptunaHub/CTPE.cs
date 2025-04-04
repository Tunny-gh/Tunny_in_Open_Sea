using Optuna.Util;

using Python.Runtime;

namespace Optuna.Sampler.OptunaHub
{
    /// <summary>
    /// https://hub.optuna.org/samplers/auto_sampler/
    /// </summary>
    public class C_TPESampler : HubSamplerBase
    {
        private const string Package = "samplers/ctpe";

        public C_TPESampler()
            : base(ObjectiveNumberSupport.SingleObjective, ConstraintSupport.OnlyWithConstraint)
        {
        }

        public dynamic ToPython(bool hasConstraints)
        {
            dynamic optunahub = Py.Import("optunahub");
            dynamic module = optunahub.load_module(package: Package, force_reload: ForceReload);
            return module.cTPESampler(
                seed: Seed,
                constraints_func: hasConstraints ? ConstraintFunc() : null
            );
        }
    }
}
