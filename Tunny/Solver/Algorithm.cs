using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;

using Optuna.Dashboard.HumanInTheLoop;
using Optuna.Study;
using Optuna.Util;

using Python.Runtime;

using Tunny.Core.Handler;
using Tunny.Core.Input;
using Tunny.Core.Settings;
using Tunny.Core.Storage;
using Tunny.Core.TEnum;
using Tunny.Core.Util;
using Tunny.Input;
using Tunny.PostProcess;
using Tunny.Process;
using Tunny.Type;
using Tunny.WPF.Common;

namespace Tunny.Solver
{
    public class Algorithm : PythonInit
    {
        public Parameter[] OptimalParameters { get; private set; }
        public EndState EndState { get; private set; }

        private readonly List<VariableBase> _variables;
        private readonly bool _hasConstraints;
        private readonly Objective _objective;
        private readonly TSettings _settings;
        private readonly Func<ProgressState, int, TrialGrasshopperItems> _evalFunc;
        private readonly List<FishEgg> _fishEgg = SharedItems.Instance.Component.GhInOut.FishEggs;

        public Algorithm(
            List<VariableBase> variables, bool hasConstraint, Objective objective,
            TSettings settings,
            Func<ProgressState, int, TrialGrasshopperItems> evalFunc)
        {
            TLog.MethodStart();
            _variables = variables;
            _hasConstraints = hasConstraint;
            _objective = objective;
            _settings = settings;
            _evalFunc = evalFunc;
        }

        public void Solve()
        {
            TLog.MethodStart();
            EndState = EndState.Error;
            OptimizeProcess.IsForcedStopOptimize = false;
            SamplerType samplerType = _settings.Optimize.SamplerType;
            int nTrials = _settings.Optimize.NumberOfTrials;
            double timeout = _settings.Optimize.Timeout <= 0 ? -1 : _settings.Optimize.Timeout;
            string[] directions = _objective.Directions;
            TLog.Info($"Optimization \"{_settings.Optimize.StudyName}\" started with {nTrials} trials and {timeout} seconds timeout and {samplerType} sampler.");

            InitializePythonEngine();
            using (Py.GIL())
            {
                TLog.Debug("Wake Python GIL.");

                dynamic optuna = Py.Import("optuna");
                dynamic sampler = SetSamplerSettings(samplerType, _hasConstraints);
                dynamic storage = _settings.Storage.CreateNewOptunaStorage(false);
                dynamic artifactBackend = _settings.Storage.CreateNewOptunaArtifactBackend(false);

                Parameter[] parameter = null;
                TrialGrasshopperItems result = null;

                if (CheckExistStudyMatching(_objective.Length))
                {
                    dynamic study;
                    switch (_objective.HumanInTheLoopType)
                    {
                        case HumanInTheLoopType.HumanSliderInput:
                            HumanSliderInputOptimization(nTrials, timeout, directions, sampler, storage, artifactBackend, out parameter, out result, out study);
                            break;
                        case HumanInTheLoopType.Preferential:
                            PreferentialOptimization(nTrials, storage, artifactBackend, out parameter, out result, out study);
                            break;
                        default:
                            NormalOptimization(nTrials, timeout, directions, sampler, storage, artifactBackend, out parameter, out result, out study);
                            break;
                    }
                    SetResultValues(_objective.Length, study, parameter);
                }
            }
            PythonEngine.Shutdown();
            TLog.Debug("Shutdown PythonEngine.");
        }

