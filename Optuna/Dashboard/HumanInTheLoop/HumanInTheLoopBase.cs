using System.IO;
using System.Reflection;

using Optuna.Artifacts;
using Optuna.Study;
using Optuna.Util;

using Python.Runtime;

namespace Optuna.Dashboard.HumanInTheLoop
{
    public class HumanInTheLoopBase
    {
        private protected readonly string _tmpPath;
        internal readonly FileSystemArtifactStoreWrapper ArtifactStore;

        public HumanInTheLoopBase(string tmpPath, string storagePath)
        {
            _tmpPath = tmpPath;
            string artifactPath = Path.Combine(Path.GetDirectoryName(storagePath), "artifacts");
            ArtifactStore = new FileSystemArtifactStoreWrapper(artifactPath);
            CheckDirectoryIsExist(_tmpPath, artifactPath);
        }

        public PyModule ImportBaseLibrary()
        {
            PyModule library = Py.CreateScope();
            SetStdOutErrDirection(library);
            library.Import("os");
            library.Import("textwrap");
            library.Import("threading");
            library.Import("time");
            library.Import("optuna");
            library.Import("optuna_dashboard");
            return library;
        }

        public void SetStdOutErrDirection(PyModule importedLibrary)
        {
            string ioPath = Path.Combine(_tmpPath, "hitl.tmp");
            importedLibrary.Import("sys");
            importedLibrary.Exec("path = r'" + ioPath + "'");
            importedLibrary.Exec("sys.stdout = open(path, 'w', encoding='utf-8')");
            importedLibrary.Exec("sys.stderr = open(path, 'w', encoding='utf-8')");
        }

        public static void WakeOptunaDashboard(string storagePath, string pythonPath)
        {
            if (File.Exists(storagePath) == false)
            {
                throw new FileNotFoundException("Storage file not found.", storagePath);
            }

            var dashboard = new Handler(
                Path.Combine(pythonPath, "Scripts", "optuna-dashboard.exe"),
                storagePath
            );
            dashboard.Run(true);
        }

        public static int GetRunningTrialNumber(StudyWrapper study)
        {
            PyModule ps = Py.CreateScope();
            var assembly = Assembly.GetExecutingAssembly();
            ps.Exec(ReadFileFromResource.Text(assembly, "Optuna.Dashboard.HumanInTheLoop.Python.running_trials_length.py"));
            dynamic runningTrialsLength = ps.Get("running_trials_length");

            return runningTrialsLength(study.PyInstance);
        }

        public static void CheckDirectoryIsExist(string tmpPath, string artifactPath)
        {
            if (!Directory.Exists(tmpPath))
            {
                Directory.CreateDirectory(tmpPath);
            }
            if (!Directory.Exists(artifactPath))
            {
                Directory.CreateDirectory(artifactPath);
            }
        }
    }
}
