using System;
using System.IO;
using System.Windows.Forms;

using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;

using Tunny.Core.Settings;
using Tunny.Core.Util;
using Tunny.Eto.Common;
using Tunny.Eto.Message;
using Tunny.Eto.Views;
using Tunny.Resources;

using EtoForms = Eto.Forms;

namespace Tunny.Component.LoadingInstruction
{
    public class RegisterTunnyToolbarItems : GH_AssemblyPriority, IDisposable
    {
        private ToolStripMenuItem _tutorialStripMenuItem;
        private ToolStripMenuItem _tunnyHelpStripMenuItem;
        private ToolStripMenuItem _optunaDashboardToolStripMenuItem;
        private ToolStripMenuItem _ttDesignExplorerToolStripMenuItem;
        private ToolStripMenuItem _aboutTunnyStripMenuItem;
        private static readonly string[] sqliteExtensions = new[] { ".db", ".sqlite" };

        public override GH_LoadingInstruction PriorityLoad()
        {
            InitializeTunnyMenuItem();

            return GH_LoadingInstruction.Proceed;
        }

        private void InitializeTunnyMenuItem()
        {
            try
            {
                Grasshopper.Instances.ComponentServer.AddCategoryIcon("Tunny", Resource.TunnyIcon);
                Grasshopper.Instances.ComponentServer.AddCategorySymbolName("Tunny", 'T');
                Grasshopper.Instances.CanvasCreated += RegisterTunnyMenuItems;
            }
            catch (Exception e)
            {
                TLog.Error($"Register Tunny Menu Items error: {e.Message}: {e.StackTrace}");
            }
        }

        void RegisterTunnyMenuItems(GH_Canvas canvas)
        {
            TLog.MethodStart();
            Grasshopper.Instances.CanvasCreated -= RegisterTunnyMenuItems;

            GH_DocumentEditor docEditor = Grasshopper.Instances.DocumentEditor;
            if (docEditor != null)
            {
                SetupTunnyMenu(docEditor);
            }
        }

        private void SetupTunnyMenu(GH_DocumentEditor docEditor)
        {
            TLog.MethodStart();
            ToolStripMenuItem tunnyToolStripMenuItem;
            tunnyToolStripMenuItem = new ToolStripMenuItem();

            docEditor.MainMenuStrip.SuspendLayout();

            docEditor.MainMenuStrip.Items.AddRange(new ToolStripItem[] {
                tunnyToolStripMenuItem
            });

            tunnyToolStripMenuItem.Name = "TunnyToolStripMenuItem";
            tunnyToolStripMenuItem.Size = new System.Drawing.Size(125, 29);
            tunnyToolStripMenuItem.Text = "Tunny";
            AddTunnyMenuItems(tunnyToolStripMenuItem.DropDownItems);

            docEditor.MainMenuStrip.ResumeLayout(false);
            docEditor.MainMenuStrip.PerformLayout();

            GH_DocumentEditor.AggregateShortcutMenuItems += GH_DocumentEditor_AggregateShortcutMenuItems;
        }

        void GH_DocumentEditor_AggregateShortcutMenuItems(object sender, GH_MenuShortcutEventArgs e)
        {
            e.AppendItem(_optunaDashboardToolStripMenuItem);
        }

        private void AddTunnyMenuItems(ToolStripItemCollection dropDownItems)
        {
            TLog.MethodStart();
            _tunnyHelpStripMenuItem = new ToolStripMenuItem("Help", null, null, "TunnyHelpStripMenuItem");
            _tutorialStripMenuItem = new ToolStripMenuItem("Tutorial Files", null, null, "TutorialStripMenuItem");
            _optunaDashboardToolStripMenuItem = new ToolStripMenuItem("Run Optuna Dashboard...", Resource.optuna_dashboard, OptunaDashboardToolStripMenuItem_Click, "OptunaDashboardToolStripMenuItem");
            _ttDesignExplorerToolStripMenuItem = new ToolStripMenuItem("Run TT DesignExplorer...", Resource.TTDesignExplorer, TTDesignExplorerToolStripMenuItem_Click, "TTDesignExplorerToolStripMenuItem");
            _aboutTunnyStripMenuItem = new ToolStripMenuItem("About...", Resource.TunnyIcon, AboutTunnyStripMenuItem_Click, "AboutTunnyStripMenuItem");

            SetTutorialDropDownItems();

            dropDownItems.AddRange(new ToolStripItem[] {
                _tunnyHelpStripMenuItem,
                _tutorialStripMenuItem,
                new ToolStripSeparator(),
                _optunaDashboardToolStripMenuItem,
                _ttDesignExplorerToolStripMenuItem,
                new ToolStripSeparator(),
                _aboutTunnyStripMenuItem
            });
        }

