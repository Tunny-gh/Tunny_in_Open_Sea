using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;

using GH_IO.Serialization;

using Grasshopper.Kernel.Types;

using Rhino.Geometry;

using Tunny.Util;

namespace Tunny.Type
{
    public class GH_FishAttribute : GH_Goo<Dictionary<string, object>>
    {
        public GH_FishAttribute()
        {
        }

        public GH_FishAttribute(Dictionary<string, object> internalData) : base(internalData)
        {
        }

        public GH_FishAttribute(GH_Goo<Dictionary<string, object>> other) : base(other)
        {
        }

        public override bool IsValid => Value != null;
        public override string TypeName => "FishAttribute";
        public override string TypeDescription => "DictionaryGoo for grasshopper";
        public override IGH_Goo Duplicate() => new GH_FishAttribute { Value = Value };

        public override bool Read(GH_IReader reader)
        {
            string base64 = string.Empty;
            if (reader.TryGetString("FishAttribute_bin", ref base64))
            {
                Value = FromBase64(base64);
            }
            return base.Read(reader);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetString("FishAttribute_bin", ToBase64(Value));
            return base.Write(writer);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (KeyValuePair<string, object> attr in Value)
            {
                string valueStrings = string.Empty;
                switch (attr.Value)
                {
                    case List<object> obj:
                        valueStrings = string.Join(", ", obj);
                        break;
                    case List<string> str:
                        valueStrings = string.Join(", ", str);
                        break;
                    case List<double> num:
                        valueStrings = string.Join(", ", num);
                        break;
                    case List<int> @int:
                        valueStrings = string.Join(", ", @int);
                        break;
                    case List<GeometryBase> geo:
                        IEnumerable<string> geometryStrings = geo.Select(g => GooConverter.GeometryBaseToGoo(g).ToString());
                        valueStrings = string.Join(", ", geometryStrings);
                        break;
                    default:
                        valueStrings = attr.Value.ToString();
                        break;
                }
                sb.AppendLine("  " + attr.Key + ": " + valueStrings);
            }
            return sb.ToString();
        }

        public override bool CastFrom(object source)
        {
            if (source is Dictionary<string, object> dict)
            {
                Value = dict;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool CastTo<T>(ref T target)
        {
            target = default;
            if (typeof(T).IsAssignableFrom(typeof(Dictionary<string, object>)))
            {
                target = (T)(object)Value;
                return true;
            }
            else
            {
                return false;
            }
        }

        private static Dictionary<string, object> FromBase64(string base64)
        {
            byte[] bytes = Convert.FromBase64String(base64);
            string jsonString = Encoding.UTF8.GetString(bytes);
            return JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString);
        }

        private static string ToBase64(Dictionary<string, object> value)
        {
            string jsonString = JsonSerializer.Serialize(value);
            byte[] bytes = Encoding.UTF8.GetBytes(jsonString);
            return Convert.ToBase64String(bytes);
        }
    }
}
