using System;

using Python.Runtime;

using Xunit;

namespace Optuna.Study.Tests
{
    public class StudyWrapperTests
    {
        [Fact]
        public void ConstructorArgTypeTest()
        {
            PythonEngine.Initialize();
            PythonEngine.BeginAllowThreads();
            using (Py.GIL())
            {
                var pyInt = new PyInt(1);
                Assert.Throws<ArgumentException>(() => new StudyWrapper(pyInt));
            }
            PythonEngine.Shutdown();
        }
    }
}