        private void SetTutorialDropDownItems()
        {
            TLog.MethodStart();
            if (Directory.Exists(Path.Combine(TEnvVariables.ExampleDirPath, "Optimization")))
            {
                var optExample = new ToolStripMenuItem("Optimization", null, null, "TutorialOptimizationStripMenuItem");
                string[] optFiles = Directory.GetFiles(Path.Combine(TEnvVariables.ExampleDirPath, "Optimization"), "*.gh");
                SetMenuItemsFromFilePath(optExample, optFiles);
                _tutorialStripMenuItem.DropDownItems.Add(optExample);
            }
            if (Directory.Exists(Path.Combine(TEnvVariables.ExampleDirPath, "Human-in-the-loop")))
            {
                var hitlExample = new ToolStripMenuItem("Human-in-the-loop", null, null, "TutorialHITLStripMenuItem");
                string[] hitlFiles = Directory.GetFiles(Path.Combine(TEnvVariables.ExampleDirPath, "Human-in-the-loop"), "*.gh");
                SetMenuItemsFromFilePath(hitlExample, hitlFiles);
                _tutorialStripMenuItem.DropDownItems.Add(hitlExample);
            }
        }

        private static void SetMenuItemsFromFilePath(ToolStripMenuItem menuItem, string[] filePaths)
        {
            TLog.MethodStart();
            for (int i = 0; i < filePaths.Length; i++)
            {
                string file = filePaths[i];
                string fileName = Path.GetFileNameWithoutExtension(file);
                var optItem = new ToolStripMenuItem("0" + i + " " + fileName, null, (sender, e) =>
                {
                    Grasshopper.Instances.DocumentServer.AddDocument(file, makeActive: true);
                }, fileName);
                menuItem.DropDownItems.Add(optItem);
            }
        }

        private void AboutTunnyStripMenuItem_Click(object sender, EventArgs e)
        {
            TLog.MethodStart();
            TLog.Debug("AboutTunnyStripMenuItem Clicked");
            TunnyMessageBox.Info_About();
        }

        private void OptunaDashboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TLog.MethodStart();
            TLog.Debug("OptunaDashboardToolStripMenuItem Clicked");
            string pythonDirectory = Path.Combine(TEnvVariables.TunnyEnvPath, "python");
            string dashboardPath = Path.Combine(pythonDirectory, "Scripts", "optuna-dashboard.exe");

            if (!Directory.Exists(pythonDirectory) && !File.Exists(dashboardPath))
            {
                TunnyMessageBox.Info_OptunaDashboardAlreadyInstalled();
            }
            else
            {
                RunOptunaDashboard(dashboardPath);
            }
        }

        private static void RunOptunaDashboard(string dashboardPath)
        {
            TLog.MethodStart();
            string settingsPath = TEnvVariables.OptimizeSettingsPath;
            string storagePath = string.Empty;
            if (File.Exists(settingsPath))
            {
                var settings = TSettings.Deserialize(settingsPath);
                storagePath = settings.Storage.Path;
            }
            var ofd = new EtoForms.OpenFileDialog
            {
                Title = @"Set Tunny result file path",
                FileName = Path.GetFileName(storagePath),
                Filters = {
                    new EtoForms.FileFilter("Journal Storage", ".log"),
                    new EtoForms.FileFilter("SQLite Storage", sqliteExtensions),
                }
            };
            if (ofd.ShowDialog(null) == EtoForms.DialogResult.Ok)
            {
                var dashboard = new Optuna.Dashboard.Handler(dashboardPath, ofd.FileName);
                dashboard.Run(true);
            }
        }

        private void TTDesignExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TLog.MethodStart();
            string settingsPath = TEnvVariables.OptimizeSettingsPath;
            string storagePath = string.Empty;
            TSettings settings;
            if (File.Exists(settingsPath))
            {
                settings = TSettings.Deserialize(settingsPath);
                storagePath = settings.Storage.Path;
            }
            else
            {
                settings = new TSettings();
            }
            var ofd = new EtoForms.OpenFileDialog
            {
                Title = @"Set Tunny result file path",
                FileName = Path.GetFileName(storagePath),
                Filters = {
                    new EtoForms.FileFilter("Journal Storage", ".log"),
                    new EtoForms.FileFilter("SQLite Storage", sqliteExtensions),
                },
            };
            if (ofd.ShowDialog(null) == EtoForms.DialogResult.Ok)
            {
                settings.Storage.Path = ofd.FileName;
                CommonSharedItems.Instance.Settings = settings;
                var deStudyNameSelector = new TargetStudyNameSelector();
                deStudyNameSelector.ShowModal();
            }
        }

        public void Dispose()
        {
            TLog.MethodStart();
            _tunnyHelpStripMenuItem.Dispose();
            _tutorialStripMenuItem.Dispose();
            _optunaDashboardToolStripMenuItem.Dispose();
            _ttDesignExplorerToolStripMenuItem.Dispose();
            _aboutTunnyStripMenuItem.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
