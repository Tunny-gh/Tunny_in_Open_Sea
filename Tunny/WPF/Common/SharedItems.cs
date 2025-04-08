using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Optuna.Study;
using Optuna.Trial;

using Tunny.Core.Settings;
using Tunny.Core.Solver;
using Tunny.Core.Storage;
using Tunny.Core.Util;
using Tunny.WPF.Models;
using Tunny.WPF.ViewModels;
using Tunny.WPF.ViewModels.Optimize;

namespace Tunny.WPF.Common
{
    [LoggingAspect]
    internal sealed class SharedItems
    {
        private static SharedItems s_instance;
        internal static SharedItems Instance => s_instance ?? (s_instance = new SharedItems());


        private SharedItems()
        {
        }

        internal MainWindow TunnyWindow { get; set; }
        internal OptimizeViewModel OptimizeViewModel { get; set; }
        internal TSettings Settings { get; set; }
        internal Dictionary<int, Trial[]> Trials { get; set; }
        private StudySummary[] _studySummaries;
        internal StudySummary[] StudySummaries
        {
            get => _studySummaries;
            set
            {
                if (value == null)
                {
                    return;
                }
                _studySummaries = value;
                var output = new Output(Settings.Storage.Path);
                Trials = output.GetAllTrial();
                var windowVM = TunnyWindow.DataContext as MainWindowViewModel;
                windowVM?.UpdateExistStudySummaries();
            }
        }
        internal Dictionary<int, ObservableCollection<OutputTrialItem>> OutputListedTrialDict { get; set; } = new Dictionary<int, ObservableCollection<OutputTrialItem>>();
        internal Dictionary<int, ObservableCollection<OutputTrialItem>> OutputTargetTrialDict { get; set; } = new Dictionary<int, ObservableCollection<OutputTrialItem>>();

        internal void Clear()
        {
            TunnyWindow = null;
            OptimizeViewModel = null;
            StudySummaries = null;
            OutputListedTrialDict.Clear();
            OutputTargetTrialDict.Clear();
        }

        internal OutputTrialItem GetOutputTrial(int studyId, int trialId)
        {
            return new OutputTrialItem
            {
                Id = trialId,
                IsSelected = false,
                Objectives = string.Join(", ", Trials[studyId][trialId].Values),
                Variables = string.Join(", ", Trials[studyId][trialId].Params.Select(p => $"{p.Key}:{p.Value}")),
            };
        }

        internal void UpdateStudySummaries()
        {
            StudySummaries = new StorageHandler().GetStudySummaries(Settings.Storage.Path);
        }
    }
}
