using System;
using System.Collections.Generic;

using Grasshopper.GUI;

using Optuna.Trial;

using Tunny.Component.Optimizer;
using Tunny.Core.Handler;
using Tunny.Core.Settings;
using Tunny.Core.Util;

namespace Tunny.CommonUI
{
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

        private IProgress<ProgressState> _progress;

        internal void AddProgress(IProgress<ProgressState> progress)
        {
            TLog.MethodStart();
            _progress = progress;
        }

        internal void ReportProgress(ProgressState progressState)
        {
            TLog.MethodStart();
            _progress?.Report(progressState);
        }

        private void ClearProgress()
        {
            TLog.MethodStart();
            _progress = null;
        }

        internal void Clear()
        {
            TLog.MethodStart();
            Component = null;
            Settings = null;
            GH_DocumentEditor = null;
            ClearProgress();
        }
    }
}
