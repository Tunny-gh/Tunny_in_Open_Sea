using Optuna.Util;

using Python.Runtime;

using Xunit;

namespace Optuna.Sampler.OptunaHub.Tests
{
    public class AutoSamplerTests
    {
        [Fact]
        public void ConstructorTest()
        {
            var sampler = new AutoSampler();
            Assert.Equal(ObjectiveNumberSupport.Any, sampler.ObjectiveNumberSupport);
            Assert.False(sampler.ForceReload);
        }

        [Fact]
        public void ToPythonTest()
        {
            PythonEngine.Initialize();
            PythonEngine.BeginAllowThreads();
            using(Py.GIL())
            {
                var sampler = new AutoSampler();
                dynamic pyObj = sampler.ToPython(false);
                Assert.Equal("AutoSampler", pyObj.ToString());
                dynamic pyObj2 = sampler.ToPython(true);
                Assert.Equal("AutoSampler", pyObj2.ToString());
            }
            PythonEngine.Shutdown();
        }
    }
}
