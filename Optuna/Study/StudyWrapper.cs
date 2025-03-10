using System;
using System.Linq;

using Optuna.Trial;
using Optuna.Util;

using Python.Runtime;

namespace Optuna.Study
{
    public class StudyWrapper
    {
        public dynamic PyInstance { get; private set; }
        public bool StopFlag => PyInstance._stop_flag;
        public int Id => PyInstance._study_id;
        public PyList Trials => PyInstance.trials;
        public double[][] BestTrials
        {
            get
            {
                dynamic[] bestTrials = PyInstance.best_trials;
                return bestTrials.Select(t => (double[])t.values).ToArray();
            }
        }

        public StudyWrapper(dynamic study)
        {
            PyInstance = study;

            dynamic optuna = Py.Import("optuna");
            dynamic py = Py.Import("builtins");
            if (!py.isinstance(study, optuna.study.Study))
            {
                throw new ArgumentException("study must be an instance of optuna.study.Study.");
            }
        }

        public bool ShouldGenerate()
        {
            return PyInstance.should_generate();
        }

        public TrialWrapper Ask()
        {
            return new TrialWrapper(PyInstance.ask());
        }

        public void Tell(TrialWrapper trial, double value)
        {
            PyInstance.tell(trial.PyInstance, value);
        }

        public void Tell(TrialWrapper trial, double[] values)
        {
            PyInstance.tell(trial.PyInstance, values);
        }

        public void Tell(TrialWrapper trial, TrialState state)
        {
            dynamic optuna = Py.Import("optuna");
            dynamic pyState;
            switch (state)
            {
                case TrialState.COMPLETE:
                    pyState = optuna.trial.TrialState.COMPLETE;
                    break;
                case TrialState.FAIL:
                    pyState = optuna.trial.TrialState.FAIL;
                    break;
                case TrialState.PRUNED:
                    pyState = optuna.trial.TrialState.PRUNED;
                    break;
                case TrialState.RUNNING:
                    pyState = optuna.trial.TrialState.RUNNING;
                    break;
                default:
                    pyState = PyInstance.Complete;
                    break;
            }
            PyInstance.tell(trial.PyInstance, pyState);
        }

        public void EnqueueTrial(PyDict paramDict, PyDict attrDict, bool skipIfExist)
        {
            PyInstance.enqueue_trial(paramDict, user_attrs: attrDict, skip_if_exists: skipIfExist);
        }

        public void SetUserAttribute(string key, string value)
        {
            PyInstance.set_user_attr(key, value);
        }

        public void SetUserAttribute(string key, string[] values)
        {
            PyList pyList = PyConverter.EnumeratorToPyList(values);
            PyInstance.set_user_attr(key, pyList);
        }

        public void SetMetricNames(string[] names)
        {
            PyList pyList = PyConverter.EnumeratorToPyList(names);
            PyInstance.set_metric_names(pyList);
        }

        public static StudyWrapper CreateStudy(string studyName, dynamic sampler, string[] directions, dynamic storage, dynamic pruner, bool loadIfExists)
        {
            dynamic optuna = Py.Import("optuna");
            dynamic study = optuna.create_study(
                sampler: sampler,
                directions: directions,
                storage: storage,
                study_name: studyName,
                load_if_exists: loadIfExists,
                pruner: pruner
            );
            // for escape exception in Brute Force sampler
            // Study.stop() method throws exception when in_optimize_loop is false
            study._thread_local.in_optimize_loop = true;
            return new StudyWrapper(study);
        }

        public static StudyWrapper LoadStudy(dynamic storage, string studyName)
        {
            dynamic optuna = Py.Import("optuna");
            return studyName == null
                ? throw new ArgumentException("studyName must not be null.")
                : new StudyWrapper(optuna.load_study(storage: storage, study_name: studyName));
        }
    }
}
