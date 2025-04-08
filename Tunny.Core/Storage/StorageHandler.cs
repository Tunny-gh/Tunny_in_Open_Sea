using System;
using System.IO;

using Optuna.Study;

using Tunny.Core.Util;

namespace Tunny.Core.Storage
{
    [LoggingAspect]
    public class StorageHandler : ITStorage
    {
        public dynamic Storage { get; set; }

        public dynamic CreateNewTStorage(bool useInnerPythonEngine, Settings.Storage storageSetting)
        {
            ICreateTStorage storage;

            switch (Path.GetExtension(storageSetting.Path))
            {
                case null:
                    storage = new InMemoryStorage();
                    break;
                case ".sqlite3":
                case ".db":
                    storage = new SqliteStorage();
                    break;
                case ".log":
                    storage = new JournalStorage();
                    break;
                default:
                    throw new ArgumentException("Storage type not supported");
            }
            Storage = storage.CreateNewTStorage(useInnerPythonEngine, storageSetting);
            return Storage;
        }

        public void DuplicateStudyInStorage(string fromStudyName, string toStudyName, Settings.Storage storageSetting)
        {
            ITStorage storage;

            switch (Path.GetExtension(storageSetting.Path))
            {
                case ".sqlite3":
                case ".db":
                    storage = new SqliteStorage();
                    break;
                case ".log":
                    storage = new JournalStorage();
                    break;
                default:
                    throw new ArgumentException("Storage type not supported");
            }
            storage.DuplicateStudyInStorage(fromStudyName, toStudyName, storageSetting);
        }

        public StudySummary[] GetStudySummaries(string storagePath)
        {
            ITStorage storage;

            switch (Path.GetExtension(storagePath))
            {
                case ".sqlite3":
                case ".db":
                    storage = new SqliteStorage();
                    break;
                case ".log":
                    storage = new JournalStorage();
                    break;
                default:
                    throw new ArgumentException("Storage type not supported");
            }

            return storage.GetStudySummaries(storagePath);
        }
    }
}
