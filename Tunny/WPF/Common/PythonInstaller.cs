using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

using Tunny.Core.Util;
using Tunny.WPF.ViewModels;

namespace Tunny.WPF.Common
{
    [LoggingAspect]
    public class PythonInstaller
    {
        private readonly MainWindowViewModel _viewModel;
        private const string NotifierPrefix = "Python Library Installer: ";

        public PythonInstaller(MainWindowViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public Task RunAsync()
        {
            return Task.Run(() =>
            {
                _viewModel.ReportProgress($"{NotifierPrefix}started", 0);
                CheckDashboardProcess();
                string[] packageList = UnzipLibraries();
                InstallPackages(packageList);
                _viewModel.ReportProgress($"{NotifierPrefix}Completed", 100);
            });
        }

        private void CheckDashboardProcess()
        {
            _viewModel.ReportProgress($"{NotifierPrefix}Check Dashboard process...", 0);

            if (Optuna.Dashboard.Handler.KillExistDashboardProcess())
            {
                _viewModel.ReportProgress($"{NotifierPrefix}Killed optuna-dashboard process", 0);
            }
        }

        private string[] UnzipLibraries()
        {
            _viewModel.ReportProgress($"{NotifierPrefix}Unzip library...", 0);
            TLog.Info("Unzip library...");
            string envPath = TEnvVariables.TunnyEnvPath;
            string componentFolderPath = TEnvVariables.ComponentFolder;
            TLog.Info("Unzip Python libraries: " + envPath);
            if (Directory.Exists(envPath + "/python"))
            {
                Directory.Delete(envPath + "/python", true);
            }
            ZipFile.ExtractToDirectory(componentFolderPath + "/Lib/python.zip", envPath + "/python");

            if (Directory.Exists(envPath + "/Lib/whl"))
            {
                Directory.Delete(envPath + "/Lib/whl", true);
            }
            ZipFile.ExtractToDirectory(componentFolderPath + "/Lib/whl.zip", envPath + "/Lib/whl");
            return Directory.GetFiles(envPath + "/Lib/whl");
        }

        private void InstallPackages(string[] packageList)
        {
            int num = packageList.Length;
            for (int i = 0; i < num; i++)
            {
                double progress = (double)i / num * 100d;
                string packageName = Path.GetFileName(packageList[i]).Split('-')[0];
                string state = $"{NotifierPrefix}Installing {packageName}...";
                _viewModel.ReportProgress(state, progress);
                TLog.Info(state);
                var startInfo = new ProcessStartInfo
                {
                    FileName = TEnvVariables.PythonPath + "/python.exe",
                    Arguments = "-m pip install --no-deps " + packageList[i],
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                };
                using (var process = new System.Diagnostics.Process())
                {
                    process.StartInfo = startInfo;
                    process.Start();
                    process.WaitForExit();
                }
            }
            TLog.Info("Finish to install Python");
        }
    }
}
