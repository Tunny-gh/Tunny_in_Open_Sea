using System;

using Python.Runtime;

namespace OptunaTests
{
    public class TestFixture : IDisposable
    {
        public dynamic ObjectiveFunc { get; }

        public TestFixture()
        {
            PythonEngine.Initialize();
            PythonEngine.BeginAllowThreads();

            PyModule ps = Py.CreateScope();
            ps.Exec(
                "def objective(trial):\n" +
                "    x = trial.suggest_float('x', -10, 10)\n" +
                "    return (x - 2) ** 2"
            );
            ObjectiveFunc = ps.Get("objective");
        }

        public void Dispose()
        {
            PythonEngine.Shutdown();
        }
    }
}
