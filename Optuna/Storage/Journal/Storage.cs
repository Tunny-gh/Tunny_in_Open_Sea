using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Newtonsoft.Json.Linq;

using Optuna.Study;
using Optuna.Trial;

namespace Optuna.Storage.Journal
{
    public class JournalStorage : IOptunaStorage
    {
        private readonly Dictionary<int, Study.Study> _studies = new Dictionary<int, Study.Study>();
        private int _nextStudyId;
        private int _trialId;

        public JournalStorage(string path, bool createIfNotExist = false)
        {
            if (File.Exists(path) == false)
            {
                if (createIfNotExist == false)
                {
                    throw new FileNotFoundException($"File not found: {path}");
                }
                else
                {
                    File.Create(path).Close();
                }
            }

            var logs = new List<string>();
            using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var sr = new StreamReader(fs))
                {
                    while (sr.Peek() >= 0)
                    {
                        logs.Add(sr.ReadLine());
                    }
                }
            }

            foreach (string log in logs)
            {
                if (log == "")
                {
                    continue;
                }

                var logObject = JObject.Parse(log);
                var opCode = (JournalOperation)Enum.ToObject(typeof(JournalOperation), (int)logObject["op_code"]);
                switch (opCode)
                {
                    case JournalOperation.CreateStudy:
                        StudyDirection[] studyDirections = logObject["directions"].ToObject<StudyDirection[]>();
                        string studyName = logObject["study_name"].ToString();
                        CreateNewStudy(studyDirections, studyName);
                        break;
                    case JournalOperation.DeleteStudy:
                        {
                            int studyId = (int)logObject["study_id"];
                            DeleteStudy(studyId);
                        }
                        break;
                    case JournalOperation.SetStudyUserAttr:
                        {
                            int studyId = (int)logObject["study_id"];
                            var userAttr = (JObject)logObject["user_attr"];
                            foreach (KeyValuePair<string, JToken> item in userAttr)
                            {
                                string[] values = item.Value.Select(v => v.ToString()).ToArray();
                                if (values == null || values.Length == 0)
                                {
                                    values = new string[] { item.Value.ToString() };
                                }
                                SetStudyUserAttr(studyId, item.Key, values);
                            }
                        }
                        break;
                    case JournalOperation.SetStudySystemAttr:
                        {
                            int studyId = (int)logObject["study_id"];
                            var systemAttr = (JObject)logObject["system_attr"];
                            foreach (KeyValuePair<string, JToken> item in systemAttr)
                            {
                                string[] values = item.Value.Select(v => v.ToString()).ToArray();
                                if (values == null || values.Length == 0)
                                {
                                    values = new string[] { item.Value.ToString() };
                                }
                                SetStudySystemAttr(studyId, item.Key, values);
                            }
                        }
                        break;
                    case JournalOperation.CreateTrial:
                        {
                            int studyId = (int)logObject["study_id"];
                            Trial.Trial trial;
                            if (logObject["datetime_complete"] == null)
                            {
                                trial = new Trial.Trial();
                                JToken start = logObject["datetime_start"];
                                if (start != null && start.HasValues)
                                {
                                    trial.DatetimeStart = (DateTime)start;
                                }
                                CreateNewTrial(studyId, trial);
                            }
                            else
                            {
                                Dictionary<string, object> trialParams = logObject["params"].ToObject<Dictionary<string, object>>();
                                var trialParamsWithType = new Dictionary<string, object>();
                                foreach (KeyValuePair<string, object> item in trialParams)
                                {
                                    switch (item.Value)
                                    {
                                        case double d:
                                            trialParamsWithType[item.Key] = d;
                                            break;
                                        case string s:
                                            trialParamsWithType[item.Key] = s;
                                            break;
                                        case int i:
                                            trialParamsWithType[item.Key] = i;
                                            break;
                                        default:
                                            trialParamsWithType[item.Key] = item.Value;
                                            break;
                                    }
                                }
                                var values = new List<double>();
                                foreach (JToken value in logObject["values"])
                                {
                                    values.Add(value.ToObject<double>());
                                }
                                if (logObject["value"].Type != JTokenType.Null)
                                {
                                    values.Add((double)logObject["value"]);
                                }
                                trial = new Trial.Trial
                                {
                                    Values = values.ToArray(),
                                    Params = trialParamsWithType,
                                    State = (TrialState)Enum.ToObject(typeof(TrialState), (int)logObject["state"]),
                                    DatetimeStart = (DateTime)logObject["datetime_start"],
                                    DatetimeComplete = (DateTime)logObject["datetime_complete"],
                                };
                                CreateNewTrial(studyId, trial);
                                SetTrialSystemAttrFromJObject(trial.TrialId, (JObject)logObject["system_attrs"]);
                                SetTrialUserAttrFromJObject(trial.TrialId, (JObject)logObject["user_attrs"]);
                            }
                        }
                        break;
                    case JournalOperation.SetTrialParam:
                        {
                            int trialId = (int)logObject["trial_id"];
                            string paramName = (string)logObject["param_name"];
                            double paramValueInternal = (double)logObject["param_value_internal"];
                            SetTrailParam(trialId, paramName, paramValueInternal, null);
                        }
                        break;
                    case JournalOperation.SetTrialStateValues:
                        {
                            int trialId = (int)logObject["trial_id"];
                            double[] trialValues = logObject["values"].Select(v => v.ToObject<double>()).ToArray();
                            var trialState = (TrialState)Enum.ToObject(typeof(TrialState), (int)logObject["state"]);
                            SetTrialStateValue(trialId, trialState, trialValues);
                        }
                        break;
                    case JournalOperation.SetTrialIntermediateValue:
                        break;
                    case JournalOperation.SetTrialUserAttr:
                        {
                            int trialId = (int)logObject["trial_id"];
                            var userAttr = (JObject)logObject["user_attr"];
                            SetTrialUserAttrFromJObject(trialId, userAttr);
                        }
                        break;
                    case JournalOperation.SetTrialSystemAttr:
                        {
                            int trialId = (int)logObject["trial_id"];
                            var systemAttr = (JObject)logObject["system_attr"];
                            SetTrialSystemAttrFromJObject(trialId, systemAttr);
                        }
                        break;
                }
            }
        }

        private void SetTrialSystemAttrFromJObject(int trialId, JObject systemAttr)
        {
            foreach (KeyValuePair<string, JToken> item in systemAttr)
            {
                string[] values = item.Value.Select(v => v.ToString()).ToArray();
                if (values == null || values.Length == 0)
                {
                    values = new string[] { item.Value.ToString() };
                }
                SetTrialSystemAttr(trialId, item.Key, values);
            }
        }

        private void SetTrialUserAttrFromJObject(int trialId, JObject userAttr)
        {
            foreach (KeyValuePair<string, JToken> item in userAttr)
            {
                string[] values = item.Value.Select(v => v.ToString()).ToArray();
                if (values == null || values.Length == 0)
                {
                    values = new string[] { item.Value.ToString() };
                }
                SetTrialUserAttr(trialId, item.Key, values);
            }
        }

        public void CheckTrialIsUpdatable(int trialId, TrialState trialState)
        {
            throw new NotImplementedException();
        }

        public int CreateNewStudy(StudyDirection[] studyDirections, string studyName = "")
        {
            string[] studyNames = _studies.Values.Select(s => s.StudyName).ToArray();
            if (!studyNames.Contains(studyName))
            {
                _studies.Add(_nextStudyId, new Study.Study(this, _nextStudyId, studyName, studyDirections));
                _nextStudyId++;
            }
            return _nextStudyId;
        }

        public int CreateNewTrial(int studyId, Trial.Trial templateTrial = null)
        {
            Trial.Trial trial = templateTrial ?? new Trial.Trial();
            trial.TrialId = _trialId++;
            trial.Number = _studies[studyId].Trials.Count;
            _studies[studyId].Trials.Add(trial);
            return _trialId;
        }

        public void DeleteStudy(int studyId)
        {
            _studies.Remove(studyId);
        }

        public Study.Study[] GetAllStudies()
        {
            return _studies.Values.ToArray();
        }

        public Trial.Trial[] GetAllTrials(int studyId, bool deepcopy = true)
        {
            return _studies[studyId].Trials.ToArray();
        }

        public Trial.Trial GetBestTrial(int studyId)
        {
            List<Trial.Trial> allTrials = _studies[studyId].Trials.FindAll(trial => trial.State == TrialState.COMPLETE);

            if (allTrials.Count == 0)
            {
                return null;
            }

            StudyDirection[] directions = GetStudyDirections(studyId);
            if (directions.Length != 1)
            {
                throw new InvalidOperationException("Study is multi-objective.");
            }

            if (directions[0] == StudyDirection.Maximize)
            {
                return allTrials.OrderByDescending(trial => trial.Values[0]).First();
            }
            else
            {
                return allTrials.OrderBy(trial => trial.Values[0]).First();
            }
        }

        public int GetNTrials(int studyId)
        {
            return _studies[studyId].Trials.Count;
        }

        public StudyDirection[] GetStudyDirections(int studyId)
        {
            return _studies[studyId].Directions;
        }

        public int GetStudyIdFromName(string studyName)
        {
            return _studies.Values.First(s => s.StudyName == studyName).StudyId;
        }

        public string GetStudyNameFromId(int studyId)
        {
            return _studies[studyId].StudyName;
        }

        public Dictionary<string, object> GetStudySystemAttrs(int studyId)
        {
            return _studies[studyId].SystemAttrs;
        }

        public Dictionary<string, object> GetStudyUserAttrs(int studyId)
        {
            return _studies[studyId].UserAttrs;
        }

        public Trial.Trial GetTrial(int trialId)
        {
            return _studies.Values.First(s => s.Trials.Any(t => t.TrialId == trialId))
                           .Trials.First(t => t.TrialId == trialId);
        }

        public int GetTrialIdFromStudyIdTrialNumber(int studyId, int trialNumber)
        {
            return _studies[studyId].Trials.Find(t => t.Number == trialNumber).TrialId;
        }

        public int GetTrialNumberFromId(int trialId)
        {
            return GetTrial(trialId).Number;
        }

        public double GetTrialParam(int trialId, string paramName)
        {
            return (double)GetTrial(trialId).Params[paramName];
        }

        public Dictionary<string, object> GetTrialParams(int trialId)
        {
            return GetTrial(trialId).Params.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public Dictionary<string, object> GetTrialSystemAttrs(int trialId)
        {
            return GetTrial(trialId).SystemAttrs.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public Dictionary<string, object> GetTrialUserAttrs(int trialId)
        {
            return GetTrial(trialId).UserAttrs.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public void RemoveSession()
        {
            throw new NotImplementedException();
        }

        public void SetStudySystemAttr(int studyId, string key, object value)
        {
            _studies[studyId].SystemAttrs[key] = value;
        }

        public void SetStudyUserAttr(int studyId, string key, object value)
        {
            _studies[studyId].UserAttrs[key] = value;
        }

        public void SetTrailParam(int trialId, string paramName, double paramValueInternal, object distribution)
        {
            GetTrial(trialId).Params[paramName] = paramValueInternal;
        }

        public void SetTrialIntermediateValue(int trialId, int intermediateStep, double intermediateValue)
        {
            throw new NotImplementedException();
        }

        public bool SetTrialStateValue(int trialId, TrialState state, double[] values = null)
        {
            Trial.Trial trial = GetTrial(trialId);
            trial.State = state;
            trial.Values = values;
            return true;
        }

        public void SetTrialSystemAttr(int trialId, string key, object value)
        {
            GetTrial(trialId).SystemAttrs[key] = value;
        }

        public void SetTrialUserAttr(int trialId, string key, object value)
        {
            GetTrial(trialId).UserAttrs[key] = value;
        }
    }
}