        private void PreferentialOptimization(int nBatch, dynamic storage, dynamic artifactBackend, out Parameter[] parameter, out TrialGrasshopperItems result, out dynamic study)
        {
            TLog.MethodStart();
            var preferentialOpt = new Preferential(TEnvVariables.TmpDirPath, _settings.Storage.Path);
            if (_objective.Length > 1)
            {
                TunnyMessageBox.Show("Human-in-the-Loop(Preferential GP optimization) only supports single objective optimization. Optimization is run without considering constraints.", "Tunny");
            }
            string[] objNickName = _objective.GetNickNames();
            study = preferentialOpt.CreateStudy(nBatch, _settings.Optimize.StudyName, storage, objNickName[0]);
            var optInfo = new OptimizationHandlingInfo(int.MaxValue, 0, study, storage, artifactBackend, _fishEgg, objNickName);
            SetStudyUserAttr(study, PyConverter.EnumeratorToPyList(_variables.Select(v => v.NickName)), false);
            HumanInTheLoopBase.WakeOptunaDashboard(_settings.Storage.Path, TEnvVariables.PythonPath);
            optInfo.Preferential = preferentialOpt;
            RunPreferentialOptimize(optInfo, nBatch, out parameter, out result);
        }

        private void NormalOptimization(int nTrials, double timeout, string[] directions, dynamic sampler, dynamic storage, dynamic artifactBackend, out Parameter[] parameter, out TrialGrasshopperItems result, out dynamic study)
        {
            TLog.MethodStart();
            PyObject optuna = Py.Import("optuna");
            dynamic pruner = _settings.Pruner.ToPython();
            study = Study.CreateStudy(optuna, _settings.Optimize.StudyName, sampler, directions, storage, pruner, _settings.Optimize.ContinueStudy);
            string[] objNickName = _objective.GetNickNames();
            var optInfo = new OptimizationHandlingInfo(nTrials, timeout, study, storage, artifactBackend, _fishEgg, objNickName);
            SetStudyUserAttr(study, PyConverter.EnumeratorToPyList(_variables.Select(v => v.NickName)));
            RunOptimize(optInfo, out parameter, out result);
        }

        private void HumanSliderInputOptimization(int nBatch, double timeout, string[] directions, dynamic sampler, dynamic storage, dynamic artifactBackend, out Parameter[] parameter, out TrialGrasshopperItems result, out dynamic study)
        {
            TLog.MethodStart();
            PyObject optuna = Py.Import("optuna");
            study = Study.CreateStudy(optuna, _settings.Optimize.StudyName, sampler, directions, storage, null, _settings.Optimize.ContinueStudy);
            string[] objNickName = _objective.GetNickNames();
            var optInfo = new OptimizationHandlingInfo(int.MaxValue, timeout, study, storage, artifactBackend, _fishEgg, objNickName);
            SetStudyUserAttr(study, PyConverter.EnumeratorToPyList(_variables.Select(v => v.NickName)));
            var humanSliderInput = new HumanSliderInput(TEnvVariables.TmpDirPath, _settings.Storage.Path);
            HumanInTheLoopBase.WakeOptunaDashboard(_settings.Storage.Path, TEnvVariables.PythonPath);
            humanSliderInput.SetObjective(study, objNickName);
            humanSliderInput.SetWidgets(study, objNickName);
            optInfo.HumanSliderInput = humanSliderInput;
            RunHumanSidlerInputOptimize(optInfo, nBatch, out parameter, out result);
        }

        private bool CheckExistStudyMatching(int nObjective)
        {
            TLog.MethodStart();
            var storage = new StorageHandler();
            StudySummary[] studySummaries = storage.GetStudySummaries(_settings.Storage.Path);
            bool containStudyName = studySummaries.Select(s => s.StudyName).Contains(_settings.Optimize.StudyName);

            if (!containStudyName)
            {
                return true;
            }
            else if (!_settings.Optimize.ContinueStudy)
            {
                EndState = EndState.UseExitStudyWithoutContinue;
                return false;
            }

            bool isSameObjectiveNumber = studySummaries.FirstOrDefault(s => s.StudyName == _settings.Optimize.StudyName)?.Directions.Length == nObjective;
            if (isSameObjectiveNumber)
            {
                return true;
            }
            else
            {
                EndState = EndState.DirectionNumNotMatch;
                return false;
            }
        }

