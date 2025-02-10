using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

using CefSharp;
using CefSharp.Wpf;

using Prism.Commands;
using Prism.Mvvm;

using Tunny.CommonUI.Message;
using Tunny.Core.Settings;
using Tunny.Process;
using Tunny.WPF.Common;
using Tunny.WPF.Views.Pages.Visualize;

namespace Tunny.WPF.ViewModels.Visualize
{
    internal sealed class VisualizeViewModel : BindableBase
    {
        private readonly TSettings _settings;
        private VisualizeType _targetVisualizeType;
        private Dictionary<VisualizeType, Lazy<Page>> _plotSettingPages;
        private Lazy<ChromiumWebBrowser> _plotFrame;
        public ChromiumWebBrowser PlotFrame { get => _plotFrame.Value; set => SetProperty(ref _plotFrame, new Lazy<ChromiumWebBrowser>(() => value)); }
        private Page _plotSettingsFrame;
        public Page PlotSettingsFrame { get => _plotSettingsFrame; set => SetProperty(ref _plotSettingsFrame, value); }

        public VisualizeViewModel()
        {
            _settings = SharedItems.Instance.Settings;
            _plotFrame = new Lazy<ChromiumWebBrowser>();
            InitializeSettingPages();
        }

        private void InitializeSettingPages()
        {
            _plotSettingPages = new Dictionary<VisualizeType, Lazy<Page>>
            {
                {VisualizeType.ParetoFront, new Lazy<Page>(() => new ParetoFrontPage())},
                {VisualizeType.OptimizationHistory, new Lazy<Page>(() => new OptimizationHistoryPage())},
                {VisualizeType.Slice, new Lazy<Page>(() => new SlicePage())},
                {VisualizeType.Contour, new Lazy<Page>(() => new ContourPage())},
                {VisualizeType.ParamImportances, new Lazy<Page>(() => new ParamImportancesPage())},
                {VisualizeType.ParallelCoordinate, new Lazy<Page>(() => new ParallelCoordinatePage())},
                {VisualizeType.Hypervolume, new Lazy<Page>(() => new HypervolumePage())},
                {VisualizeType.EDF, new Lazy<Page>(() => new EdfPage())},
                {VisualizeType.Rank, new Lazy<Page>(() => new RankPage())},
                {VisualizeType.Timeline, new Lazy<Page>(() => new TimelinePage())},
                {VisualizeType.TerminatorImprovement, new Lazy<Page>(() => new TerminatorImprovementPage())}
            };
            PlotSettingsFrame = _plotSettingPages[VisualizeType.ParetoFront].Value;
        }

        internal void SetTargetVisualizeType(VisualizeType visualizeType)
        {
            _targetVisualizeType = visualizeType;
            PlotSettingsFrame = _plotSettingPages[_targetVisualizeType].Value;
            var frameViewModel = (PlotSettingsViewModelBase)PlotSettingsFrame.DataContext;
            frameViewModel.UpdateItems();

            ShowInitialView();
        }

        private void ShowInitialView()
        {
            string resource = "Tunny.WPF.Assets.html.visualize_top.html";
            string content = LoadEmbeddedHtml(resource);
            PlotFrame.LoadHtml(content);
        }

        private static string LoadEmbeddedHtml(string resource)
        {
            string content = string.Empty;
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
            {
                if (stream != null)
                {
                    content = new StreamReader(stream).ReadToEnd();
                }
            }

            return content;
        }

        private DelegateCommand _plotCommand;
        public ICommand PlotCommand
        {
            get
            {
                if (_plotCommand == null)
                {
                    _plotCommand = new DelegateCommand(Plot);
                }

                return _plotCommand;
            }
        }
        private void Plot()
        {
            SetLoadingPage();
            switch (_targetVisualizeType)
            {
                case VisualizeType.OptunaHub:
                    break;
                default:
                    PlotAsync();
                    break;
            }
        }

        private void SetLoadingPage()
        {
            string resource = "Tunny.WPF.Assets.html.loading.html";
            string content = LoadEmbeddedHtml(resource);
            PlotFrame.LoadHtml(content);
        }

        private async void PlotAsync()
        {
            var viewModel = (IPlotSettings)PlotSettingsFrame.DataContext;
            if (!viewModel.TryGetPlotSettings(out PlotSettings plotSettings))
            {
                TunnyMessageBox.Error_PlotParameterSet();
                ShowInitialView();
                return;
            }
            await Task.Run(() =>
            {
                string htmlPath = VisualizeProcess.Plot(_settings.Storage, plotSettings);
                string fileUrl = "file:///" + htmlPath.Replace("\\", "/");
                PlotFrame.Load(fileUrl);
            });
        }

        private DelegateCommand _saveCommand;
        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new DelegateCommand(Save);
                }
                return _saveCommand;
            }
        }
        private async void Save()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "fish",
                DefaultExt = "html",
                Filter = @"html(*.html)|*.html|All Files (*.*)|*.*",
                Title = @"Set File Path",
            };

            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                var viewModel = (IPlotSettings)PlotSettingsFrame.DataContext;
                if (!viewModel.TryGetPlotSettings(out PlotSettings plotSettings))
                {
                    TunnyMessageBox.Error_PlotParameterSet();
                    ShowInitialView();
                    return;
                }
                await Task.Run(() => VisualizeProcess.Save(_settings.Storage, plotSettings, dialog.FileName));
            }
        }

        internal void UpdateExistStudySummaries()
        {
            foreach (Lazy<Page> page in _plotSettingPages.Values)
            {
                if (page.IsValueCreated)
                {
                    var viewModel = page.Value.DataContext as PlotSettingsViewModelBase;
                    viewModel.UpdateItems();
                }
            }
        }
    }
}
