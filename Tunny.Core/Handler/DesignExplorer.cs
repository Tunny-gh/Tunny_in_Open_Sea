using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;

using Optuna.Util;

using Python.Runtime;

using Tunny.Core.Util;

namespace Tunny.Core.Handler
{
    [LoggingAspect]
    public class DesignExplorer : PythonInit
    {
        private bool _hasImage;
        private readonly string _targetStudyName;
        private readonly Settings.Storage _storage;

        public DesignExplorer(string targetStudyName, Settings.Storage storage)
        {
            _targetStudyName = targetStudyName;
            _storage = storage;

            string envPath = TEnvVariables.TunnyEnvPath;
            if (!Directory.Exists(envPath + "/TT-DesignExplorer"))
            {
                SetupTTDesignExplorer();
            }
        }

        public void Run()
        {
            OutputResultCsv();
            KillExistTunnyServerProcess();
            int port = FindAvailablePort(8081);

            if (_hasImage)
            {
                string pythonDirectory = Path.Combine(TEnvVariables.TunnyEnvPath, "python");
                string dashboardPath = Path.Combine(pythonDirectory, "Scripts", "optuna-dashboard.exe");
                var dashboard = new Optuna.Dashboard.Handler(dashboardPath, _storage.Path);
                dashboard.Run(false);
            }

            var server = new Process();
            string path = Path.Combine(TEnvVariables.DesignExplorerPath, "TunnyDEServer.exe");
            server.StartInfo.FileName = path;
            server.StartInfo.Arguments = port.ToString(CultureInfo.InvariantCulture);
            server.StartInfo.WorkingDirectory = TEnvVariables.DesignExplorerPath;
            server.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            server.StartInfo.UseShellExecute = true;
            server.Start();

            var client = new Process();
            client.StartInfo.FileName = $"http://127.0.0.1:{port}/index.html";
            client.StartInfo.UseShellExecute = true;
            client.Start();
        }

        private void OutputResultCsv()
        {
            InitializePythonEngine();
            using (Py.GIL())
            {
                PyModule ps = Py.CreateScope();
                var assembly = Assembly.GetExecutingAssembly();
                ps.Exec(ReadFileFromResource.Text(assembly, "Tunny.Core.Handler.Python.export_fish_csv.py"));
                dynamic storage = _storage.CreateNewOptunaStorage(false);
                dynamic func = ps.Get("export_fish_csv");
                string outputPath = Path.Combine(TEnvVariables.DesignExplorerPath, "design_explorer_data");
                _hasImage = func(storage, _targetStudyName, outputPath);
            }
            PythonEngine.Shutdown();
        }

        private static void KillExistTunnyServerProcess()
        {
            int killCount = 0;
            Process[] server = Process.GetProcessesByName("TunnyDEServer");
            if (server.Length > 0)
            {
                foreach (Process p in server)
                {
                    p.Kill();
                    killCount++;
                }
            }
        }

        private static int FindAvailablePort(int startPort)
        {
            int port = startPort;
            while (IsPortInUse(port))
            {
                port++;
            }
            return port;
        }

        private static bool IsPortInUse(int port)
        {
            var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpListeners();

            foreach (IPEndPoint endpoint in tcpConnInfoArray)
            {
                if (endpoint.Port == port)
                {
                    return true;
                }
            }

            return false;
        }

        public static void SetupTTDesignExplorer()
        {
            string envPath = TEnvVariables.TunnyEnvPath;
            string componentFolderPath = TEnvVariables.ComponentFolder;
            TLog.Info("Unzip TT-DesignExplorer libraries: " + envPath);
            if (Directory.Exists(envPath + "/TT-DesignExplorer"))
            {
                KillExistTunnyServerProcess();
                Directory.Delete(envPath + "/TT-DesignExplorer", true);
            }
            ZipFile.ExtractToDirectory(componentFolderPath + "/Lib/TT-DesignExplorer.zip", envPath + "/TT-DesignExplorer");
        }
    }
}
