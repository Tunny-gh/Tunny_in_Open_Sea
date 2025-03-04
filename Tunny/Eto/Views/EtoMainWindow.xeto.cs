using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

using Eto.Forms;
using Eto.Serialization.Xaml;

using Optuna.Study;

using Tunny.Component.Optimizer;
using Tunny.Core.Handler;
using Tunny.Core.Settings;
using Tunny.Core.Storage;
using Tunny.Core.TEnum;
using Tunny.Core.Util;
using Tunny.Eto.Common;
using Tunny.Eto.Message;

namespace Tunny.Eto.Views
{
    public class EtoMainWindow : Form
    {
        public bool IsLoadCorrectly { get; private set; }
        public ComboBox SamplerComboBox { get; set; }
        public Button RunButton { get; set; }
        public Button StopButton { get; set; }
        public ProgressBar ProgressBar { get; set; }
        public Button PlotButton { get; set; }
        public CheckBox ContinueCheckBox { get; set; }
        public NumericStepper NumTrials { get; set; }
        public TextBox StudyName { get; set; }
        public ComboBox ExistStudyComboBox { get; set; }

        private System.Diagnostics.Process _dashboardProcess;
        private static CommonSharedItems CoSharedItems => CommonSharedItems.Instance;

        public EtoMainWindow(OptimizeComponentBase component)
        {
            TLog.MethodStart();
            XamlReader.Load(this);
            CoSharedItems.GH_DocumentEditor.DisableUI();
            CoSharedItems.Component = component;

            if (!TSettings.TryLoadFromJson(out TSettings settings))
            {
                TunnyMessageBox.Warn_SettingsJsonFileLoadFail();
            }
            else
            {
                settings.Storage.Path = TEnvVariables.DefaultStoragePath;
                if (File.Exists(settings.Storage.Path) == false)
                {
                    File.CreateText(settings.Storage.Path).Dispose();
                }
                CoSharedItems.Settings = settings;
            }

            if (File.Exists(TEnvVariables.QuitFishingPath))
            {
                File.Delete(TEnvVariables.QuitFishingPath);
            }

            LoadComplete += EtoMainWindow_LoadComplete;
            Closing += EtoMainWindow_Closing;
            bool result = InstallPython();
            if (!result)
            {
                TunnyMessageBox.Error_RhinoCodePythonNotFound();
                CoSharedItems.GH_DocumentEditor.EnableUI();
                IsLoadCorrectly = false;
            }
            else
            {
                IsLoadCorrectly = true;
            }
        }

        private static bool InstallPython()
        {
            if (!Directory.Exists(TEnvVariables.PythonPath))
            {
                return false;
            }
#if MACOS
            string requirementsPath = Path.Combine(TEnvVariables.ComponentFolder, "Lib", "requirements_mac.txt");
            var installer = new System.Diagnostics.Process();
            installer.StartInfo.FileName = Path.Combine(TEnvVariables.PythonPath, "bin", "pip");
            installer.StartInfo.Arguments = "install -r \"" + requirementsPath + "\"";
            installer.StartInfo.UseShellExecute = false;
            installer.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            installer.Start();
            installer.WaitForExit();
#endif
            return true;
        }

        private void EtoMainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (StopButton.Enabled)
            {
                DialogResult result = MessageBox.Show(
                    "Optimization is running. Do you want to stop it and close the window?",
                    MessageBoxButtons.YesNo,
                    MessageBoxType.Warning
                );

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }

                StopButton_Click(null, null);
            }

