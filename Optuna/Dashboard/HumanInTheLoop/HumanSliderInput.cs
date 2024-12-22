using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

using Optuna.Trial;

using Python.Runtime;

namespace Optuna.Dashboard.HumanInTheLoop
{
    public class HumanSliderInput : HumanInTheLoopBase
    {
        private readonly PyModule _importedLibrary;
        private readonly dynamic _artifactBackend;

        public HumanSliderInput(string tmpPath, string storagePath) : base(tmpPath, storagePath)
        {
            PyModule importedLibrary = ImportBaseLibrary();
            importedLibrary.Exec("from optuna_dashboard import save_note");
            importedLibrary.Exec("from optuna_dashboard import SliderWidget");
            importedLibrary.Exec("from optuna_dashboard import ObjectiveUserAttrRef");
            importedLibrary.Exec("from optuna_dashboard import register_objective_form_widgets");
            importedLibrary.Exec("from optuna_dashboard import set_objective_names");
            importedLibrary.Exec("from optuna_dashboard.artifact import upload_artifact");
            importedLibrary.Exec("from optuna_dashboard.artifact.file_system import FileSystemBackend");

            dynamic fileSystemBackend = importedLibrary.Get("FileSystemBackend");
            _artifactBackend = fileSystemBackend(base_path: _artifactPath);
            _importedLibrary = importedLibrary;
        }

        public void SetObjective(dynamic study, string[] objectiveNames)
        {
            dynamic setObjectiveNames = _importedLibrary.Get("set_objective_names");
            PyList pyNameList = EnumeratorToPyList(objectiveNames.Select(s => s.Replace("Human-in-the-Loop", "HITL")));
            setObjectiveNames(study, pyNameList);
        }

        public void SetWidgets(dynamic study, string[] objectiveNames)
        {
            dynamic registerObjectiveFromWidgets = _importedLibrary.Get("register_objective_form_widgets");
            dynamic sliderWidget = _importedLibrary.Get("SliderWidget");
            dynamic objectiveUserAttrRef = _importedLibrary.Get("ObjectiveUserAttrRef");

            var widgets = new dynamic[objectiveNames.Length];
            for (int i = 0; i < objectiveNames.Length; i++)
            {
                if (objectiveNames[i].Contains("Human-in-the-Loop"))
                {
                    widgets[i] = sliderWidget(min: 1, max: 5, step: 1, description: "Smaller is better");
                }
                else
                {
                    string objectiveName = objectiveNames[i];
                    var key = new PyString("result_" + objectiveName);
                    widgets[i] = objectiveUserAttrRef(key: key);
                }
            }
            registerObjectiveFromWidgets(study, widgets: widgets);
        }

        public void SaveNote(dynamic study, TrialWrapper trial, Bitmap[] bitmaps)
        {
            dynamic uploadArtifact = _importedLibrary.Get("upload_artifact");
            var noteText = new StringBuilder();
            noteText.AppendLine("# Image");
            noteText.AppendLine("");

            CheckDirectoryIsExist();
            for (int i = 0; i < bitmaps.Length; i++)
            {
                Bitmap bitmap = bitmaps[i];
                string path = $"{_tmpPath}/image_{study._study_id}_{trial.Id}.png";
                bitmap?.Save(path, System.Drawing.Imaging.ImageFormat.Png);
                dynamic artifactId = uploadArtifact(_artifactBackend, trial.PyObject, path);
                noteText.AppendLine($"![](/artifacts/{study._study_id}/{trial.Id}/{artifactId})");
            }

            dynamic textWrap = _importedLibrary.Get("textwrap");
            dynamic note = textWrap.dedent(noteText.ToString());
            dynamic saveNote = _importedLibrary.Get("save_note");
            saveNote(trial.PyObject, note);
        }

        public static PyList EnumeratorToPyList(IEnumerable<string> enumerator)
        {
            var pyList = new PyList();
            foreach (string item in enumerator)
            {
                pyList.Append(new PyString(item));
            }
            return pyList;
        }
    }
}