        private void SetResultValues(int nObjective, dynamic study, Parameter[] parameter)
        {
            TLog.MethodStart();
            if (nObjective == 1 && _objective.HumanInTheLoopType != HumanInTheLoopType.Preferential)
            {
                PyObject[] pyBestParams = study.best_params.values();
                string[] values = pyBestParams.Select(x => x.ToString()).ToArray();
                string[] keys = (string[])study.best_params.keys();
                var opt = new Parameter[_variables.Count];

                SetParamsFromOptunaBestParams(values, keys, opt);
                OptimalParameters = opt;
            }
            else
            {
                OptimalParameters = parameter;
            }
        }

        private void SetParamsFromOptunaBestParams(string[] values, string[] keys, Parameter[] opt)
        {
            for (int i = 0; i < _variables.Count; i++)
            {
                int index = Array.IndexOf(keys, _variables[i].NickName);
                if (index == -1)
                {
                    continue;
                }
                switch (_variables[i])
                {
                    case NumberVariable _:
                        double num = double.Parse(values[index], CultureInfo.InvariantCulture);
                        opt[i] = new Parameter(num);
                        break;
                    case CategoricalVariable _:
                        opt[i] = new Parameter(values[index]);
                        break;
                    default:
                        throw new ArgumentException("Variable type is not supported.");
                }
            }
        }

        private void RunOptimize(OptimizationHandlingInfo optInfo, out Parameter[] parameter, out TrialGrasshopperItems result)
        {
            TLog.MethodStart();
            parameter = new Parameter[_variables.Count];
            result = new TrialGrasshopperItems();
            int trialNum = 0;
            DateTime startTime = DateTime.Now;
            EnqueueTrial(optInfo.Study, optInfo.EnqueueItems);

            while (true)
            {
                if (result == null || CheckOptimizeComplete(optInfo, trialNum, startTime))
                {
                    break;
                }
                result = RunSingleOptimizeStep(optInfo, parameter, trialNum, startTime);
                trialNum++;
            }

            SaveInMemoryStudy(optInfo.Storage);
        }

        private void RunPreferentialOptimize(OptimizationHandlingInfo optInfo, int nBatch, out Parameter[] parameter, out TrialGrasshopperItems result)
        {
            TLog.MethodStart();
            parameter = new Parameter[_variables.Count];
            result = new TrialGrasshopperItems();
            int trialNum = 0;
            DateTime startTime = DateTime.Now;
            EnqueueTrial(optInfo.Study, optInfo.EnqueueItems);

            while (true)
            {
                if (result == null || CheckOptimizeComplete(optInfo, trialNum, startTime))
                {
                    break;
                }
                if (Preferential.GetRunningTrialNumber(optInfo.Study) >= nBatch)
                {
                    continue;
                }
                result = RunSingleOptimizeStep(optInfo, parameter, trialNum, startTime);
                trialNum++;
            }

            SaveInMemoryStudy(optInfo.Storage);
        }

        private void RunHumanSidlerInputOptimize(OptimizationHandlingInfo optInfo, int nBatch, out Parameter[] parameter, out TrialGrasshopperItems result)
        {
            TLog.MethodStart();
            parameter = new Parameter[_variables.Count];
            result = new TrialGrasshopperItems();
            int trialNum = 0;
            DateTime startTime = DateTime.Now;
            EnqueueTrial(optInfo.Study, optInfo.EnqueueItems);

            while (true)
            {
                if (result == null || CheckOptimizeComplete(optInfo, trialNum, startTime))
                {
                    break;
                }
                if (HumanSliderInput.GetRunningTrialNumber(optInfo.Study) >= nBatch)
                {
                    continue;
                }
                result = RunSingleOptimizeStep(optInfo, parameter, trialNum, startTime);
                trialNum++;
            }

            SaveInMemoryStudy(optInfo.Storage);
        }

