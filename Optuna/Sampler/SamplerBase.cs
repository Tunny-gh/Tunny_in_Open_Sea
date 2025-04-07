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
        public ConstraintSupport ConstraintSupport { get; }
        public ObjectiveNumberSupport ObjectiveNumberSupport { get; }
        public HumanInTheLoopSupport HumanInTheLoopSupport { get; }
        public int? Seed { get; set; }

        public SamplerBase(ObjectiveNumberSupport objectiveNumberSupport, ConstraintSupport constraintSupport, HumanInTheLoopSupport humanInTheLoopSupport)
        {
            ObjectiveNumberSupport = objectiveNumberSupport;
            ConstraintSupport = constraintSupport;
            HumanInTheLoopSupport = humanInTheLoopSupport;
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
