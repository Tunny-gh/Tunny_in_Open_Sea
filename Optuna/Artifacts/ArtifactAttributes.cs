using System;

using Newtonsoft.Json;

namespace Optuna.Artifacts
{
    public class ArtifactAttributes
    {
        [JsonProperty("artifact_id")]
        public string Id { get; set; }
        [JsonProperty("filename")]
        public string Filename { get; set; }
        [JsonProperty("mimetype")]
        public string Mimetype { get; set; }
        [JsonProperty("encoding")]
        public string Encoding { get; set; }
        public string ArtifactDir { get; set; }

        public ArtifactAttributes()
        {
        }

        public static ArtifactAttributes ParseAttributeJson(string attribute, string artifactDir)
        {
            if (string.IsNullOrEmpty(attribute))
            {
                throw new ArgumentNullException(nameof(attribute), "Attribute string cannot be null or empty");
            }

            try
            {
                string jsonString = attribute;
                ArtifactAttributes artifact = JsonConvert.DeserializeObject<ArtifactAttributes>(jsonString);
                artifact.ArtifactDir = artifactDir;
                return artifact;
            }
            catch (JsonException ex)
            {
                throw new ArgumentException("Failed to parse the attribute string as artifact attribute JSON", nameof(attribute), ex);
            }
        }
    }
}
