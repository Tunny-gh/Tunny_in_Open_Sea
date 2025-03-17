using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

using Optuna.Trial;

using Rhino.Geometry;

using Tunny.Core.Input;
using Tunny.Core.Util;

namespace Tunny.Type
{
    [Serializable]
    public class Fish
    {
        public int TrialNumber { get; set; }
        public Dictionary<string, object> Variables { get; set; }
        public Dictionary<string, double> Objectives { get; set; }
        public Dictionary<string, object> Attributes { get; set; }

        public Fish()
        {
            TLog.MethodStart();
        }

        public Fish(Trial trial, string[] objectiveNames)
        {
            TLog.MethodStart();
            TrialNumber = trial.Number;
            Variables = trial.Params;
            Attributes = trial.UserAttrs;
            Objectives = new Dictionary<string, double>();
            for (int i = 0; i < objectiveNames.Length; i++)
            {
                Objectives.Add(objectiveNames[i], trial.Values[i]);
            }
        }

        public Parameter[] GetParameterClassFormatVariables()
        {
            TLog.MethodStart();
            var parameters = new List<Parameter>();
            if (Variables == null)
            {
                return parameters.ToArray();
            }

            foreach (KeyValuePair<string, object> variable in Variables)
            {
                if (variable.Value is double v)
                {
                    parameters.Add(new Parameter(v));
                }
                else
                {
                    parameters.Add(new Parameter(variable.Value.ToString()));
                }
            }
            return parameters.ToArray();
        }

        public static string ToBase64(Fish fish)
        {
            TLog.MethodStart();
            string json = JsonSerializer.Serialize(fish);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            return Convert.ToBase64String(bytes);
        }

        public static Fish FromBase64(string base64String)
        {
            TLog.MethodStart();
            Fish fish;
            byte[] bytes = Convert.FromBase64String(base64String);
            using (var ms = new MemoryStream(bytes))
            {
                using (var reader = new StreamReader(ms))
                {
                    string json = reader.ReadToEnd();
                    fish = JsonSerializer.Deserialize<Fish>(json);
                }
            }
            return UpdateObjectTypeProperties(fish);
        }

        private static Fish UpdateObjectTypeProperties(Fish fish)
        {
            fish.Variables = UpdateVariables(fish);
            fish.Attributes = UpdateAttributes(fish);
            return fish;
        }

        private static Dictionary<string, object> UpdateAttributes(Fish fish)
        {
            var newAttributes = new Dictionary<string, object>();

            foreach (KeyValuePair<string, object> attrs in fish.Attributes)
            {
                if (attrs.Value is JsonElement jsonElement)
                {
                    if (jsonElement.ValueKind == JsonValueKind.Array)
                    {
                        var objs = new List<object>();
                        foreach (JsonElement item in jsonElement.EnumerateArray())
                        {
                            if (item.ValueKind == JsonValueKind.Number)
                            {
                                objs.Add(item.GetDouble());
                            }
                            else if (item.ValueKind == JsonValueKind.String &&
                                     double.TryParse(item.GetString(), out double parsedValue))
                            {
                                objs.Add(parsedValue);
                            }
                            else
                            {
                                objs.Add(item.ToString());
                            }
                        }
                        newAttributes.Add(attrs.Key, objs);
                    }
                    else if (jsonElement.ValueKind == JsonValueKind.Number)
                    {
                        newAttributes.Add(attrs.Key, jsonElement.GetDouble());
                    }
                    else if (jsonElement.ValueKind == JsonValueKind.String &&
                            double.TryParse(jsonElement.GetString(), out double parsedValue))
                    {
                        newAttributes.Add(attrs.Key, parsedValue);
                    }
                    else if (jsonElement.ValueKind == JsonValueKind.String)
                    {
                        string jsonString = jsonElement.GetString();
# if NET5_0_OR_GREATER
                        if (jsonString.StartsWith('[') &&
                            jsonString.EndsWith(']'))
#else
                        if (jsonString.StartsWith("[", StringComparison.Ordinal) &&
                            jsonString.EndsWith("]", StringComparison.Ordinal))
#endif
                        {
                            try
                            {
                                var doc = JsonDocument.Parse(jsonString);
                                var numbers = new List<double>();

                                foreach (JsonElement element in doc.RootElement.EnumerateArray())
                                {
                                    if (element.ValueKind == JsonValueKind.Number)
                                    {
                                        numbers.Add(element.GetDouble());
                                    }
                                    else if (element.ValueKind == JsonValueKind.String &&
                                             double.TryParse(element.GetString(), out double parsedNum))
                                    {
                                        numbers.Add(parsedNum);
                                    }
                                }
                                newAttributes.Add(attrs.Key, numbers);
                            }
                            catch (Exception)
                            {
                                newAttributes.Add(attrs.Key, jsonString);
                            }
                        }
                        else
                        {
                            newAttributes.Add(attrs.Key, jsonElement.ToString());
                        }
                    }
                    else if (attrs.Value is double v)
                    {
                        newAttributes.Add(attrs.Key, v);
                    }
                    else if (attrs.Value is string s && double.TryParse(s, out double parsedNum))
                    {
                        newAttributes.Add(attrs.Key, parsedNum);
                    }
                    else
                    {
                        newAttributes.Add(attrs.Key, attrs.Value.ToString());
                    }
                }
            }

            return newAttributes;
        }

        private static Dictionary<string, object> UpdateVariables(Fish fish)
        {
            var newVariables = new Dictionary<string, object>();

            foreach (KeyValuePair<string, object> variable in fish.Variables)
            {
                if (variable.Value is JsonElement jsonElement)
                {
                    if (jsonElement.ValueKind == JsonValueKind.Number)
                    {
                        newVariables.Add(variable.Key, jsonElement.GetDouble());
                    }
                    else if (jsonElement.ValueKind == JsonValueKind.String &&
                            double.TryParse(jsonElement.GetString(), out double parsedValue))
                    {
                        newVariables.Add(variable.Key, parsedValue);
                    }
                    else
                    {
                        newVariables.Add(variable.Key, jsonElement.ToString());
                    }
                }
                else if (variable.Value is double v)
                {
                    newVariables.Add(variable.Key, v);
                }
                else if (variable.Value is string s && double.TryParse(s, out double parsedValue))
                {
                    newVariables.Add(variable.Key, parsedValue);
                }
                else
                {
                    newVariables.Add(variable.Key, variable.Value.ToString());
                }
            }

            return newVariables;
        }

        public GeometryBase[] GetGeometries()
        {
            TLog.MethodStart();
            var geometries = new List<GeometryBase>();
            if (Attributes != null)
            {
                foreach (KeyValuePair<string, object> attr in Attributes)
                {
                    if (attr.Value is GeometryBase geometry)
                    {
                        geometries.Add(geometry);
                    }
                }
            }
            if (geometries.Count == 0)
            {
                geometries.Add(new Point(new Point3d(0, 0, 0)));
            }
            return geometries.ToArray();
        }
    }
}
