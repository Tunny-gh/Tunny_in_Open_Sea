using System;
using System.Diagnostics;
using System.IO;

using Optuna.Pruner;

using Python.Runtime;

using Tunny.Core.TEnum;
using Tunny.Core.Util;

namespace Tunny.Core.Settings
{
    [LoggingAspect]
    public class Pruner : IDisposable
    {
        public PrunerType Type { get; set; } = PrunerType.None;
        public bool IsEnabled { get; set; }
        public int EvaluateIntervalSeconds { get; set; }
        public bool IsWatcher { get; set; }
        public string ReporterPath { get; set; } = string.Empty;
        public string ReportFilePath { get; set; } = string.Empty;
        public string StopperPath { get; set; } = string.Empty;
        public string[] ReporterInput { get; set; } = Array.Empty<string>();
        public string[] StopperInput { get; set; } = Array.Empty<string>();
        public HyperbandPruner Hyperband { get; set; } = new HyperbandPruner();
        public MedianPruner Median { get; set; } = new MedianPruner();
        public NopPruner Nop { get; set; } = new NopPruner();
        public PatientPruner Patient { get; set; } = new PatientPruner();
        public PercentilePruner Percentile { get; set; } = new PercentilePruner();
        public SuccessiveHalvingPruner SuccessiveHalving { get; set; } = new SuccessiveHalvingPruner();
        public ThresholdPruner Threshold { get; set; } = new ThresholdPruner();
        public WilcoxonPruner Wilcoxon { get; set; } = new WilcoxonPruner();

        private Process _reporter;
        private PrunerStatus _status = PrunerStatus.None;

        public PrunerStatus GetPrunerStatus()
        {
            return _status;
        }

        public PrunerStatus CheckStatus()
        {
            if (ReporterPath == string.Empty || ReportFilePath == string.Empty || StopperPath == string.Empty)
            {
                _status = PrunerStatus.None;
            }
            else if (!File.Exists(ReporterPath) && !File.Exists(StopperPath))
            {
                _status = PrunerStatus.PathError;
            }
            else
            {
                _status = PrunerStatus.Runnable;
            }
            return _status;
        }

        public void ClearReporter()
        {
            if (_reporter != null)
            {
                _reporter.Kill();
                _reporter.Dispose();
                _reporter = null;
            }
        }

        public PrunerReport Evaluate()
        {
            PrunerReport report = null;
            if (_status != PrunerStatus.Runnable)
            {
                return report;
            }

            if (_reporter == null)
            {
                RunReporterProcess();
            }

            if (!IsWatcher)
            {
                _reporter.WaitForExit();
            }

            if (File.Exists(ReportFilePath))
            {
                report = PrunerReport.Deserialize(ReportFilePath);
                File.Delete(ReportFilePath);
            }

            if (!IsWatcher)
            {
                _reporter = null;
            }

            return report;
        }

        private void RunReporterProcess()
        {
            _reporter = new Process();
            _reporter.StartInfo.FileName = ReporterPath;
            _reporter.StartInfo.Arguments = string.Join(" ", ReporterInput);
            _reporter.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            _reporter.Start();
        }

        public void RunStopperProcess()
        {
            var stopper = new Process();
            stopper.StartInfo.FileName = StopperPath;
            stopper.StartInfo.Arguments = string.Join(" ", StopperInput);
            stopper.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            stopper.Start();
            stopper.WaitForExit();
        }

        public dynamic ToPython()
        {
            dynamic optunaPruner;
            dynamic optuna = Py.Import("optuna");
            switch (Type)
            {
                case PrunerType.Hyperband:
                    optunaPruner = Hyperband.ToPython(optuna);
                    break;
                case PrunerType.Median:
                    optunaPruner = Median.ToPython(optuna);
                    break;
                case PrunerType.Nop:
                    optunaPruner = Nop.ToPython(optuna);
                    break;
                case PrunerType.Patient:
                    optunaPruner = Patient.ToPython(optuna);
                    break;
                case PrunerType.Percentile:
                    optunaPruner = Percentile.ToPython(optuna);
                    break;
                case PrunerType.SuccessiveHalving:
                    optunaPruner = SuccessiveHalving.ToPython(optuna);
                    break;
                case PrunerType.Threshold:
                    optunaPruner = Threshold.ToPython(optuna);
                    break;
                case PrunerType.Wilcoxon:
                    optunaPruner = Wilcoxon.ToPython(optuna);
                    break;
                case PrunerType.None:
                    optunaPruner = null;
                    break;
                default:
                    throw new ArgumentException("Invalid pruner type.");
            }

            return optunaPruner;
        }

        public void Dispose()
        {
            if (_reporter != null)
            {
                _reporter.Kill();
                _reporter.Dispose();
            }
            GC.SuppressFinalize(this);
        }
    }
}
