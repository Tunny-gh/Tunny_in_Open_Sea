using Python.Runtime;

namespace Optuna.Sampler.OptunaHub
{
    /// <summary>
    /// https://hub.optuna.org/samplers/auto_sampler/
    /// </summary>
    public class AutoSampler : HubSamplerBase
    {
        private const string Package = "samplers/auto_sampler";

        public dynamic ToPython()
        {
            dynamic optunahub = Py.Import("optunahub");
            dynamic module = optunahub.load_module(package: Package, force_reload: ForceReload);
            return module.AutoSampler(
                seed: Seed
            );
        }
    }
}
