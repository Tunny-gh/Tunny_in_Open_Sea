using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Tunny.Core.Util
{
    public static class TEnvVariables
    {
        public static Version Version { get; } = new Version(1, 0, 5);
        public static string DefaultStoragePath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "fish.log");
        public static string TunnyEnvPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".tunny_dev_env");
        public static string LogPath { get; } = Path.Combine(TunnyEnvPath, "logs");
        public static string QuitFishingPath { get; } = Path.Combine(TunnyEnvPath, "quit.fishing");
        public static string DesignExplorerPath { get; } = Path.Combine(TunnyEnvPath, "TT-DesignExplorer");
        public static string OptimizeSettingsPath { get; } = Path.Combine(TunnyEnvPath, "settings.json");
        public static string ComponentFolder { get; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string ExampleDirPath { get; } = Path.Combine(ComponentFolder, "Examples", "Grasshopper");
        public static Version OldStorageVersion { get; } = new Version("0.9.1");
        public static IntPtr GrasshopperWindowHandle { get; set; }
        public static string PythonPath
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return Path.Combine(TunnyEnvPath, "python");
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".rhinocode", "py39-rh8");
                }
                else
                {
                    throw new PlatformNotSupportedException();
                }
            }
        }
        public static string PythonDllPath
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return Path.Combine(PythonPath, @"python310.dll");
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return Path.Combine(PythonPath, "libpython3.9.dylib");
                }
                else
                {
                    throw new PlatformNotSupportedException();
                }
            }
        }
        public static string DashboardPath
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return Path.Combine(PythonPath, "Scripts", "optuna-dashboard.exe");
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return Path.Combine(PythonPath, "bin", "optuna-dashboard");
                }
                else
                {
                    throw new PlatformNotSupportedException();
                }
            }
        }

        public static string TmpDirPath
        {
            get
            {
                string path = Path.Combine(TunnyEnvPath, "tmp");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
        }
    }
}
