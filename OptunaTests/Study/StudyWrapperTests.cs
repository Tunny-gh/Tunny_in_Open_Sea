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
        public void ConstructorArgStudyTest()
        {
            using (Py.GIL())
            {
                dynamic optuna = Py.Import("optuna");
                dynamic study = optuna.create_study();
                var studyWrapper = new StudyWrapper(study);
                Assert.Equal(study, studyWrapper.PyInstance);
                Assert.Equal(0, studyWrapper.Id);
            }
        }

        [Fact]
        public void ConstructorArgPyIntTest()
        {
            using (Py.GIL())
            {
                var pyInt = new PyInt(1);
                Assert.Throws<ArgumentException>(() => new StudyWrapper(pyInt));
            }
        }
    }
}
