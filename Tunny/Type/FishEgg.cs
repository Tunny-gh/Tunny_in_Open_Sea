using System;
using System.Collections.Generic;
using System.Text;

using Optuna.Study;

using Python.Runtime;

using Tunny.Core.Util;

namespace Tunny.Type
{
    [Serializable]
    public class FishEgg
    {
        public int RegisteredParamsCount => _paramDict.Count;
        private bool _skipIfExist;
        private Dictionary<string, string> _paramDict;
        private Dictionary<string, string> _attrDict;

        public FishEgg()
        {
            TLog.MethodStart();
            Initialize(true);
        }

        public FishEgg(bool skipIfExist)
        {
            TLog.MethodStart();
            Initialize(skipIfExist);
        }

        private void Initialize(bool skipIfExist)
        {
            _skipIfExist = skipIfExist;
            _paramDict = new Dictionary<string, string>();
            _attrDict = new Dictionary<string, string>();
            AddAttr("TunnyInfo", "Trial with FishEgg");
        }

        public override string ToString()
        {
            TLog.MethodStart();
            var sb = new StringBuilder();
            foreach (KeyValuePair<string, string> item in _paramDict)
            {
                string str = $"\"{item.Key}\": {item.Value}, ";
                sb.Append(str);
            }
            sb.Append("SkipIfExist: " + _skipIfExist);
            return sb.ToString();
        }

        public void AddParam(string key, string value)
        {
            TLog.MethodStart();
            _paramDict.Add(key, value);
        }

        public void AddAttr(string key, string value)
        {
            TLog.MethodStart();
            _attrDict.Add(key, value);
        }

        internal PyDict GetParamPyDict()
        {
            TLog.MethodStart();
            var dict = new PyDict();
            foreach (KeyValuePair<string, string> item in _paramDict)
            {
                var key = new PyString(item.Key);
                PyObject value = double.TryParse(item.Value, out double result)
                    ? new PyFloat(result)
                    : (PyObject)new PyString(item.Value);
                dict.SetItem(key, value);
            }
            return dict;
        }

        private PyDict GetAttrPyDict()
        {
            TLog.MethodStart();
            var dict = new PyDict();
            foreach (KeyValuePair<string, string> item in _attrDict)
            {
                var key = new PyString(item.Key);
                PyObject value = new PyString(item.Value);
                dict.SetItem(key, value);
            }
            return dict;
        }

        public void EnqueueStudy(StudyWrapper study)
        {
            TLog.MethodStart();
            study.EnqueueTrial(GetParamPyDict(), GetAttrPyDict(), _skipIfExist);
        }
    }
}
