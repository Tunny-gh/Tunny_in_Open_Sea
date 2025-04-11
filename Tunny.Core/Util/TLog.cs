using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;

using Serilog;
using Serilog.Core;

using Tunny.Core.Settings;

namespace Tunny.Core.Util
{
    public static class TLog
    {
        private static bool s_isInitialized;
        private static readonly LoggingLevelSwitch LevelSwitch = new LoggingLevelSwitch();

        public static void InitializeLogger()
        {
            SetInitialLogLevels();
            if(!Directory.Exists(TEnvVariables.LogPath))
            {
                Directory.CreateDirectory(TEnvVariables.LogPath);
            }

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(LevelSwitch)
                .WriteTo.File(path: TEnvVariables.LogPath + "/log_.txt", rollingInterval: RollingInterval.Day, formatProvider: CultureInfo.InvariantCulture)
                .CreateLogger();
            CheckAndDeleteOldLogFiles();

            s_isInitialized = true;
            Log.Information("Tunny is loaded.");
        }

        private static void SetInitialLogLevels()
        {
            TSettings.TryLoadFromJson(out TSettings settings);
            LevelSwitch.MinimumLevel = settings.LogLevel;
        }

        private static void CheckAndDeleteOldLogFiles()
        {
            string logDirectory = TEnvVariables.LogPath;
            string logFilePattern = "*.txt";

            DateTime threshold = DateTime.Now.AddDays(-1);

            var directory = new DirectoryInfo(logDirectory);
            FileInfo[] logFiles = directory.GetFiles(logFilePattern);
            foreach (FileInfo file in logFiles)
            {
                if (file.LastWriteTime < threshold)
                {
                    file.Delete();
                }
            }
        }

        public static void Verbose(string message)
        {
            if (!s_isInitialized)
            {
                return;
            }
            Log.Verbose(message);
        }

        public static void Debug(string message)
        {
            if (!s_isInitialized)
            {
                return;
            }
            Log.Debug(message);
        }

        public static void Info(string message)
        {
            if (!s_isInitialized)
            {
                return;
            }
            Log.Information(message);
        }

        public static void Warning(string message)
        {
            if (!s_isInitialized)
            {
                return;
            }
            Log.Warning(message);
        }

        public static void Error(string message)
        {
            if (!s_isInitialized)
            {
                return;
            }
            Log.Error(message);
        }

        public static void Fatal(string message)
        {
            if (!s_isInitialized)
            {
                return;
            }
            Log.Fatal(message);
        }

        public static void MethodStart(
            string message = "started.",
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = -1)
        {
            if (!s_isInitialized)
            {
                return;
            }
            string fileName = Path.GetFileName(filePath);
            Log.Verbose($"|{fileName}|{memberName}|{lineNumber}|{message}");
        }
    }
}
