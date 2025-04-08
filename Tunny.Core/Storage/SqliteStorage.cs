using System;
using System.Linq;

using Optuna.Study;

using Python.Runtime;

using Tunny.Core.Util;

namespace Tunny.Core.Storage
{
    [LoggingAspect]
    public class SqliteStorage : PythonInit, ITStorage
    {
        public dynamic Storage { get; set; }

        public dynamic CreateNewTStorage(bool useInnerPythonEngine, Settings.Storage storageSetting)
        {
            string sqlitePath = storageSetting.GetOptunaStoragePathByExtension();
            if (useInnerPythonEngine)
            {
                InitializePythonEngine();
                using (Py.GIL())
                {
                    CreateTStorageProcess(sqlitePath);
                }
                PythonEngine.Shutdown();
            }
            else
            {
                CreateTStorageProcess(sqlitePath);
            }

            return Storage;
        }

        private void CreateTStorageProcess(string sqlitePath)
        {
            dynamic optuna = Py.Import("optuna");
            Storage = optuna.storages.RDBStorage(sqlitePath);
        }

        public void DuplicateStudyInStorage(string fromStudyName, string toStudyName, Settings.Storage storageSetting)
        {
            string storage = storageSetting.GetOptunaStoragePath();
            InitializePythonEngine();
            using (Py.GIL())
            {
                dynamic optuna = Py.Import("optuna");
                optuna.copy_study(from_study_name: fromStudyName, to_study_name: toStudyName, from_storage: storage, to_storage: storage);
            }
            PythonEngine.Shutdown();
        }

        public StudySummary[] GetStudySummaries(string storagePath)
        {
            var storage = new Optuna.Storage.RDB.SqliteStorage(storagePath);
            StudySummary[] studySummaries = Study.GetAllStudySummaries(storage);

            var oldFormatVersion = new Version("0.9.1");
            foreach (StudySummary studySummary in studySummaries)
            {
                string versionString = (studySummary.UserAttrs["tunny_version"] as string[])[0];
                var version = new Version(versionString);
                if (version <= oldFormatVersion)
                {
                    UpdateVariableNamesAttr(studySummary);
                }
            }

            return studySummaries;
        }

        private static void UpdateVariableNamesAttr(StudySummary studySummary)
        {
            studySummary.UserAttrs["variable_names"] = (studySummary.UserAttrs["variable_names"] as string[])[0].Split(',').ToArray();
        }
    }
}
