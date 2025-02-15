using System;
using System.ComponentModel;
using System.IO;

using Eto.Forms;
using Eto.Serialization.Xaml;

using Tunny.Component.Optimizer;
using Tunny.Core.Handler;
using Tunny.Core.Settings;
using Tunny.Core.Util;
using Tunny.Eto.Common;
using Tunny.Eto.Message;

namespace Tunny.Eto.Views
{
    public class EtoMainWindow : Form
    {
        public ComboBox SamplerComboBox { get; set; }
        public Button RunButton { get; set; }
        public Button StopButton { get; set; }
        public ProgressBar ProgressBar { get; set; }
        public Button PlotButton { get; set; }
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

            LoadComplete += EtoMainWindow_LoadComplete;
            Closing += EtoMainWindow_Closing;
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
        }

        private async void RunButton_Click(object sender, EventArgs e)
        {
            int selectedAlgorithm = SamplerComboBox.SelectedIndex;

            RunButton.Enabled = false;
            StopButton.Enabled = true;
            SamplerComboBox.Enabled = false;
            ProgressBar.Value = 0;

            CoSharedItems.AddProgress(CreateProgressAction());

            CoSharedItems.Settings.Optimize.SamplerType = Core.TEnum.SamplerType.TPE;
            await EtoOptimizeProcess.RunAsync(this);

            UpdateUIStates();
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
            UpdateUIStates();
        }

        private void UpdateUIStates()
        {
            RunButton.Enabled = true;
            StopButton.Enabled = false;
            SamplerComboBox.Enabled = true;
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

            var dashboard = new Optuna.Dashboard.Handler(dashboardPath, storagePath);
            dashboard.Run(true);
        }

        public void UpdateProgress(int value)
        {
            Application.Instance.Invoke(() =>
            {
                ProgressBar.Value = value;
            });
        }
    }
}
