using System.Reflection;

using Optuna.Util;

using Python.Runtime;

namespace Optuna.Sampler
{
    /// <summary>
    /// https://optuna.readthedocs.io/en/stable/reference/samplers.html
    /// </summary>
    public class SamplerBase
    {
        public bool SupportsMultiObjective { get; }
        public bool SupportsConstraint { get; }
        public bool IsSinglePurposeRestricted { get; }
        public int? Seed { get; set; }

        public SamplerBase(bool supportMultiObjective, bool supportConstraint, bool isSinglePurposeRestricted)
        {
            SupportsMultiObjective = supportMultiObjective;
            SupportsConstraint = supportConstraint;
            IsSinglePurposeRestricted = isSinglePurposeRestricted;
        }

        public static dynamic ConstraintFunc()
        {
            PyModule ps = Py.CreateScope();
            var assembly = Assembly.GetExecutingAssembly();
            ps.Exec(ReadFileFromResource.Text(assembly, "Optuna.Sampler.Python.constraints.py"));
            return ps.Get("constraints");
        }
    }
}
