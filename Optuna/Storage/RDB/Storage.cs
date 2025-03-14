using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

using Optuna.Study;
using Optuna.Trial;

namespace Optuna.Storage.RDB
{
    public class SqliteStorage : IOptunaStorage
    {
        private readonly Dictionary<int, Study.Study> _studies = new Dictionary<int, Study.Study>();
        private readonly SQLiteConnectionStringBuilder _sqliteConnection;
        private int _nextStudyId;

        public SqliteStorage(string filePath, bool createIfNotExist = false)
        {
            _sqliteConnection = new SQLiteConnectionStringBuilder
            {
                DataSource = filePath,
                Version = 3,
            };
            if (!File.Exists(filePath))
            {
                if (!createIfNotExist)
                {
                    throw new ArgumentException("The specified database does not exist.");
                }
                else
                {
                    SQLiteConnection.CreateFile(filePath);
                    CreateBaseTables();
                }
            }

            GetAllStudies();
        }

        public void CheckTrialIsUpdatable(int trialId, TrialState trialState)
        {
            throw new NotImplementedException();
        }

        public int CreateNewStudy(StudyDirection[] studyDirections, string studyName)
        {
            long maxLength;
            using (var connection = new SQLiteConnection(_sqliteConnection.ToString()))
            {
                connection.Open();
                using (var command = new SQLiteCommand("SELECT COUNT(*) FROM studies", connection))
                {
                    maxLength = (long)command.ExecuteScalar();
                }
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = "SELECT COUNT(*) FROM studies WHERE study_name = @SearchName;";
                    command.Parameters.AddWithValue("@SearchName", studyName);
                    bool hasTargetStudyName = (long)command.ExecuteScalar() > 0;
                    if (hasTargetStudyName)
                    {
                        throw new InvalidOperationException($"The study name '{studyName}' already exists.");
                    }
                }
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = "INSERT INTO studies (study_name) VALUES (@studyName);";
                    command.Parameters.AddWithValue("@studyName", studyName);
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand(connection))
                {
                    for (int i = 0; i < studyDirections.Length; i++)
                    {
                        StudyDirection direction = studyDirections[i];
                        command.CommandText = "INSERT INTO study_directions (direction, study_id, objective) VALUES (@direction, @studyId, @objective);";
                        command.Parameters.AddWithValue("@direction", direction.ToString().ToUpperInvariant());
                        command.Parameters.AddWithValue("@studyId", maxLength + 1);
                        command.Parameters.AddWithValue("@objective", i);
                        command.ExecuteNonQuery();
                    }
                }
                connection.Close();
            }
            _nextStudyId = (int)maxLength + 1;
            _studies[_nextStudyId] = new Study.Study(this, _nextStudyId, studyName, studyDirections);
            return _nextStudyId++;
        }

        private void CreateBaseTables()
        {
            var commands = new StringBuilder();
            commands.Append("CREATE TABLE IF NOT EXISTS alembic_version(");
            commands.Append("   version_num VARCHAR(32) PRIMARY KEY NOT NULL");
            commands.Append(");");
            commands.Append("CREATE TABLE IF NOT EXISTS studies(");
            commands.Append("   study_id INTEGER PRIMARY KEY NOT NULL,");
            commands.Append("   study_name VARCHAR(512) NOT NULL");
            commands.Append(");");
            commands.Append("CREATE TABLE IF NOT EXISTS study_directions(");
            commands.Append("   study_direction_id INTEGER PRIMARY KEY NOT NULL,");
            commands.Append("   direction VARCHAR(8) NOT NULL,");
            commands.Append("   study_id INTEGER NOT NULL,");
            commands.Append("   objective INTEGER NOT NULL");
            commands.Append(");");
            commands.Append("CREATE TABLE IF NOT EXISTS study_system_attributes(");
            commands.Append("   study_system_attribute_id INTEGER PRIMARY KEY NOT NULL,");
            commands.Append("   study_id INTEGER,");
            commands.Append("   key VARCHAR(512),");
            commands.Append("   value_json TEXT");
            commands.Append(");");
            commands.Append("CREATE TABLE IF NOT EXISTS study_user_attributes(");
            commands.Append("   study_user_attribute_id INTEGER PRIMARY KEY NOT NULL,");
            commands.Append("   study_id INTEGER,");
            commands.Append("   key VARCHAR(512),");
            commands.Append("   value_json TEXT");
            commands.Append(");");
            commands.Append("CREATE TABLE IF NOT EXISTS trial_heartbeats(");
            commands.Append("   trial_heartbeat_id INTEGER PRIMARY KEY NOT NULL,");
            commands.Append("   trial_id INTEGER NOT NULL,");
            commands.Append("   heartbeat DATETIME NOT NULL");
            commands.Append(");");
            commands.Append("CREATE TABLE IF NOT EXISTS trial_intermediate_values(");
            commands.Append("   trial_intermediate_value_id INTEGER PRIMARY KEY NOT NULL,");
            commands.Append("   trial_id INTEGER NOT NULL,");
            commands.Append("   step INTEGER NOT NULL,");
            commands.Append("   intermediate_value FLOAT,");
            commands.Append("   intermediate_value_type VARCHAR(7) NOT NULL");
            commands.Append(");");
            commands.Append("CREATE TABLE IF NOT EXISTS trial_params(");
            commands.Append("   param_id INTEGER PRIMARY KEY NOT NULL,");
            commands.Append("   trial_id INTEGER,");
            commands.Append("   param_name VARCHAR(512),");
            commands.Append("   param_value FLOAT,");
            commands.Append("   distribution_json TEXT");
            commands.Append(");");
            commands.Append("CREATE TABLE IF NOT EXISTS trial_system_attributes(");
            commands.Append("   trial_system_attribute_id INTEGER PRIMARY KEY NOT NULL,");
            commands.Append("   trial_id INTEGER,");
            commands.Append("   key VARCHAR(512),");
            commands.Append("   value_json TEXT");
            commands.Append(");");
            commands.Append("CREATE TABLE IF NOT EXISTS trial_user_attributes(");
            commands.Append("   trial_user_attribute_id INTEGER PRIMARY KEY NOT NULL,");
            commands.Append("   trial_id INTEGER,");
            commands.Append("   key VARCHAR(512),");
            commands.Append("   value_json TEXT");
            commands.Append(");");
            commands.Append("CREATE TABLE IF NOT EXISTS trial_values(");
            commands.Append("   trial_value_id INTEGER PRIMARY KEY NOT NULL,");
            commands.Append("   trial_id INTEGER NOT NULL,");
            commands.Append("   objective INTEGER NOT NULL,");
            commands.Append("   value FLOAT");
            commands.Append("   value_type VARCHAR(7) NOT NULL");
            commands.Append(");");
            commands.Append("CREATE TABLE IF NOT EXISTS trials(");
            commands.Append("   trial_id INTEGER PRIMARY KEY NOT NULL,");
            commands.Append("   number INTEGER,");
            commands.Append("   study_id INTEGER,");
            commands.Append("   state VARCHAR(8) NOT NULL,");
            commands.Append("   datetime_start DATETIME,");
            commands.Append("   datetime_complete DATETIME");
            commands.Append(");");
            commands.Append("CREATE TABLE IF NOT EXISTS version_info(");
            commands.Append("   version_info_id INTEGER PRIMARY KEY NOT NULL,");
            commands.Append("   schema_version INTEGER,");
            commands.Append("   library_version VARCHAR(256)");
            commands.Append(");");

            using (var connection = new SQLiteConnection(_sqliteConnection.ToString()))
            {
                connection.Open();
                using (var command = new SQLiteCommand(commands.ToString(), connection))
                {
                    command.ExecuteNonQuery();
                }
                SetAlembicVersion(connection);
                SetVersionInfo(connection);
                connection.Close();
            }
        }

        private static void SetVersionInfo(SQLiteConnection connection)
        {
            long columnLength;
            using (var command = new SQLiteCommand("SELECT COUNT(*) FROM version_info", connection))
            {
                columnLength = (long)command.ExecuteScalar();
            }
            if (columnLength > 0)
            {
                return;
            }
            using (var command = new SQLiteCommand(connection))
            {
                command.CommandText = "INSERT INTO version_info (schema_version, library_version) VALUES (@schema, @library);";
                command.Parameters.AddWithValue("@schema", 12);
                command.Parameters.AddWithValue("@library", "3.4.0");
                command.ExecuteNonQuery();
            }
        }

        private static void SetAlembicVersion(SQLiteConnection connection)
        {
            long columnLength;
            using (var command = new SQLiteCommand("SELECT COUNT(*) FROM alembic_version", connection))
            {
                columnLength = (long)command.ExecuteScalar();
            }
            if (columnLength > 0)
            {
                return;
            }
            using (var command = new SQLiteCommand(connection))
            {
                command.CommandText = "INSERT INTO alembic_version (version_num) VALUES (@versionNum);";
                command.Parameters.AddWithValue("@versionNum", "v3.2.0.a");
                command.ExecuteNonQuery();
            }
        }

        public int CreateNewTrial(int studyId, Trial.Trial templateTrial = null)
        {
            if (templateTrial != null)
            {
                throw new NotImplementedException();
            }

            using (var connection = new SQLiteConnection(_sqliteConnection.ToString()))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = "INSERT INTO trials (number, study_id, datetime_start) VALUES (@number, @studyId, @datetimeStart);";
                    command.Parameters.AddWithValue("@number", 0);
                    command.ExecuteNonQuery();
                }
            }

            return 2;
        }

        public void DeleteStudy(int studyId)
        {
            throw new NotImplementedException();
        }

        public Study.Study[] GetAllStudies()
        {
            using (var connection = new SQLiteConnection(_sqliteConnection.ToString()))
            {
                connection.Open();
                using (var command = new SQLiteCommand("SELECT study_id, study_name FROM studies", connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int studyId = reader.GetInt32(0);
                            string studyName = reader.GetString(1);
                            var study = new Study.Study(this, studyId, studyName, GetStudyDirections(studyId));
                            foreach (KeyValuePair<string, object> attr in GetStudySystemAttrs(studyId))
                            {
                                study.SystemAttrs.Add(attr.Key, attr.Value);
                            }
                            foreach (KeyValuePair<string, object> attr in GetStudyUserAttrs(studyId))
                            {
                                study.UserAttrs.Add(attr.Key, attr.Value);
                            }
                            _studies[studyId] = study;
                        }
                    }
                }
                connection.Close();
            }

            foreach (KeyValuePair<int, Study.Study> study in _studies)
            {
                study.Value.Trials.AddRange(GetAllTrials(study.Key));
            }

            return _studies.Values.ToArray();
        }

        public void RemoveSession()
        {
            throw new NotImplementedException();
        }

        public void SetStudySystemAttr(int studyId, string key, object value)
        {
            throw new NotImplementedException();
        }

        public void SetStudyUserAttr(int studyId, string key, object value)
        {
            throw new NotImplementedException();
        }

        public void SetTrailParam(int trialId, string paramName, double paramValueInternal, object distribution)
        {
            throw new NotImplementedException();
        }

        public void SetTrialIntermediateValue(int trialId, int intermediateStep, double intermediateValue)
        {
            throw new NotImplementedException();
        }

        public bool SetTrialStateValue(int trialId, TrialState state, double[] values = null)
        {
            throw new NotImplementedException();
        }

        public void SetTrialSystemAttr(int trialId, string key, object value)
        {
            throw new NotImplementedException();
        }

        public void SetTrialUserAttr(int trialId, string key, object value)
        {
            throw new NotImplementedException();
        }

        public int GetStudyIdFromName(string studyName)
        {
            throw new NotImplementedException();
        }

        public string GetStudyNameFromId(int studyId)
        {
            throw new NotImplementedException();
        }

        public StudyDirection[] GetStudyDirections(int studyId)
        {
            if (_studies.TryGetValue(studyId, out Study.Study value))
            {
                return value.Directions;
            }
            else
            {
                var directions = new List<StudyDirection>();
                using (var connection = new SQLiteConnection(_sqliteConnection.ToString()))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(connection))
                    {
                        command.CommandText = "SELECT direction FROM study_directions WHERE study_id = @studyId;";
                        command.Parameters.AddWithValue("@studyId", studyId);
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
# if NET48
                                directions.Add((StudyDirection)Enum.Parse(typeof(StudyDirection), reader.GetString(0), true));
#else
                                directions.Add(Enum.Parse<StudyDirection>(reader.GetString(0), true));
#endif
                            }
                        }
                    }
                    connection.Close();
                }
                return directions.ToArray();
            }
        }

        public Dictionary<string, object> GetStudyUserAttrs(int studyId)
        {
            var studyUserAttrs = new Dictionary<string, object>();
            using (var connection = new SQLiteConnection(_sqliteConnection.ToString()))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = "SELECT key, value_json FROM study_user_attributes WHERE study_id = @studyId;";
                    command.Parameters.AddWithValue("@studyId", studyId);
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            GetAttrs(studyUserAttrs, reader);
                        }
                    }
                }
                connection.Close();
            }
            return studyUserAttrs;
        }

        public Dictionary<string, object> GetStudySystemAttrs(int studyId)
        {
            var studySystemAttrs = new Dictionary<string, object>();
            using (var connection = new SQLiteConnection(_sqliteConnection.ToString()))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = "SELECT key, value_json FROM study_system_attributes WHERE study_id = @studyId;";
                    command.Parameters.AddWithValue("@studyId", studyId);
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            GetAttrs(studySystemAttrs, reader);
                        }
                    }
                }
                connection.Close();
            }
            return studySystemAttrs;
        }

        public int GetTrialIdFromStudyIdTrialNumber(int studyId, int trialNumber)
        {
            throw new NotImplementedException();
        }

        public int GetTrialNumberFromId(int trialId)
        {
            throw new NotImplementedException();
        }

        public double GetTrialParam(int trialId, string paramName)
        {
            throw new NotImplementedException();
        }

        public Trial.Trial GetTrial(int trialId)
        {
            var trial = new Trial.Trial();
            using (var connection = new SQLiteConnection(_sqliteConnection.ToString()))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = "SELECT number, state, datetime_start, datetime_complete FROM trials WHERE trial_id = @trialId;";
                    command.Parameters.AddWithValue("@trialId", trialId);
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DateTime completeTime = reader.GetValue(3) is DBNull ? DateTime.MaxValue : reader.GetDateTime(3);
                            trial = new Trial.Trial
                            {
                                TrialId = trialId,
                                Number = reader.GetInt32(0),
# if NET48
                                State = (TrialState)Enum.Parse(typeof(TrialState), reader.GetString(1), true),
#else
                                State = Enum.Parse<TrialState>(reader.GetString(1), true),
#endif
                                DatetimeStart = reader.GetDateTime(2),
                                DatetimeComplete = completeTime
                            };
                        }
                    }
                }
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = "SELECT value FROM trial_values WHERE trial_id = @trialId;";
                    command.Parameters.AddWithValue("@trialId", trialId);
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        var objective = new List<double>();
                        while (reader.Read())
                        {
                            objective.Add(reader.GetDouble(0));
                        }
                        trial.Values = objective.ToArray();
                    }
                }
                trial.Params = GetTrialParams(trialId);
                trial.SystemAttrs = GetTrialSystemAttrs(trialId);
                trial.UserAttrs = GetTrialUserAttrs(trialId);
                connection.Close();
            }
            return trial;
        }

        public Trial.Trial[] GetAllTrials(int studyId, bool deepcopy = true)
        {
            var trials = new List<Trial.Trial>();
            using (var connection = new SQLiteConnection(_sqliteConnection.ToString()))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = "SELECT trial_id FROM trials WHERE study_id = @studyId;";
                    command.Parameters.AddWithValue("@studyId", studyId);
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            trials.Add(GetTrial(reader.GetInt32(0)));
                        }
                    }
                }
                connection.Close();
            }
            return trials.ToArray();
        }

        public int GetNTrials(int studyId)
        {
            if (!_studies.ContainsKey(studyId))
            {
                GetAllStudies();
            }
            return _studies[studyId].Trials.Count;
        }

        public Trial.Trial GetBestTrial(int studyId)
        {
            if (!_studies.ContainsKey(studyId))
            {
                GetAllStudies();
            }

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

        public Dictionary<string, object> GetTrialParams(int trialId)
        {
            var trialParams = new Dictionary<string, object>();
            using (var connection = new SQLiteConnection(_sqliteConnection.ToString()))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = "SELECT param_name, param_value FROM trial_params WHERE trial_id = @trialId;";
                    command.Parameters.AddWithValue("@trialId", trialId);
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            trialParams.Add(reader.GetString(0), reader.GetValue(1));
                        }
                    }
                }
                connection.Close();
            }
            return trialParams;
        }

        public Dictionary<string, object> GetTrialUserAttrs(int trialId)
        {
            var trialUserAttrs = new Dictionary<string, object>();
            using (var connection = new SQLiteConnection(_sqliteConnection.ToString()))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = "SELECT key, value_json FROM trial_user_attributes WHERE trial_id = @trialId;";
                    command.Parameters.AddWithValue("@trialId", trialId);
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            GetAttrs(trialUserAttrs, reader);
                        }
                    }
                }
                connection.Close();
            }
            return trialUserAttrs;
        }

        public Dictionary<string, object> GetTrialSystemAttrs(int trialId)
        {
            var trialSystemAttrs = new Dictionary<string, object>();
            using (var connection = new SQLiteConnection(_sqliteConnection.ToString()))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = "SELECT key, value_json FROM trial_system_attributes WHERE trial_id = @trialId;";
                    command.Parameters.AddWithValue("@trialId", trialId);
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            GetAttrs(trialSystemAttrs, reader);
                        }
                    }
                }
                connection.Close();
            }
            return trialSystemAttrs;
        }

        private static void GetAttrs(Dictionary<string, object> attrs, SQLiteDataReader reader)
        {
            string key = reader.GetString(0);
            string value = reader.GetString(1);
            if (!string.IsNullOrEmpty(value) && value.Contains('[') && value.Contains(']'))
            {
                //FIXME: This is a hack to handle the case where the value is a list of strings.
                if (key.Contains("preference"))
                {
                    attrs.Add(key, value);
                }
                else
                {
                    try
                    {
                        string[] values = JsonConvert.DeserializeObject<string[]>(value);
                        attrs.Add(reader.GetString(0), values);
                    }
                    catch (JsonReaderException)
                    {
                        if (value.Contains('"'))
                        {
                            value = value.Replace("\"", "");
                        }
                        attrs.Add(reader.GetString(0), new[] { value });
                    }
                }
            }
            else
            {
                if (value.Contains('"'))
                {
                    value = value.Replace("\"", "");
                }
                attrs.Add(reader.GetString(0), new[] { value });
            }
        }
    }
}
