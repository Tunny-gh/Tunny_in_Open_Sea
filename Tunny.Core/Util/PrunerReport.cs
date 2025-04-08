using System.IO;

using Newtonsoft.Json;

namespace Tunny.Core.Util
{
    [LoggingAspect]
    public class PrunerReport
    {
        public double IntermediateValue { get; set; }
        public string Attribute { get; set; }
        public double? TrialTellValue { get; set; }

        public static PrunerReport Deserialize(string path)
        {
            string value = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<PrunerReport>(value);
        }
    }
}
