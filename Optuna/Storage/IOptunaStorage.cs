using System.Collections.Generic;

using Optuna.Study;
using Optuna.Trial;

namespace Optuna.Storage
{
    public interface IOptunaStorage
    {
        // Basic study manipulation
        int CreateNewStudy(StudyDirection[] studyDirections, string studyName);
        void DeleteStudy(int studyId);
        void SetStudyUserAttr(int studyId, string key, object value);
        void SetStudySystemAttr(int studyId, string key, object value);

        // Basic study access
        int GetStudyIdFromName(string studyName);
        string GetStudyNameFromId(int studyId);
        StudyDirection[] GetStudyDirections(int studyId);
        Dictionary<string, object> GetStudyUserAttrs(int studyId);
        Dictionary<string, object> GetStudySystemAttrs(int studyId);
        Study.Study[] GetAllStudies();

        // Basic trial manipulation
        int CreateNewTrial(int studyId, Trial.Trial templateTrial = null);
        void SetTrailParam(int trialId, string paramName, double paramValueInternal, object distribution);
        int GetTrialIdFromStudyIdTrialNumber(int studyId, int trialNumber);
        int GetTrialNumberFromId(int trialId);
        double GetTrialParam(int trialId, string paramName);
        bool SetTrialStateValue(int trialId, TrialState state, double[] values = null);
        void SetTrialIntermediateValue(int trialId, int intermediateStep, double intermediateValue);
        void SetTrialUserAttr(int trialId, string key, object value);
        void SetTrialSystemAttr(int trialId, string key, object value);

        // Basic trial access
        Trial.Trial GetTrial(int trialId);
        Trial.Trial[] GetAllTrials(int studyId, bool deepcopy = true);
        int GetNTrials(int studyId);
        Trial.Trial GetBestTrial(int studyId);
        Dictionary<string, object> GetTrialParams(int trialId);
        Dictionary<string, object> GetTrialUserAttrs(int trialId);
        Dictionary<string, object> GetTrialSystemAttrs(int trialId);
        void RemoveSession();
        void CheckTrialIsUpdatable(int trialId, TrialState trialState);
    }
}
