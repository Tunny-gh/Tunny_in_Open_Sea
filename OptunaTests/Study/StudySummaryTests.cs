using System;

using Optuna.Storage;
using Optuna.Storage.Journal;
using Optuna.Storage.RDB;

using Xunit;

namespace Optuna.Study.Tests
{
    public class StudySummaryTests
    {
        [Theory]
        [InlineData(@"TestFile/sqlite.db", "sqlite")]
        [InlineData(@"TestFile/journal.log", "log")]
        public void StorageLoadStudySummaryTest(string path, string type)
        {
            IOptunaStorage storage = type == "sqlite"
                ? new SqliteStorage(path)
                : new JournalStorage(path);
            StudySummary[] summary = Study.GetAllStudySummaries(storage);
            Assert.Equal(3, summary.Length);
            Assert.Equal(2, summary[0].Directions.Length);
            Assert.Throws<InvalidOperationException>(() => summary[0].Direction);
            Assert.Equal(StudyDirection.Minimize, summary[1].Direction);
            Assert.Equal(3, summary[2].Directions.Length);
        }
    }
}
