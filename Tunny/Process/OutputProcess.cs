using System.Collections.Generic;
using System.Linq;

using Optuna.Trial;

using Tunny.Component.Optimizer;
using Tunny.Core.Solver;
using Tunny.Core.Util;
using Tunny.Eto.Common;
using Tunny.Eto.Message;
using Tunny.Type;

namespace Tunny.Process
{
    internal static class OutputProcess
    {
        public static string StudyName;
        public static int[] Indices;

        internal static void Run()
        {
            TLog.MethodStart();
            OptimizeComponentBase component = CommonSharedItems.Instance.Component;

            var fishes = new List<Fish>();

            if (component != null)
            {
                var output = new Output(CommonSharedItems.Instance.Settings.Storage.Path);
                Trial[] targetTrials = output.GetTargetTrial(Indices, StudyName);
                string[] metricNames = output.GetMetricNames(StudyName);
                Dictionary<string, object>.KeyCollection nickNames = targetTrials[0].Params.Keys;

                if (metricNames == null || metricNames.Length == 0)
                {
                    metricNames = targetTrials[0].Values.Select((_, i) => $"Objective{i}").ToArray();
                }

                foreach (Trial trial in targetTrials)
                {
                    SetResultToFish(fishes, trial, nickNames, metricNames);
                }
                component.Fishes = fishes.ToArray();
                component.ExpireSolution(true);
            }

            TunnyMessageBox.Info_OutputFinish();
        }

        public static void SetResultToFish(List<Fish> fishes, Trial trial, IEnumerable<string> varNickname, string[] objNickname)
        {
            TLog.MethodStart();
            fishes.Add(new Fish
            {
                TrialNumber = trial.Number,
                Variables = SetVariables(trial.Params, varNickname),
                Objectives = SetObjectives(trial.Values, objNickname),
                Attributes = SetAttributes(ParseAttrs(trial.UserAttrs)),
            });
        }

        private static Dictionary<string, object> SetVariables(Dictionary<string, object> variables, IEnumerable<string> nickNames)
        {
            TLog.MethodStart();
            return nickNames.SelectMany(name => variables.Where(obj => obj.Key == name))
                .ToDictionary(variable => variable.Key, variable => variable.Value);
        }

        private static Dictionary<string, double> SetObjectives(double[] values, string[] nickNames)
        {
            TLog.MethodStart();
            var objectives = new Dictionary<string, double>();
            if (values == null)
            {
                return null;
            }

            for (int i = 0; i < values.Length; i++)
            {
                if (objectives.ContainsKey(nickNames[i]))
                {
                    objectives.Add(nickNames[i] + i, values[i]);
                }
                else
                {
                    objectives.Add(nickNames[i], values[i]);
                }
            }
            return objectives;
        }

        private static Dictionary<string, object> SetAttributes(Dictionary<string, List<string>> trialAttr)
        {
            TLog.MethodStart();
            var attribute = new Dictionary<string, object>();
            foreach (KeyValuePair<string, List<string>> attr in trialAttr)
            {
                attribute.Add(attr.Key, attr.Value);
            }
            return attribute;
        }

        private static Dictionary<string, List<string>> ParseAttrs(Dictionary<string, object> userAttrs)
        {
            TLog.MethodStart();
            var attributes = new Dictionary<string, List<string>>();
            foreach (string key in userAttrs.Keys)
            {
                string[] values = userAttrs[key] as string[];
                attributes.Add(key, values.ToList());
            }
            return attributes;
        }
    }
}
