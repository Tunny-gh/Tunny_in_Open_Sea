using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Optuna.Dashboard
{
    public class Handler
    {
        private readonly string _dashboardPath;
        private readonly string _storage;
        private readonly string _host;
        private readonly string _port;
        private readonly string _artifactDir;

        public Handler(string dashboardPath, string storagePath, string artifactDir = null, string host = "127.0.0.1", string port = "8080")
        {
            CheckFileExist(dashboardPath, storagePath);
            _dashboardPath = dashboardPath;
            _storage = GetStorageArgument(storagePath);
            _host = host;
            _port = FindAvailablePort(port);
            _artifactDir = artifactDir ?? Path.GetDirectoryName(storagePath) + "/artifacts";
            if (!Directory.Exists(_artifactDir))
            {
                Directory.CreateDirectory(_artifactDir);
            }
        }

        private string FindAvailablePort(string initialPort)
        {
            int port = int.Parse(initialPort, NumberStyles.Integer, CultureInfo.InvariantCulture);
            const int MAX_PORT = 65535;

            while (port <= MAX_PORT)
            {
                try
                {
                    var tcpListener = new TcpListener(IPAddress.Parse(_host), port);
                    try
                    {
                        tcpListener.Start();
                        tcpListener.Stop();
                        return port.ToString(CultureInfo.InvariantCulture);
                    }
                    finally
                    {
                        tcpListener.Server.Close();
                    }
                }
                catch (SocketException)
                {
                    port++;
                }
            }
            throw new InvalidOperationException("No available port found.");
        }


        private static void CheckFileExist(string dashboardPath, string storagePath)
        {
            if (!File.Exists(dashboardPath))
            {
                throw new FileNotFoundException("Dashboard file not found.", dashboardPath);
            }
            if (!File.Exists(storagePath))
            {
                throw new FileNotFoundException("Storage file not found.", storagePath);
            }
        }

        private static string GetStorageArgument(string path)
        {
            switch (Path.GetExtension(path))
            {
                case ".sqlite3":
                case ".db":
                    return @"sqlite:///" + $"\"{path}\"";
                case ".log":
                    return $"\"{path}\"";
                default:
                    throw new ArgumentException("Unsupported storage file.");
            }
        }

        public Process Run(bool openBrowser, string path = "")
        {
            KillExistDashboardProcess();
            string argument = $"\"{_storage}\" --host {_host} --port {_port} --artifact-dir \"{_artifactDir}\"";

            var dashboard = new Process();
            dashboard.StartInfo.FileName = _dashboardPath;
            dashboard.StartInfo.Arguments = argument;
            dashboard.StartInfo.UseShellExecute = false;
            dashboard.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            dashboard.Start();

            if (openBrowser)
            {
                OpenBrowser(path);
            }

            return dashboard;
        }

        private void OpenBrowser(string path)
        {
            var browser = new Process();
            browser.StartInfo.FileName = $@"http://{_host}:{_port}/{path}";
            browser.StartInfo.UseShellExecute = true;
            browser.Start();
        }

        public static bool KillExistDashboardProcess()
        {
            int killCount = 0;
            Process[] dashboardProcess = Process.GetProcessesByName("optuna-dashboard");
            if (dashboardProcess.Length > 0)
            {
                foreach (Process p in dashboardProcess)
                {
                    p.Kill();
                    killCount++;
                }
            }
            return killCount > 0;
        }
    }
}
