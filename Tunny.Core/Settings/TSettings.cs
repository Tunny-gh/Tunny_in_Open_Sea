using System;
using System.IO;

using Newtonsoft.Json;

using Serilog.Events;

using Tunny.Core.TEnum;
using Tunny.Core.Util;

namespace Tunny.Core.Settings
{
    [LoggingAspect]
    public class TSettings
    {
        public string Version { get; set; } = TEnvVariables.Version.ToString();
        public Optimize Optimize { get; set; } = new Optimize();
        public Pruner Pruner { get; set; } = new Pruner();
        public Storage Storage { get; set; } = new Storage();
        public bool CheckPythonLibraries { get; set; } = true;
        public LogEventLevel LogLevel { get; set; } = LogEventLevel.Information;

        public TSettings()
        {
        }

        public TSettings(string settingsPath, string storagePath, StorageType storageType, bool createNewFile)
        {
            Storage = new Storage
            {
                Path = storagePath,
                Type = storageType
            };

            if (createNewFile)
            {
                CreateNewSettingsFile(settingsPath);
            }
        }

        public void Serialize(string path)
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            string dirPath = Path.GetDirectoryName(path);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            File.WriteAllText(path, json);
        }

        public static TSettings Deserialize(string settingsPath)
        {
            try
            {
                return JsonConvert.DeserializeObject<TSettings>(File.ReadAllText(settingsPath));
            }
            catch (Exception e)
            {
                TLog.Error(e.Message);
                TLog.Warning("Create new settings.json");
                File.Delete(settingsPath);
                return new TSettings(settingsPath, TEnvVariables.DefaultStoragePath, StorageType.Journal, true);
            }
        }

        public void CreateNewSettingsFile(string path)
        {
            Serialize(path);
        }

        public static bool TryLoadFromJson(out TSettings settings)
        {
            string settingsPath = TEnvVariables.OptimizeSettingsPath;
            if (File.Exists(settingsPath))
            {
                TLog.Info("Load existing setting.json");
                settings = Deserialize(settingsPath);
                return true;
            }
            else
            {
                settings = new TSettings(TEnvVariables.OptimizeSettingsPath, TEnvVariables.DefaultStoragePath, StorageType.Journal, true);
                return false;
            }
        }
    }
}
