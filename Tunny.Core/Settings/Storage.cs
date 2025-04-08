using System;
using System.IO;

using Optuna.Storage;
using Optuna.Study;

using Python.Runtime;

using Tunny.Core.Storage;
using Tunny.Core.TEnum;
using Tunny.Core.Util;

namespace Tunny.Core.Settings
{
    [LoggingAspect]
    public class Storage : PythonInit
    {
        public string Path { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/fish.log";
        public StorageType Type { get; set; } = StorageType.Journal;

        public string GetArtifactBackendPath()
        {
            return System.IO.Path.GetDirectoryName(Path) + "/artifacts";
        }

        public string GetOptunaStoragePath()
        {
            switch (Type)
            {
                case StorageType.InMemory:
                    return string.Empty;
                case StorageType.Sqlite:
                    return "sqlite:///" + Path;
                case StorageType.Postgres:
                case StorageType.MySql:
                    throw new NotImplementedException();
                case StorageType.Journal:
                    return Path;
                default:
                    throw new NotImplementedException();
            }
        }

        public string GetOptunaStoragePathByExtension()
        {
            switch (System.IO.Path.GetExtension(Path))
            {
                case null:
                    return string.Empty;
                case ".sqlite3":
                case ".db":
                    return "sqlite:///" + Path;
                case ".log":
                    return Path;
                default:
                    throw new NotImplementedException();
            }
        }

        public StorageType GetStorageTypeByExtension()
        {
            switch (System.IO.Path.GetExtension(Path))
            {
                case null:
                    return StorageType.InMemory;
                case ".sqlite3":
                case ".db":
                    return StorageType.Sqlite;
                case ".log":
                    return StorageType.Journal;
                default:
                    throw new NotImplementedException();
            }
        }

        public dynamic CreateNewOptunaStorage(bool useInnerPythonEngine)
        {
            dynamic storage;
            switch (Type)
            {
                case StorageType.InMemory:
                    storage = new InMemoryStorage().CreateNewTStorage(useInnerPythonEngine, this);
                    break;
                case StorageType.Sqlite:
                    storage = new SqliteStorage().CreateNewTStorage(useInnerPythonEngine, this);
                    break;
                case StorageType.Journal:
                    storage = new JournalStorage().CreateNewTStorage(useInnerPythonEngine, this);
                    break;
                default:
                    throw new ArgumentException("Storage type is not defined.");
            }

            return storage;
        }

        public dynamic CreateNewOptunaArtifactBackend(bool useInnerPythonEngine)
        {
            dynamic backend;

            if (useInnerPythonEngine)
            {
                InitializePythonEngine();
                using (Py.GIL())
                {
                    backend = CreateArtifactBackendProcess();
                }
                PythonEngine.Shutdown();
            }
            else
            {
                backend = CreateArtifactBackendProcess();
            }

            return backend;
        }

        private dynamic CreateArtifactBackendProcess()
        {
            dynamic optuna = Py.Import("optuna");
            string backendPath = GetArtifactBackendPath();
            Directory.CreateDirectory(backendPath);
            return optuna.artifacts.FileSystemArtifactStore(base_path: backendPath);
        }

        public Study[] GetAllStudies()
        {
            IOptunaStorage storage;
            switch (Type)
            {
                case StorageType.Sqlite:
                    storage = new Optuna.Storage.RDB.SqliteStorage(Path);
                    break;
                case StorageType.Journal:
                    storage = new Optuna.Storage.Journal.JournalStorage(Path);
                    break;
                default:
                    throw new ArgumentException("Storage type is not defined.");
            }
            return storage.GetAllStudies();
        }
    }
}