        private TrialGrasshopperItems RunSingleOptimizeStep(OptimizationHandlingInfo optInfo, Parameter[] parameter, int trialNum, DateTime startTime)
        {
            TLog.MethodStart();
            dynamic optuna = Py.Import("optuna");
            dynamic trial;
            int progress = trialNum * 100 / optInfo.NTrials;
            var result = new TrialGrasshopperItems();

            int nullCount = 0;

            while (true)
            {
                if (optInfo.Preferential != null && !optInfo.Study.should_generate() && !OptimizeProcess.IsForcedStopOptimize)
                {
                    Thread.Sleep(100);
                    continue;
                }

                trial = optInfo.Study.ask();
                SetOptimizationParameter(parameter, trial);
                ProgressState pState = SetProgressState(optInfo, parameter, trialNum, startTime, trial, _settings.Pruner, progress);
                if (_settings.Optimize.IgnoreDuplicateSampling && IsSampleDuplicate(trial, out result))
                {
                    TLog.Info($"Trial {trialNum} is duplicate sample.");
                    pState.IsReportOnly = true;
                    _evalFunc(pState, progress);
                    break;
                }
                else
                {
                    result = _evalFunc(pState, progress);
                }

                if (optInfo.HumanSliderInput != null)
                {
                    optInfo.HumanSliderInput.SaveNote(optInfo.Study, trial, result.Objectives.Images);
                }
                else if (optInfo.Preferential != null && result.Objectives.Images.Length == 1)
                {
                    optInfo.Preferential.UploadArtifact(trial, result.Objectives.Images[0]);
                }

                if (nullCount >= 10)
                {
                    return TenTimesNullResultErrorMessage();
                }
                else if (result.Objectives.Numbers.Contains(double.NaN))
                {
                    nullCount++;
                }
                else
                {
                    break;
                }
            }

            SetTrialUserAttr(result, trial, optInfo);
            try
            {
                TellResultToOptuna(optInfo, parameter, trialNum, trial, result);
            }
            catch (Exception e)
            {
                TLog.Error(e.Message);
                throw new ArgumentException(e.Message);
            }
            finally
            {
                RunGC(result);
            }

            return result;
        }

        private void TellResultToOptuna(OptimizationHandlingInfo optInfo, Parameter[] parameter, int trialNum, dynamic trial, TrialGrasshopperItems result)
        {
            TLog.MethodStart();
            dynamic optuna = Py.Import("optuna");

            if (result.Artifacts.Count() > 0)
            {
                result.Artifacts.UploadArtifacts(optInfo.ArtifactBackend, trial);
            }

            if (_objective.Length == 1 && CheckShouldPrune(trial))
            {
                TellPruned(optInfo, trialNum, trial, optuna);
            }
            else if (result.Attribute.TryGetValue("IsFAIL", out List<string> isFail) && isFail.Contains("True"))
            {
                TellFailed(optInfo, trialNum, trial, optuna);
            }
            else if (optInfo.HumanSliderInput == null && optInfo.Preferential == null)
            {
                TellCompleted(optInfo, parameter, trialNum, trial, result);
            }
        }

        private void TellCompleted(OptimizationHandlingInfo optInfo, Parameter[] parameter, int trialNum, dynamic trial, TrialGrasshopperItems result)
        {
            optInfo.Study.tell(trial, result.Objectives.Numbers);
            SetTrialResultLog(trialNum, result, optInfo, parameter);
        }

        private static void TellFailed(OptimizationHandlingInfo optInfo, int trialNum, dynamic trial, dynamic optuna)
        {
            optInfo.Study.tell(trial, state: optuna.trial.TrialState.FAIL);
            TLog.Warning($"Trial {trialNum} failed.");
        }