            CoSharedItems.GH_DocumentEditor.EnableUI();
        }

        private void EtoMainWindow_LoadComplete(object sender, EventArgs e)
        {
            RunButton.Click += RunButton_Click;
            StopButton.Click += StopButton_Click;
            PlotButton.Click += PlotButton_Click;
            StopButton.Enabled = false;
            SetExistingStudyNamesToComboBox();
        }

        private async void RunButton_Click(object sender, EventArgs e)
        {
            string selectedAlgorithm = SamplerComboBox.SelectedKey;
            SamplerType samplerType;
            switch (selectedAlgorithm)
            {
                case "TPE":
                    samplerType = SamplerType.TPE;
                    break;
                case "NSGAII":
                    samplerType = SamplerType.NSGAII;
                    break;
                case "QMC":
                    samplerType = SamplerType.QMC;
                    break;
                case "Random":
                    samplerType = SamplerType.Random;
                    break;
                default:
                    samplerType = SamplerType.AUTO;
                    break;
            }

            RunButton.Enabled = false;
            StopButton.Enabled = true;
            SamplerComboBox.Enabled = false;
            ProgressBar.Value = 0;

            CoSharedItems.AddProgress(CreateProgressAction());

            TSettings settings = CoSharedItems.Settings;
            settings.Optimize.SamplerType = samplerType;
            settings.Optimize.ContinueStudy = ContinueCheckBox.Checked.Value == true;
            settings.Optimize.NumberOfTrials = (int)NumTrials.Value;
            SetStudyName(settings);

            await EtoOptimizeProcess.RunAsync(this);

            UpdateUIStates();
        }

        private void SetStudyName(TSettings settings)
        {
            string studyName = StudyName.Text;

            if (ContinueCheckBox.Checked.Value == true && ExistStudyComboBox.SelectedValue != null)
            {
                studyName = ExistStudyComboBox.SelectedValue.ToString();
            }

            settings.Optimize.StudyName = studyName.Equals("AUTO", StringComparison.OrdinalIgnoreCase)
                ? "no-name-" + Guid.NewGuid().ToString("D")
                : studyName;
        }

        private Progress<ProgressState> CreateProgressAction()
        {
            return new Progress<ProgressState>(value =>
            {
                TLog.MethodStart();
                CoSharedItems.Component.UpdateGrasshopper(value);
                ProgressBar.Value = value.PercentComplete;
            });
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            TLog.MethodStart();
            CoSharedItems.IsForcedStopOptimize = true;
            FileStream fs = null;
            try
            {
                fs = File.Create(TEnvVariables.QuitFishingPath);
            }
            finally
            {
                fs?.Dispose();
            }
            UpdateUIStates();
        }

        private void UpdateUIStates()
        {
            TLog.MethodStart();
            RunButton.Enabled = true;
            StopButton.Enabled = false;
            SamplerComboBox.Enabled = true;

            SetExistingStudyNamesToComboBox();
        }

        private void SetExistingStudyNamesToComboBox()
        {
            StudySummary[] studySummaries = new StorageHandler().GetStudySummaries(CoSharedItems.Settings.Storage.Path);
            ExistStudyComboBox.Items.Clear();
            foreach (StudySummary studySummary in studySummaries)
            {
                ExistStudyComboBox.Items.Add(studySummary.StudyName);
            }
        }

        private void PlotButton_Click(object sender, EventArgs e)
        {
            TLog.MethodStart();
            if (File.Exists(CommonSharedItems.Instance.Settings.Storage.Path) == false)
            {
                TunnyMessageBox.Error_ResultFileNotExist();
                return;
            }
            string dashboardPath = TEnvVariables.DashboardPath;
            string storagePath = CommonSharedItems.Instance.Settings.Storage.Path;

            _dashboardProcess?.Kill();
            var dashboard = new Optuna.Dashboard.Handler(dashboardPath, storagePath);
            _dashboardProcess = dashboard.Run(true);
        }

        public void UpdateProgress(int value)
        {
            Application.Instance.Invoke(() =>
            {
                ProgressBar.Value = value;
            });
        }

        private void ContinueCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ExistStudyComboBox.Enabled = ContinueCheckBox.Checked.Value == true;
            StudyName.Enabled = ContinueCheckBox.Checked.Value == false;
        }
    }
}
