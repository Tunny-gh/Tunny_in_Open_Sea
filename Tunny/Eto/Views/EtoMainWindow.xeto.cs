using System;
using System.IO;

using Eto.Forms;
using Eto.Serialization.Xaml;

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

        public EtoMainWindow()
        {
            XamlReader.Load(this);

            LoadComplete += EtoMainWindow_LoadComplete;
        }

        private void EtoMainWindow_LoadComplete(object sender, EventArgs e)
        {
            RunButton.Click += RunButton_Click;
            StopButton.Click += StopButton_Click;
            PlotButton.Click += PlotButton_Click;

            StopButton.Enabled = false;
        }

        private void RunButton_Click(object sender, EventArgs e)
        {
            // 最適化アルゴリズムの選択を取得
            string selectedAlgorithm = SamplerComboBox.SelectedKey;

            // UIの状態を更新
            RunButton.Enabled = false;
            StopButton.Enabled = true;
            SamplerComboBox.Enabled = false;
            ProgressBar.Value = 0;

            // ここに最適化処理の開始ロジックを実装
            // 例: Task.Run(() => StartOptimization(selectedAlgorithm));
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            // 最適化処理の停止ロジックを実装

            // UIの状態を元に戻す
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
            string dashboardPath = Path.Combine(TEnvVariables.TunnyEnvPath, "python", "Scripts", "optuna-dashboard.exe");
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
