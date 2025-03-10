using Optuna.Util;

using OptunaTests;

using Python.Runtime;

using Xunit;

namespace Optuna.Sampler.OptunaHub.Tests
{
    public class AutoSamplerTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;

        public AutoSamplerTests(TestFixture fixture)
        {
            _fixture = fixture;
        }

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
            var sampler = new AutoSampler();
            dynamic pyObj = sampler.ToPython(false);
            Assert.Equal("AutoSampler", pyObj.ToString());
            dynamic pyObj2 = sampler.ToPython(true);
            Assert.Equal("AutoSampler", pyObj2.ToString());
        }

        [Fact]
        public void RunOptimizeTest()
        {
            var sampler = new AutoSampler();
            dynamic optuna = Py.Import("optuna");
            dynamic study = optuna.create_study(sampler: sampler.ToPython());
            study.optimize(_fixture.ObjectiveFunc, n_trials: 10);
        }
    }
}
