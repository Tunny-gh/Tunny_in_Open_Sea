using System;
using System.Collections.Generic;
using System.IO;

using Eto.Forms;

using Tunny.Component.Optimizer;
using Tunny.Core.Handler;
using Tunny.Core.Input;
using Tunny.Core.Settings;
using Tunny.Core.TEnum;
using Tunny.Core.Util;
using Tunny.Eto.Common;
using Tunny.Eto.Message;
using Tunny.Input;
using Tunny.PostProcess;

namespace Tunny.Solver
{
    [LoggingAspect]
    public class Solver
    {
        public Parameter[] OptimalParameters { get; private set; }
        private readonly bool _hasConstraint;
        private readonly TSettings _settings;
        private static CommonSharedItems CoSharedItems => CommonSharedItems.Instance;
        private const string CompleteMessagePrefix = "Solver completed successfully.";
        private const string ErrorMessagePrefix = "Solver error.";

        public Solver(TSettings settings, bool hasConstraint)
        {
            _settings = settings;
            _hasConstraint = hasConstraint;
        }

        public bool RunSolver(
            List<VariableBase> variables,
            Objective objectives,
            Func<ProgressState, int, TrialGrasshopperItems> evaluate)
        {
            TrialGrasshopperItems Eval(ProgressState pState, int progress)
            {
                return evaluate(pState, progress);
            }

            try
            {
                InitializeTmpDir();
                var optimize = new Algorithm(variables, _hasConstraint, objectives, _settings, Eval);
                optimize.Solve();
                OptimalParameters = optimize.OptimalParameters;
                DialogResult msgResult = EndMessage(optimize, objectives.Length > 1);

                return msgResult == DialogResult.Yes;
            }
            catch (Exception e)
            {
                ShowErrorMessages(e);
                return false;
            }
        }

        private static void InitializeTmpDir()
        {
            if (!Directory.Exists(TEnvVariables.TmpDirPath))
            {
                TLog.Info("Create tmp folder");
                Directory.CreateDirectory(TEnvVariables.TmpDirPath);
            }
            else
            {
                TLog.Info("Start clean tmp files");
                var tmpDir = new DirectoryInfo(TEnvVariables.TmpDirPath);
                foreach (FileInfo file in tmpDir.GetFiles())
                {
                    try
                    {
                        file.Delete();
                    }
                    catch (Exception e)
                    {
                        TLog.Error(e.Message);
                    }
                }
                TLog.Info("Finish clean tmp files");
            }
        }

        private static DialogResult EndMessage(Algorithm optimize, bool isMultiObjective)
        {
            DialogResult msgResult = DialogResult.None;
            ToComponentEndMessage(optimize);
            if (CoSharedItems.Component is UIOptimizeComponentBase)
            {
                msgResult = ShowUIEndMessages(optimize.EndState, isMultiObjective);
            }
            return msgResult;
        }

        private static void ToComponentEndMessage(Algorithm optimize)
        {
            string message;
            switch (optimize.EndState)
            {
                case EndState.Timeout:
                    message = CompleteMessagePrefix + " The specified time has elapsed.";
                    break;
                case EndState.AllTrialCompleted:
                    message = CompleteMessagePrefix + " The specified number of trials has been completed.";
                    break;
                case EndState.StoppedByUser:
                    message = CompleteMessagePrefix + " The user stopped the solver.";
                    break;
                case EndState.StoppedByOptuna:
                    message = CompleteMessagePrefix + " The Optuna stopped the solver.";
                    break;
                case EndState.DirectionNumNotMatch:
                    message = ErrorMessagePrefix + " The number of Objective in the existing Study does not match the one that you tried to run; Match the number of objective, or change the \"Study Name\".";
                    break;
                case EndState.UseExitStudyWithoutContinue:
                    message = ErrorMessagePrefix + " \"Load if study file exists\" was false even though the same \"Study Name\" exists. Please change the name or set it to true.";
                    break;
                default:
                    message = ErrorMessagePrefix;
                    break;
            }
            if (CoSharedItems.Component is BoneFishComponent)
            {
                TLog.Info(message);
            }
            CoSharedItems.Component.SetInfo(message);
        }

        private static DialogResult ShowUIEndMessages(EndState endState, bool isMultiObjective)
        {
            DialogResult msgResult;
            MessageBoxButtons button = isMultiObjective ? MessageBoxButtons.OK : MessageBoxButtons.YesNo;
            string reinstateMessage = isMultiObjective ? string.Empty : "\nReinstate the best trial to the slider?\n\nIf reinstated, The components connected to the sliders are recomputed as the sliders are updated.\n";
            switch (endState)
            {
                case EndState.Timeout:
                    msgResult = TunnyMessageBox.Show(CompleteMessagePrefix + "\n\nThe specified time has elapsed." + reinstateMessage, "Tunny", button);
                    break;
                case EndState.AllTrialCompleted:
                    msgResult = TunnyMessageBox.Show(CompleteMessagePrefix + "\n\nThe specified number of trials has been completed." + reinstateMessage, "Tunny", button);
                    break;
                case EndState.StoppedByUser:
                    msgResult = TunnyMessageBox.Show(CompleteMessagePrefix + "\n\nThe user stopped the solver." + reinstateMessage, "Tunny", button);
                    break;
                case EndState.StoppedByOptuna:
                    msgResult = TunnyMessageBox.Show(CompleteMessagePrefix + "\n\nThe Optuna stopped the solver." + reinstateMessage, "Tunny", button);
                    break;
                case EndState.DirectionNumNotMatch:
                    msgResult = TunnyMessageBox.Show(ErrorMessagePrefix + "\n\nThe number of Objective in the existing Study does not match the one that you tried to run; Match the number of objective, or change the \"Study Name\".", "Tunny", MessageBoxButtons.OK, MessageBoxType.Error);
                    break;
                case EndState.UseExitStudyWithoutContinue:
                    msgResult = TunnyMessageBox.Show(ErrorMessagePrefix + "\n\n\"Load if study file exists\" was false even though the same \"Study Name\" exists. Please change the name or set it to true.", "Tunny", MessageBoxButtons.OK, MessageBoxType.Error);
                    break;
                case EndState.Error:
                    msgResult = TunnyMessageBox.Show(ErrorMessagePrefix, "Tunny");
                    break;
                default:
                    msgResult = TunnyMessageBox.Show(ErrorMessagePrefix + "\n\n Unexpected exception.", "Tunny");
                    break;
            }
            return msgResult;
        }

        private static void ShowErrorMessages(Exception e)
        {
            TunnyMessageBox.Show(
                "Tunny runtime error:\n" +
                "Please send below message (& gh file if possible) to Tunny support.\n" +
                "If this error occurs, the Tunny solver will not work after this unless Rhino is restarted.\n\n" +
                "Source: " + e.Source + " \n" +
                "Message: " + e.Message,
                "Tunny",
                MessageBoxButtons.OK,
                MessageBoxType.Error);
        }
    }
}
