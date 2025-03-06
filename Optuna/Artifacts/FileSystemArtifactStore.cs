using System;
using System.IO;

using Optuna.Trial;

using Python.Runtime;

namespace Optuna.Artifacts
{
    public class FileSystemArtifactStoreWrapper
    {
        public dynamic PyInstance { get; }
        private readonly dynamic _optuna;

        public FileSystemArtifactStoreWrapper(string storeBasePath)
        {
            if (string.IsNullOrEmpty(storeBasePath))
            {
                throw new ArgumentException("Path is null or empty");
            }
            if (!Directory.Exists(storeBasePath))
            {
                Directory.CreateDirectory(storeBasePath);
            }
            _optuna = Py.Import("optuna");
            PyInstance = _optuna.artifacts.FileSystemArtifactStore(base_path: storeBasePath);
        }

        public string UploadArtifact(string filePath, TrialWrapper trial)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("filePath is null or empty");
            }
            if (!File.Exists(filePath))
            {
                throw new ArgumentException("filePath does not exist");
            }
            return _optuna.artifacts.upload_artifact(
                artifact_store: PyInstance,
                file_path: filePath,
                study_or_trial: trial.PyInstance
            );
        }
    }
}
