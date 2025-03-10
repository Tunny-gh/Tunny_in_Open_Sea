using System;

using Python.Runtime;

namespace OptunaTests
{
    public class TestFixture : IDisposable
    {
        public TestFixture()
        {
        }

        public void Dispose()
        {
        }

        public static dynamic InitializeObjectiveFunction()
        {
            PyModule ps = Py.CreateScope();
            ps.Exec(
                "def objective(trial):\n" +
                "    x = trial.suggest_float('x', -10, 10)\n" +
                "    return (x - 2) ** 2"
            );
            return ps.Get("objective");
        }

    }
}
