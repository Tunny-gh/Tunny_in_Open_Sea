using System;

using OptunaTests;

using Python.Runtime;

using Xunit;

namespace Optuna.Study.Tests
{
    public class StudyWrapperTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;

        public StudyWrapperTests(TestFixture fixture)
        {
            _fixture = fixture;
        }

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