        private static void TellPruned(OptimizationHandlingInfo optInfo, int trialNum, dynamic trial, dynamic optuna)
        {
            if (trial.user_attrs.get(OptimizeProcess.PrunedTrialReportValueKey) != null)
            {
                optInfo.Study.tell(trial, trial.user_attrs.get(OptimizeProcess.PrunedTrialReportValueKey));
            }
            else
            {
                optInfo.Study.tell(trial, state: optuna.trial.TrialState.PRUNED);
            }
            TLog.Warning($"Trial {trialNum} pruned.");
        }

        //TODO: Fix Do not use try-catch block
        private static bool CheckShouldPrune(dynamic trial)
        {
            bool shouldPrune;
            try
            {
                shouldPrune = trial.should_prune();
            }
            catch (Exception)
            {
                PyObject pyShouldPrune = trial.should_prune().item();
                shouldPrune = pyShouldPrune.As<bool>();
            }

            return shouldPrune;
        }

        private static bool IsSampleDuplicate(dynamic trial, out TrialGrasshopperItems result)
        {
            double[] values;
            PyModule ps = Py.CreateScope();
            var assembly = Assembly.GetExecutingAssembly();
            ps.Exec(ReadFileFromResource.Text(assembly, "Tunny.Solver.Python.check_duplication.py"));
            dynamic checkDuplicate = ps.Get("check_duplicate");
            values = checkDuplicate(trial);
            result = new TrialGrasshopperItems(values);
            return values != null;
        }

        private void SetOptimizationParameter(Parameter[] parameter, dynamic trial)
        {
            TLog.MethodStart();
            foreach ((VariableBase variable, int i) in _variables.Select((v, i) => (v, i)))
            {
                string name = variable.NickName;
                switch (variable)
                {
                    case NumberVariable number:
                        double numParam;
                        if (number.IsLogScale)
                        {
                            numParam = number.IsInteger
                                ? trial.suggest_int(name, number.LowerBond, number.UpperBond, log: true)
                                : trial.suggest_float(name, number.LowerBond, number.UpperBond, log: true);
                        }
                        else
                        {
                            numParam = number.IsInteger
                                ? trial.suggest_int(name, number.LowerBond, number.UpperBond, step: number.Epsilon)
                                : trial.suggest_float(name, number.LowerBond, number.UpperBond, step: number.Epsilon);
                        }
                        parameter[i] = new Parameter(numParam);
                        break;
                    case CategoricalVariable category:
                        string catParam = (string)trial.suggest_categorical(name, category.Categories);
                        parameter[i] = new Parameter(catParam);
                        break;
                }
            }
        }

        private void SetTrialResultLog(int trialNum, TrialGrasshopperItems result, OptimizationHandlingInfo optInfo, Parameter[] parameter)
        {
            TLog.MethodStart();
            var sb = new StringBuilder();
            sb.Append("Trial " + trialNum + " finished with values: {");
            foreach (string value in optInfo.ObjectiveNames.Zip(result.Objectives.Numbers, (name, value) => "'" + name + "': " + value))
            {
                sb.Append(value + ", ");
            }
            sb.Remove(sb.Length - 2, 2);
            sb.Append("} and parameters: {");
            foreach (string value in _variables.Zip(parameter, (v, value) => "'" + v.NickName + "': " + value))
            {
                sb.Append(value + ", ");
            }
            sb.Remove(sb.Length - 2, 2);
            sb.Append("}.");
            string message = sb.ToString();
            SharedItems.Instance.Component.SetInfo(message);
            TLog.Info(sb.ToString());
        }

