using System;

using Python.Runtime;

namespace OptunaTests
{
    public class TestFixture : IDisposable
    {
        public TestFixture()
        {
            PythonEngine.Initialize();
        }

        public void Dispose()
        {
            PythonEngine.Shutdown();
        }
    }

}
