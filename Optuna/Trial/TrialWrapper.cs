using System;

using Optuna.Util;

using Python.Runtime;

namespace Optuna.Trial
{
    public class TrialWrapper
    {
        public dynamic PyInstance { get; }
        public int Id => PyInstance._trial_id;
        public int Number => PyInstance.number;

        public TrialWrapper(dynamic trial)
        {
            PyInstance = trial;
        }

        public void Report(double value, int step)
        {
            PyInstance.report(value, step);
        }

        public void SetUserAttribute(string key, PyObject value)
        {
            PyInstance.set_user_attr(key, value);
        }

        public void SetUserAttribute(string key, string value)
        {
            PyInstance.set_user_attr(key, value);
        }

        public void SetUserAttribute(string key, string[] values)
        {
            PyList valueList = PyConverter.EnumeratorToPyList(values);
            PyInstance.set_user_attr(key, valueList);
        }

        public void SetUserAttribute(string key, double value)
        {
            PyInstance.set_user_attr(key, value);
        }

        public void SetUserAttribute(string key, double[] values)
        {
            PyList valueList = PyConverter.EnumeratorToPyList(values);
            PyInstance.set_user_attr(key, valueList);
        }

        //TODO: Fix Do not use try-catch block
        public bool ShouldPrune()
        {
            bool shouldPrune;
            try
            {
                shouldPrune = PyInstance.should_prune();
            }
            catch (Exception)
            {
                PyObject pyShouldPrune = PyInstance.should_prune().item();
                shouldPrune = pyShouldPrune.As<bool>();
            }

            return shouldPrune;
        }

        public string SuggestCategorical(string name, string[] choices)
        {
            return PyInstance.suggest_categorical(name, choices);
        }

        public double SuggestFloat(string name, double low, double high, double step)
        {
            return PyInstance.suggest_float(name, low, high, step: step);
        }

        public double SuggestFloat(string name, double low, double high, bool log)
        {
            return PyInstance.suggest_float(name, low, high, log: log);
        }

        public int SuggestInt(string name, double low, double high, double step)
        {
            return PyInstance.suggest_int(name, low, high, step: step);
        }

        public int SuggestInt(string name, double low, double high, bool log)
        {
            return PyInstance.suggest_int(name, low, high, log: log);
        }
    }
}
