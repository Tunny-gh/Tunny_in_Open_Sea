using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;

using Grasshopper.GUI;

using Optuna.Study;

using Rhino.Display;

using Tunny.Core.Handler;
using Tunny.Core.Storage;
using Tunny.Core.TEnum;
using Tunny.Core.Util;
using Tunny.Process;

namespace Tunny.UI
{
    public partial class OptimizationWindow
    {
        private StudySummary[] _summaries;

        private void InMemoryCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            TLog.MethodStart();
            if (inMemoryCheckBox.Checked)
            {
                continueStudyCheckBox.Checked = false;
                continueStudyCheckBox.Enabled = false;
                copyStudyCheckBox.Checked = false;
                copyStudyCheckBox.Enabled = false;
                studyNameTextBox.Enabled = true;
                _settings.Storage.Type = StorageType.InMemory;
            }
            else
            {
                continueStudyCheckBox.Enabled = true;
                _settings.Storage.Type = Path.GetExtension(_settings.Storage.Path) == ".log" ? StorageType.Journal : StorageType.Sqlite;
            }
        }

        private void ContinueStudyCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            TLog.MethodStart();
            if (continueStudyCheckBox.Checked)
            {
                existingStudyComboBox.Enabled = true;
                copyStudyCheckBox.Enabled = true;
                studyNameTextBox.Enabled = copyStudyCheckBox.Checked;
                inMemoryCheckBox.Enabled = false;
            }
            else
            {
                copyStudyCheckBox.Enabled = false;
                existingStudyComboBox.Enabled = false;
                studyNameTextBox.Enabled = true;
                inMemoryCheckBox.Enabled = true;
            }
        }

        private void CopyStudyCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            TLog.MethodStart();
            studyNameTextBox.Enabled = copyStudyCheckBox.Checked;
        }

        private void OptimizeRunButton_Click(object sender, EventArgs e)
        {
            TLog.MethodStart();
            var ghCanvas = Owner as GH_DocumentEditor;
            ghCanvas?.DisableUI();

            optimizeRunButton.Enabled = false;
            GetUIValues();
            RhinoView.EnableDrawing = !_settings.Optimize.DisableViewportDrawing;
            ShowRealtimeResultCheckBox.Enabled = false;
            OptimizeProcess.Settings = _settings;

            if (!CheckInputValue(ghCanvas))
            {
                return;
            }

            if (optimizeBackgroundWorker.IsBusy)
            {
                optimizeBackgroundWorker.Dispose();
            }
            optimizeBackgroundWorker.RunWorkerAsync(_component);
            optimizeStopButton.Enabled = true;
        }

        private bool CheckInputValue(GH_DocumentEditor ghCanvas)
        {
            TLog.MethodStart();
            bool checkResult = CheckObjectivesCount(ghCanvas);

            switch (checkResult)
            {
                case true when studyNameTextBox.Enabled && existingStudyComboBox.Items.Contains(studyNameTextBox.Text):
                    checkResult = NameAlreadyExistMessage(ghCanvas);
                    break;
                case true when copyStudyCheckBox.Enabled && copyStudyCheckBox.Checked:
                    if (string.IsNullOrEmpty(studyNameTextBox.Text))
                    {
                        _settings.Optimize.StudyName = "no-name-" + Guid.NewGuid().ToString("D");
                    }
                    new StorageHandler().DuplicateStudyInStorage(existingStudyComboBox.Text, _settings.Optimize.StudyName, _settings.Storage);
                    break;
                case true when continueStudyCheckBox.Checked:
                    checkResult = CheckSameStudyName(ghCanvas);
                    break;
                default:
                    break;
            }
            return checkResult;
        }

        private bool CheckSameStudyName(GH_DocumentEditor ghCanvas)
        {
            TLog.MethodStart();
            if (existingStudyComboBox.Text == string.Empty)
            {
                return SameStudyNameMassage(ghCanvas);
            }
            _settings.Optimize.StudyName = existingStudyComboBox.Text;

            return true;
        }

        private bool CheckObjectivesCount(GH_DocumentEditor ghCanvas)
        {
            TLog.MethodStart();
            int length = _component.GhInOut.Objectives.Length;
            if (length == 0)
            {
                ghCanvas.EnableUI();
                optimizeRunButton.Enabled = true;
                return false;
            }
            else if (length > 1 &&
                ((samplerComboBox.SelectedIndex == (int)SamplerType.CmaEs) || samplerComboBox.SelectedIndex == (int)SamplerType.GP)
            )
            {
                return SupportOneObjectiveMessage(ghCanvas);
            }

            return true;
        }

        private void OptimizeStopButton_Click(object sender, EventArgs e)
        {
            TLog.MethodStart();
            optimizeRunButton.Enabled = true;
            optimizeStopButton.Enabled = false;
            OptimizeProcess.IsForcedStopOptimize = true;
            ShowRealtimeResultCheckBox.Enabled = true;
            optimizeBackgroundWorker?.Dispose();

            UpdateStudyComboBox();

            //Enable GUI
            var ghCanvas = Owner as GH_DocumentEditor;
            ghCanvas?.EnableUI();
            if (RhinoView.EnableDrawing == false)
            {
                RhinoView.EnableDrawing = true;
            }
        }

        private void UpdateStudyComboBox()
        {
            TLog.MethodStart();
            try
            {
                UpdateStudyComboBox(_settings.Storage.Path);
            }
            catch (Exception)
            {
                string message = "The storage file loading error.Please check the storage path or use new storage file.";
                TunnyMessageBox.Show(message, "Error");
            }
        }

        private void UpdateStudyComboBox(string storagePath)
        {
            TLog.MethodStart();
            existingStudyComboBox.Items.Clear();
            visualizeTargetStudyComboBox.Items.Clear();
            outputTargetStudyComboBox.Items.Clear();
            cmaEsWarmStartComboBox.Items.Clear();

            TLog.Info($"Get study summaries from storage file: {storagePath}");
            _summaries = new StorageHandler().GetStudySummaries(storagePath);

            if (_summaries.Length > 0)
            {
                string[] studyNames = _summaries.Select(summary => summary.StudyName).ToArray();
                existingStudyComboBox.Items.AddRange(studyNames);
                cmaEsWarmStartComboBox.Items.AddRange(studyNames);
                UpdateExistingStudyComboBox();

                visualizeTargetStudyComboBox.Items.AddRange(studyNames);
                UpdateVisualizeTargetStudyComboBox();

                outputTargetStudyComboBox.Items.AddRange(studyNames);

                if (!_summaries[0].SystemAttrs.ContainsKey("study:metric_names") || !_summaries[0].UserAttrs.ContainsKey("variable_names"))
                {
                    return;
                }

                UpdateVisualizeListBox();
            }
        }

        private void UpdateVisualizeTargetStudyComboBox()
        {
            TLog.MethodStart();
            if (visualizeTargetStudyComboBox.Items.Count > 0 && visualizeTargetStudyComboBox.Items.Count - 1 < visualizeTargetStudyComboBox.SelectedIndex)
            {
                visualizeTargetStudyComboBox.SelectedIndex = 0;
            }
            else if (visualizeTargetStudyComboBox.Items.Count == 0)
            {
                visualizeTargetStudyComboBox.Text = string.Empty;
            }
        }

        private void UpdateExistingStudyComboBox()
        {
            TLog.MethodStart();
            if (existingStudyComboBox.Items.Count > 0 && existingStudyComboBox.Items.Count - 1 < existingStudyComboBox.SelectedIndex)
            {
                existingStudyComboBox.SelectedIndex = 0;
                cmaEsWarmStartComboBox.SelectedIndex = 0;
            }
            else if (existingStudyComboBox.Items.Count == 0)
            {
                existingStudyComboBox.Text = string.Empty;
                continueStudyCheckBox.Checked = false;
                cmaEsWarmStartComboBox.Text = string.Empty;
                cmaEsWarmStartCmaEsCheckBox.Checked = false;
            }
        }

        private void OptimizeProgressChangedHandler(object sender, ProgressChangedEventArgs e)
        {
            TLog.MethodStart();
            var progressState = (ProgressState)e.UserState;
            _component.UpdateGrasshopper(progressState);
            const string trialNumLabel = "Trial: ";
            optimizeTrialNumLabel.Text = e.ProgressPercentage == 100
                ? trialNumLabel + "#"
                : trialNumLabel + (progressState.TrialNumber + 1);
            SetBestValues(e, progressState);

            EstimatedTimeRemainingLabel.Text = progressState.EstimatedTimeRemaining.TotalSeconds > 0
                ? "Estimated Time Remaining: " + new DateTime(0).Add(progressState.EstimatedTimeRemaining).ToString("HH:mm:ss", CultureInfo.InvariantCulture)
                : "Estimated Time Remaining: 00:00:00";
            optimizeProgressBar.Value = e.ProgressPercentage;
            optimizeProgressBar.Update();
        }

        private void SetBestValues(ProgressChangedEventArgs e, ProgressState pState)
        {
            TLog.MethodStart();
            if (pState.BestValues == null)
            {
                optimizeBestValueLabel.Text = @"BestValue: #";
            }
            else if (e.ProgressPercentage == 0 || e.ProgressPercentage == 100)
            {
                optimizeBestValueLabel.Text = pState.ObjectiveNum == 1
                    ? @"BestValue: #"
                    : @"Hypervolume Ratio: #";
            }
            else if (pState.BestValues.Length > 0)
            {
                optimizeBestValueLabel.Text = pState.ObjectiveNum == 1
                    ? $"BestValue: {pState.BestValues[0][0]:e4}"
                    : $"Hypervolume Ratio: {pState.HypervolumeRatio:0.000}";
            }
        }
    }
}
