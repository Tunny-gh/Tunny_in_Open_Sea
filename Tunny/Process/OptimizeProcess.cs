using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Optuna.Trial;

using Tunny.Component.Optimizer;
using Tunny.Core.Handler;
using Tunny.Core.Input;
using Tunny.Core.Settings;
using Tunny.Core.TEnum;
using Tunny.Core.Util;
using Tunny.Eto.Common;
using Tunny.Eto.Models;
using Tunny.Input;
using Tunny.PostProcess;

#if NET7_0_MACOS
#else
using Tunny.WPF.Common;
using Tunny.WPF.ViewModels.Optimize;
#endif

namespace Tunny.Process
{
    internal static class OptimizeProcess
    {
        private const string IntermediateValueKey = "intermediate_value_step_";
        internal const string PrunedTrialReportValueKey = "pruned_trial_report_value";
        public static bool IsForcedStopOptimize { get; set; }
        private static CommonSharedItems CoSharedItems => CommonSharedItems.Instance;

#if NET7_0_MACOS
#else
        private static SharedItems SharedItems => SharedItems.Instance;
        internal async static Task RunAsync(OptimizeViewModel optimizeViewModel)
        {
            TLog.MethodStart();
            CoSharedItems.Component?.GhInOutInstantiate();
            SharedItems.OptimizeViewModel = optimizeViewModel;

            ProgressState progressState = await RunOptimizationLoopAsync();
            if (progressState.Parameter == null || progressState.Parameter.Count == 0)
            {
                return;
            }

            if (CoSharedItems.Component != null)
            {
                var tcs = new TaskCompletionSource<bool>();
                void EventHandler(object sender, GrasshopperStates status)
                {
                    if (status == GrasshopperStates.RequestProcessed)
                    {
                        tcs.SetResult(true);
                    }
                }
                CoSharedItems.Component.GrasshopperStatusChanged += EventHandler;
                try
                {
                    CoSharedItems.Component.GrasshopperStatus = GrasshopperStates.RequestSent;
                    await ReportAsync(progressState);
                    await tcs.Task;
                }
                finally
                {
                    CoSharedItems.Component.GrasshopperStatusChanged -= EventHandler;
                }
                SharedItems.UpdateStudySummaries();
            }
        }

        private static async Task ReportAsync(ProgressState progressState)
        {
            TLog.MethodStart();
            await Task.Run(() => CoSharedItems.ReportProgress(progressState));
        }

        private static async Task<ProgressState> RunOptimizationLoopAsync()
        {
            TLog.MethodStart();
            Objective objectives = SetObjectives();
            List<VariableBase> variables = SetVariables();
            bool hasConstraint = CoSharedItems.Component.GhInOut.HasConstraint;
            var progressState = new ProgressState(Array.Empty<Parameter>());

            var optunaSolver = new Solver.Solver(SharedItems.Settings, hasConstraint);
            bool reinstateResult = await Task.Run(() =>
                optunaSolver.RunSolver(variables, objectives, EvaluateFunction)
            );

            return reinstateResult
                ? new ProgressState(optunaSolver.OptimalParameters)
                : new ProgressState(optunaSolver.OptimalParameters, true);
        }

        private static List<VariableBase> SetVariables()
        {
            List<VariableBase> variables = CoSharedItems.Component.GhInOut.Variables;
            int count = 0;
            foreach (VariableBase variable in variables)
            {
                if (variable is NumberVariable numberVariable)
                {
                    numberVariable.IsLogScale = SharedItems.OptimizeViewModel.VariableSettingItems[count].IsLogScale;
                    count++;
                }
            }

            return variables;
        }
#endif

        private static Objective SetObjectives()
        {
            Objective objectives = CoSharedItems.Component.GhInOut.Objectives;
            IEnumerable<ObjectiveSettingItem> settingItems = SharedItems.OptimizeViewModel.ObjectiveSettingItems;
            objectives.SetDirections(settingItems);
            return objectives;
        }

        private static TrialGrasshopperItems EvaluateFunction(ProgressState pState, int progress)
        {
            TLog.MethodStart();
            CoSharedItems.ReportProgress(pState);
            OptimizeComponentBase component = CoSharedItems.Component;
            if (pState.IsReportOnly)
            {
                return null;
            }

            component.GrasshopperStatus = GrasshopperStates.RequestSent;

            int step = 0;
            DateTime timer = DateTime.Now;
            while (component.GrasshopperStatus != GrasshopperStates.RequestProcessed)
            {
                PrunerProgress(pState, ref step, ref timer);
            }
            pState.Pruner.ClearReporter();

            return new TrialGrasshopperItems
            {
                Objectives = component.GhInOut.Objectives,
                Attributes = component.GhInOut.Attributes,
                Artifacts = component.GhInOut.Artifacts,
            };
        }

        private static void PrunerProgress(ProgressState pState, ref int step, ref DateTime timer)
        {
            if (CommonSharedItems.Instance.Settings.Pruner.IsEnabled
                && pState.Pruner.GetPrunerStatus() == PrunerStatus.Runnable
                && DateTime.Now - timer > TimeSpan.FromSeconds(pState.Pruner.EvaluateIntervalSeconds))
            {
                step = ReportPruner(pState.TrialWrapper, step, pState.Pruner);
                timer = DateTime.Now;
            }
        }

        private static int ReportPruner(TrialWrapper trial, int step, Pruner pruner)
        {
            PrunerReport report = pruner.Evaluate();
            if (report == null)
            {
                return step;
            }
            else
            {
                trial.Report(report.IntermediateValue, step);
                if (!string.IsNullOrEmpty(report.Attribute))
                {
                    trial.SetUserAttribute(IntermediateValueKey + step, report.Attribute);
                }

                if (trial.ShouldPrune())
                {
                    pruner.RunStopperProcess();
                    if (report.TrialTellValue.HasValue)
                    {
                        trial.SetUserAttribute(PrunedTrialReportValueKey, report.TrialTellValue.Value);
                    }
                }

                return step + 1;
            }
        }
    }
}
