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
        public void Test1()
        {
            using(Py.GIL())
            {
                dynamic optuna = Py.Import("optuna");
                dynamic study = optuna.create_study();
                var wrapper = new StudyWrapper(study);
                Assert.Equal(0, wrapper.Id);
            }
        }
    }
}