        private static TrialGrasshopperItems TenTimesNullResultErrorMessage()
        {
            TLog.MethodStart();
            TunnyMessageBox.Show(
                "The objective function returned NaN 10 times in a row. Tunny terminates the optimization. Please check the objective function.",
                "Tunny",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
            return null;
        }

        private bool CheckOptimizeComplete(OptimizationHandlingInfo optInfo, int trialNum, DateTime startTime)
        {
            TLog.MethodStart();

            int nTrials = optInfo.NTrials;
            double timeout = optInfo.Timeout;
            dynamic study = optInfo.Study;
            bool studyStopFlag = false;
            if (optInfo.HumanSliderInput == null && optInfo.Preferential == null)
            {
                studyStopFlag = study._stop_flag;
            }


            if (File.Exists(TEnvVariables.QuitFishingPath))
            {
                OptimizeProcess.IsForcedStopOptimize = true;
                File.Delete(TEnvVariables.QuitFishingPath);
            }

            bool isOptimizeCompleted = false;
            if (trialNum >= nTrials)
            {
                EndState = EndState.AllTrialCompleted;
                isOptimizeCompleted = true;
            }
            else if (timeout > 0 && (DateTime.Now - startTime).TotalSeconds >= timeout)
            {
                EndState = EndState.Timeout;
                isOptimizeCompleted = true;
            }
            else if (OptimizeProcess.IsForcedStopOptimize)
            {
                EndState = EndState.StoppedByUser;
                OptimizeProcess.IsForcedStopOptimize = false;
                isOptimizeCompleted = true;
            }
            else if (optInfo.HumanSliderInput == null && optInfo.Preferential == null && studyStopFlag)
            {
                EndState = EndState.StoppedByOptuna;
                isOptimizeCompleted = true;
            }

            return isOptimizeCompleted;
        }

        private ProgressState SetProgressState(OptimizationHandlingInfo optSet, Parameter[] parameter, int trialNum, DateTime startTime, dynamic trial, Pruner pruner, int progress)
        {
            TLog.MethodStart();
            double[][] bestValues = ComputeBestValues(optSet.Study);
            return new ProgressState
            {
                PercentComplete = progress,
                TrialNumber = trialNum,
                ObjectiveNum = _objective.Length,
                BestValues = bestValues,
                Parameter = parameter,
                HypervolumeRatio = 0,
                OptunaTrial = trial,
                Pruner = pruner,
                EstimatedTimeRemaining = optSet.Timeout <= 0
                    ? TimeSpan.FromSeconds((DateTime.Now - startTime).TotalSeconds * (optSet.NTrials - trialNum) / (trialNum + 1))
                    : TimeSpan.FromSeconds(optSet.Timeout - (DateTime.Now - startTime).TotalSeconds)
            };
        }

        private double[][] ComputeBestValues(dynamic study)
        {
            TLog.MethodStart();
            if (_settings.Optimize.ShowRealtimeResult)
            {
                dynamic[] bestTrials = study.best_trials;
                return bestTrials.Select(t => (double[])t.values).ToArray();
            }
            else
            {
                return null;
            }
        }

        private void SaveInMemoryStudy(dynamic storage)
        {
            TLog.MethodStart();
            if (_settings.Storage.Type == StorageType.InMemory)
            {
                dynamic optuna = Py.Import("optuna");
                string studyName = _settings.Optimize.StudyName;
                optuna.copy_study(from_study_name: studyName, to_study_name: studyName, from_storage: storage, to_storage: new StorageHandler().CreateNewTStorage(false, _settings.Storage));
            }
        }

        private static dynamic EnqueueTrial(dynamic study, List<FishEgg> enqueueItems)
        {
            TLog.MethodStart();
            if (enqueueItems == null || enqueueItems.Count == 0)
            {
                return study;
            }
            foreach (FishEgg egg in enqueueItems)
            {
                egg.EnqueueStudy(study);
            }
            return study;
        }

        private void RunGC(TrialGrasshopperItems result)
        {
            TLog.MethodStart();
            GcAfterTrial gcAfterTrial = _settings.Optimize.GcAfterTrial;
            if ((gcAfterTrial != GcAfterTrial.Always) &&
                ((result.GeometryJson.Length <= 0) || (gcAfterTrial != GcAfterTrial.HasGeometry)))
            {
                return;
            }

            dynamic gc = Py.Import("gc");
            gc.collect();
        }

        private void SetStudyUserAttr(dynamic study, PyList variableName, bool setMetricNames = true)
        {
            TLog.MethodStart();
            study.set_user_attr("variable_names", variableName);
            study.set_user_attr("tunny_version", TEnvVariables.Version.ToString(3));
            if (setMetricNames)
            {
                study.set_metric_names(PyConverter.EnumeratorToPyList(_objective.GetNickNames()));
            }
        }

        private static void SetTrialUserAttr(TrialGrasshopperItems result, dynamic trial, OptimizationHandlingInfo optSet)
        {
            TLog.MethodStart();
            if (result.GeometryJson.Length != 0)
            {
                PyList pyJson = PyConverter.EnumeratorToPyList(result.GeometryJson);
                trial.set_user_attr("Geometry", pyJson);
            }

            if (result.Attribute != null)
            {
                SetNonGeometricAttr(result, trial);
            }

            if (result.Objectives.Length != 0 && optSet.HumanSliderInput != null)
            {
                int imageCount = 0;
                for (int i = 0; i < result.Objectives.Length; i++)
                {
                    if (!optSet.ObjectiveNames[i].Contains("Human-in-the-Loop"))
                    {
                        var key = new PyString("result_" + optSet.ObjectiveNames[i]);
                        var value = new PyFloat(result.Objectives.Numbers[i - imageCount]);
                        trial.set_user_attr(key, value);
                    }
                    else
                    {
                        imageCount++;
                    }
                }
            }
        }

        private static void SetNonGeometricAttr(TrialGrasshopperItems result, dynamic trial)
        {
            TLog.MethodStart();
            foreach (KeyValuePair<string, List<string>> pair in result.Attribute)
            {
                PyObject pyReportValues;
                if (pair.Key == "Constraint")
                {
                    IEnumerable<double> values = pair.Value.Select(x => double.Parse(x, CultureInfo.InvariantCulture));
                    pyReportValues = PyConverter.EnumeratorToPyList(values);
                }
                else if (pair.Key == "Direction")
                {
                    continue;
                }
                else if (pair.Value.Count == 1)
                {
                    pyReportValues = double.TryParse(pair.Value[0], out double num)
                        ? new PyFloat(num)
                        : (PyObject)new PyString(pair.Value[0]);
                }
                else if (pair.Value.All(x => double.TryParse(x, out double _)))
                {
                    IEnumerable<double> values = pair.Value.Select(x => double.Parse(x, CultureInfo.InvariantCulture));
                    pyReportValues = PyConverter.EnumeratorToPyList(values);
                }
                else
                {
                    pyReportValues = PyConverter.EnumeratorToPyList(pair.Value);
                }
                trial.set_user_attr(pair.Key, pyReportValues);
            }
        }

        private dynamic SetSamplerSettings(SamplerType samplerType, bool hasConstraints)
        {
            TLog.MethodStart();
            string storagePath = _settings.Storage.GetOptunaStoragePath();
            PyDict cmaEsX0 = GetFirstEgg(_fishEgg);

            dynamic optunaSampler = _settings.Optimize.Sampler.ToPython(samplerType, storagePath, hasConstraints, cmaEsX0);
            if (
                (samplerType == SamplerType.GP || samplerType == SamplerType.CmaEs || samplerType == SamplerType.QMC || samplerType == SamplerType.Random || samplerType == SamplerType.AUTO || samplerType == SamplerType.BruteForce)
                && hasConstraints
            )
            {
                TunnyMessageBox.Show("Only TPE, GP:BoTorch and NSGA support constraints. Optimization is run without considering constraints.", "Tunny");
            }
            return optunaSampler;
        }

        private static PyDict GetFirstEgg(List<FishEgg> fishEgg)
        {
            TLog.MethodStart();
            if (fishEgg == null || fishEgg.Count == 0)
            {
                return null;
            }

            return fishEgg[0].GetParamPyDict();
        }
    }
}
