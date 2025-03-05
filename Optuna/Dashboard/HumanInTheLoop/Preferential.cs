using System;
using System.Drawing;

using Optuna.Study;
using Optuna.Trial;

using Python.Runtime;

namespace Optuna.Dashboard.HumanInTheLoop
{
    public class Preferential : HumanInTheLoopBase
    {
        private readonly PyModule _importedLibrary;
        private readonly dynamic _artifactBackend;
        private readonly dynamic _sampler;
        private string _userAttrKey;

        public Preferential(string tmpPath, string storagePath, dynamic sampler) : base(tmpPath, storagePath)
        {
            PyModule importedLibrary = ImportBaseLibrary();
            importedLibrary.Exec("from optuna_dashboard.artifact.file_system import FileSystemBackend");
            importedLibrary.Exec("from optuna_dashboard.artifact import upload_artifact");
            importedLibrary.Exec("from optuna_dashboard import register_preference_feedback_component");
            importedLibrary.Exec("from optuna_dashboard.preferential import create_study");

            dynamic fileSystemBackend = importedLibrary.Get("FileSystemBackend");
            _artifactBackend = fileSystemBackend(base_path: _artifactPath);
            _importedLibrary = importedLibrary;
            _sampler = sampler;
        }

        public StudyWrapper CreateStudy(int nGenerate, string studyName, dynamic storage, string objectiveName)
        {
            dynamic createStudy = _importedLibrary.Get("create_study");
            string name = studyName == null || studyName == "" ? "no-name-" + Guid.NewGuid().ToString("D") : studyName;
            dynamic study = createStudy(
                n_generate: nGenerate,
                study_name: name,
                sampler: _sampler,
                storage: storage,
                load_if_exists: true
            );
            dynamic registerPreferenceFeedbackComponent = _importedLibrary.Get("register_preference_feedback_component");
            _userAttrKey = objectiveName;
            registerPreferenceFeedbackComponent(study, "artifact", objectiveName);
            return new StudyWrapper(study);
        }

        public void UploadArtifact(TrialWrapper trial, Bitmap image)
        {
            dynamic uploadArtifact = _importedLibrary.Get("upload_artifact");
            CheckDirectoryIsExist();
            string path = $"{_tmpPath}/image_{trial.Number}.png";
            image.Save(path, System.Drawing.Imaging.ImageFormat.Png);
            dynamic artifactId = uploadArtifact(_artifactBackend, trial.PyInstance, path);
            trial.SetUserAttribute(_userAttrKey, artifactId);
        }
    }
}
