using Python.Runtime;

namespace Optuna.Sampler.OptunaHub
{
    /// <summary>
    /// https://hub.optuna.org/samplers/differential_evolution/
    /// </summary>
    public class DESampler : HubSamplerBase
    {
        private const string Package = "samplers/differential_evolution";
        public double DEFactor { get; set; } = 0.8;
        public double CrossOverRate { get; set; } = 0.7;
        public int? PopulationSize { get; set; }

        public dynamic ToPython()
        {
            dynamic optunahub = Py.Import("optunahub");
            dynamic module = optunahub.load_module(package: Package, force_reload: ForceReload);
            PyObject popSize;
            if (PopulationSize == null || PopulationSize.Value <= 0)
            {
                popSize = new PyString("auto");
            }
            else
            {
                popSize = new PyInt(PopulationSize.Value);
            }
            return module.DESampler(
                seed: Seed,
                population_size: popSize,
                F: DEFactor,
                CR: CrossOverRate,
                debug: false
            );
        }
    }
}
