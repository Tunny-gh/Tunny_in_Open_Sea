using Optuna.Sampler.Dashboard;
using Optuna.Study;

using OptunaTests;

using Python.Runtime;

using Xunit;

namespace Optuna.Dashboard.HumanInTheLoop.Tests
{
    public class PreferentialTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;

        public PreferentialTests(TestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void CreateStudyTest()
        {
            using (Py.GIL())
            {
                dynamic sampler = new PreferentialGpSampler().ToPython();
                var preferential = new Preferential("tmpPath", "storagePath", sampler);
                StudyWrapper study = preferential.CreateStudy(10, null, null, "objectiveName");
                Assert.NotNull(study);
            }
        }
    }
}
