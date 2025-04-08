using System;

using Grasshopper.GUI;

using Tunny.Component.Optimizer;
using Tunny.Core.Handler;
using Tunny.Core.Settings;
using Tunny.Core.Util;
using Tunny.Eto.Views;

namespace Tunny.Eto.Common
{
    [LoggingAspect]
    internal sealed class CommonSharedItems
    {
        private static CommonSharedItems s_instance;
        internal static CommonSharedItems Instance => s_instance ?? (s_instance = new CommonSharedItems());


        private CommonSharedItems()
        {
        }

        internal OptimizeComponentBase Component { get; set; }
        internal TSettings Settings { get; set; }
        internal GH_DocumentEditor GH_DocumentEditor { get; set; }
        internal bool IsForcedStopOptimize { get; set; }
        internal EtoMainWindow EtoWindow;

        private IProgress<ProgressState> _progress;

        internal void AddProgress(IProgress<ProgressState> progress)
        {
            _progress = progress;
        }

        internal void ReportProgress(ProgressState progressState)
        {
            _progress?.Report(progressState);
        }

        private void ClearProgress()
        {
            _progress = null;
        }

        internal void Clear()
        {
            Component = null;
            Settings = null;
            GH_DocumentEditor = null;
            ClearProgress();
        }
    }
}
