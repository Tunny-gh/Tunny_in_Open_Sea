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
        private readonly Dictionary<int, Trial.Trial> _trialCache = new Dictionary<int, Trial.Trial>();
        private readonly Dictionary<string, int> _studyNameToIdIndex = new Dictionary<string, int>();
        private readonly object _initLock = new object();
        private readonly string _filePath;
        private int _nextStudyId;
        private int _trialId;
        private bool _isInitialized;
        private bool _isInitialing;

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
                    _isInitialized = true;
                }
            }
            _filePath = path;
        }

        private void EnsureInitialized()
        {
            if (_isInitialized || _isInitialing)
            {
                return;
            }

            lock (_initLock)
            {
                if (_isInitialized || _isInitialing)
                {
                    return;
                }

                _isInitialing = true;
                LoadDataFromFile();
                _isInitialized = true;
                _isInitialing = false;
            }
        }

        private void LoadDataFromFile()
        {
            const int BufferSize = 65536;
            var jsonBatch = new List<string>(1000);

            using (var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, BufferSize))
            {
                using (var sr = new StreamReader(fs))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            continue;
                        }

                        jsonBatch.Add(line);
                        if (jsonBatch.Count >= 1000)
                        {
                            ProcessJsonBatch(jsonBatch);
                            jsonBatch.Clear();
                        }
                    }

                    if (jsonBatch.Count > 0)
                    {
                        ProcessJsonBatch(jsonBatch);
                    }
                }

                BuildIndices();
            }
        }

        private void BuildIndices()
        {
            foreach (Study.Study study in _studies.Values)
            {
                _studyNameToIdIndex[study.StudyName] = study.StudyId;
            }

            foreach (Study.Study study in _studies.Values)
            {
                foreach (Trial.Trial trial in study.Trials)
                {
                    _trialCache[trial.TrialId] = trial;
                }
            }
        }

        private void ProcessJsonBatch(List<string> jsonBatch)
        {
            foreach (string json in jsonBatch)
            {
                var logObject = JObject.Parse(json);
                var opCode = (JournalOperation)Enum.ToObject(typeof(JournalOperation), (int)logObject["op_code"]);
                ProcessLogEntry(opCode, logObject);
            }
        }

        private void ProcessLogEntry(JournalOperation opCode, JObject logObject)
        {
            switch (opCode)
            {
                case JournalOperation.CreateStudy:
                    {
                        StudyDirection[] studyDirections = logObject["directions"].ToObject<StudyDirection[]>();
                        string studyName = logObject["study_name"].ToString();
                        CreateNewStudy(studyDirections, studyName);
                        break;
                    }
                case JournalOperation.DeleteStudy:
                    {
                        int studyId = (int)logObject["study_id"];
                        DeleteStudy(studyId);
                        break;
                    }
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
                        break;
                    }
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
                        break;
                    }
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
                        break;
                    }
                case JournalOperation.SetTrialParam:
                    {
                        int trialId = (int)logObject["trial_id"];
                        string paramName = (string)logObject["param_name"];
                        double paramValueInternal = (double)logObject["param_value_internal"];
                        SetTrailParam(trialId, paramName, paramValueInternal, null);
                        break;
                    }
                case JournalOperation.SetTrialStateValues:
                    {
                        int trialId = (int)logObject["trial_id"];
                        double[] trialValues = logObject["values"].Select(v => v.ToObject<double>()).ToArray();
                        var trialState = (TrialState)Enum.ToObject(typeof(TrialState), (int)logObject["state"]);
                        SetTrialStateValue(trialId, trialState, trialValues);
                        break;
                    }
                case JournalOperation.SetTrialIntermediateValue:
                    break;
                case JournalOperation.SetTrialUserAttr:
                    {
                        int trialId = (int)logObject["trial_id"];
                        var userAttr = (JObject)logObject["user_attr"];
                        SetTrialUserAttrFromJObject(trialId, userAttr);
                        break;
                    }
                case JournalOperation.SetTrialSystemAttr:
                    {
                        int trialId = (int)logObject["trial_id"];
                        var systemAttr = (JObject)logObject["system_attr"];
                        SetTrialSystemAttrFromJObject(trialId, systemAttr);
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
            EnsureInitialized();

            if (!_studyNameToIdIndex.ContainsKey(studyName))
            {
                var study = new Study.Study(this, _nextStudyId, studyName, studyDirections);
                _studies.Add(_nextStudyId, study);
                _studyNameToIdIndex[studyName] = _nextStudyId;
                _nextStudyId++;
            }
            return _nextStudyId;
        }

        public int CreateNewTrial(int studyId, Trial.Trial templateTrial = null)
        {
            EnsureInitialized();

            Trial.Trial trial = templateTrial ?? new Trial.Trial();
            trial.TrialId = _trialId++;
            trial.Number = _studies[studyId].Trials.Count;
            _studies[studyId].Trials.Add(trial);
            _trialCache[trial.TrialId] = trial;
            return _trialId;
        }

        public void DeleteStudy(int studyId)
        {
            EnsureInitialized();

            if (_studies.TryGetValue(studyId, out Study.Study study))
            {
                foreach (Trial.Trial trial in study.Trials)
                {
                    _trialCache.Remove(trial.TrialId);
                }

                _studyNameToIdIndex.Remove(study.StudyName);
                _studies.Remove(studyId);
            }
        }

        public Study.Study[] GetAllStudies()
        {
            EnsureInitialized();

            return _studies.Values.ToArray();
        }

        public Trial.Trial[] GetAllTrials(int studyId, bool deepcopy = true)
        {
            EnsureInitialized();

            if (!_studies.TryGetValue(studyId, out Study.Study study))
            {
                throw new KeyNotFoundException($"Study with ID '{studyId}' not found.");
            }

            return study.Trials.ToArray();
        }

        public Trial.Trial GetBestTrial(int studyId)
        {
            EnsureInitialized();

            if (!_studies.TryGetValue(studyId, out Study.Study study))
            {
                throw new KeyNotFoundException($"Study with ID '{studyId}' not found.");
            }

            var allTrials = study.Trials.Where(trial => trial.State == TrialState.COMPLETE).ToList();

            if (allTrials.Count == 0)
            {
                return null;
            }

            StudyDirection[] directions = study.Directions;
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
            EnsureInitialized();

            if (!_studies.TryGetValue(studyId, out Study.Study study))
            {
                throw new KeyNotFoundException($"Study with ID '{studyId}' not found.");
            }

            return study.Trials.Count;
        }

        public StudyDirection[] GetStudyDirections(int studyId)
        {
            EnsureInitialized();

            if (!_studies.TryGetValue(studyId, out Study.Study study))
            {
                throw new KeyNotFoundException($"Study with ID '{studyId}' not found.");
            }

            return study.Directions;
        }

        public int GetStudyIdFromName(string studyName)
        {
            EnsureInitialized();

            if (_studyNameToIdIndex.TryGetValue(studyName, out int studyId))
            {
                return studyId;
            }

            throw new KeyNotFoundException($"Study with name '{studyName}' not found.");
        }

        public string GetStudyNameFromId(int studyId)
        {
            EnsureInitialized();

            if (!_studies.TryGetValue(studyId, out Study.Study study))
            {
                throw new KeyNotFoundException($"Study with ID '{studyId}' not found.");
            }

            return study.StudyName;
        }

        public Dictionary<string, object> GetStudySystemAttrs(int studyId)
        {
            EnsureInitialized();

            if (!_studies.TryGetValue(studyId, out Study.Study study))
            {
                throw new KeyNotFoundException($"Study with ID '{studyId}' not found.");
            }

            return study.SystemAttrs;
        }

        public Dictionary<string, object> GetStudyUserAttrs(int studyId)
        {
            EnsureInitialized();

            if (!_studies.TryGetValue(studyId, out Study.Study study))
            {
                throw new KeyNotFoundException($"Study with ID '{studyId}' not found.");
            }

            return study.UserAttrs;
        }

        public Trial.Trial GetTrial(int trialId)
        {
            EnsureInitialized();

            if (_trialCache.TryGetValue(trialId, out Trial.Trial trial))
            {
                return trial;
            }

            return _studies.Values.First(s => s.Trials.Any(t => t.TrialId == trialId))
                           .Trials.First(t => t.TrialId == trialId);
        }

        public int GetTrialIdFromStudyIdTrialNumber(int studyId, int trialNumber)
        {
            EnsureInitialized();

            if (!_studies.TryGetValue(studyId, out Study.Study study))
            {
                throw new KeyNotFoundException($"Study with ID '{studyId}' not found.");
            }

            Trial.Trial trial = study.Trials.FirstOrDefault(t => t.Number == trialNumber);
            if (trial == null)
            {
                throw new KeyNotFoundException($"Trial with number '{trialNumber}' not found in study '{studyId}'.");
            }
            return trial.TrialId;
        }

        public int GetTrialNumberFromId(int trialId)
        {
            EnsureInitialized();

            if (!_trialCache.TryGetValue(trialId, out Trial.Trial trial))
            {
                throw new KeyNotFoundException($"Trial with ID '{trialId}' not found.");
            }

            return trial.Number;
        }

        public double GetTrialParam(int trialId, string paramName)
        {
            EnsureInitialized();

            if (!_trialCache.TryGetValue(trialId, out Trial.Trial trial))
            {
                throw new KeyNotFoundException($"Trial with ID {trialId} not found.");
            }

            if (!trial.Params.TryGetValue(paramName, out object paramValue))
            {
                throw new KeyNotFoundException($"Parameter '{paramName}' not found in trial {trialId}.");
            }

            return (double)paramValue;
        }

        public Dictionary<string, object> GetTrialParams(int trialId)
        {
            EnsureInitialized();

            if (!_trialCache.TryGetValue(trialId, out Trial.Trial trial))
            {
                throw new KeyNotFoundException($"Trial with ID {trialId} not found.");
            }

            return trial.Params.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public Dictionary<string, object> GetTrialSystemAttrs(int trialId)
        {
            EnsureInitialized();

            if (!_trialCache.TryGetValue(trialId, out Trial.Trial trial))
            {
                throw new KeyNotFoundException($"Trial with ID {trialId} not found.");
            }

            return trial.SystemAttrs.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public Dictionary<string, object> GetTrialUserAttrs(int trialId)
        {
            EnsureInitialized();

            if (!_trialCache.TryGetValue(trialId, out Trial.Trial trial))
            {
                throw new KeyNotFoundException($"Trial with ID {trialId} not found.");
            }

            return trial.UserAttrs.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public void RemoveSession()
        {
            throw new NotImplementedException();
        }

        public void SetStudySystemAttr(int studyId, string key, object value)
        {
            EnsureInitialized();

            if (!_studies.TryGetValue(studyId, out Study.Study study))
            {
                throw new KeyNotFoundException($"Study with ID '{studyId}' not found.");
            }

            study.SystemAttrs[key] = value;
        }

        public void SetStudyUserAttr(int studyId, string key, object value)
        {
            EnsureInitialized();

            if (!_studies.TryGetValue(studyId, out Study.Study study))
            {
                throw new KeyNotFoundException($"Study with ID '{studyId}' not found.");
            }

            study.UserAttrs[key] = value;
        }

        public void SetTrailParam(int trialId, string paramName, double paramValueInternal, object distribution)
        {
            EnsureInitialized();

            if (!_trialCache.TryGetValue(trialId, out Trial.Trial trial))
            {
                throw new KeyNotFoundException($"Trial with ID {trialId} not found.");
            }

            trial.Params[paramName] = paramValueInternal;
        }

        public void SetTrialIntermediateValue(int trialId, int intermediateStep, double intermediateValue)
        {
            throw new NotImplementedException();
        }

        public bool SetTrialStateValue(int trialId, TrialState state, double[] values = null)
        {
            EnsureInitialized();

            if (!_trialCache.TryGetValue(trialId, out Trial.Trial trial))
            {
                throw new KeyNotFoundException($"Trial with ID {trialId} not found.");
            }

            trial.State = state;
            if (values != null)
            {
                trial.Values = values;
            }

            return true;
        }

        public void SetTrialSystemAttr(int trialId, string key, object value)
        {
            EnsureInitialized();

            if (!_trialCache.TryGetValue(trialId, out Trial.Trial trial))
            {
                throw new KeyNotFoundException($"Trial with ID {trialId} not found.");
            }

            trial.SystemAttrs[key] = value;
        }

        public void SetTrialUserAttr(int trialId, string key, object value)
        {
            EnsureInitialized();

            if (!_trialCache.TryGetValue(trialId, out Trial.Trial trial))
            {
                throw new KeyNotFoundException($"Trial with ID {trialId} not found.");
            }

            trial.UserAttrs[key] = value;
        }
    }
}
