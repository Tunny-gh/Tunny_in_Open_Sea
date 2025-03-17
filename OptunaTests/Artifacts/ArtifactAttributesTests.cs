using System;

using Xunit;

namespace Optuna.Artifacts.Tests
{
    public class ArtifactAttributesTests
    {
        [Theory]
        [InlineData("{\"artifact_id\": \"056-a371\", \"filename\": \"artifact_model.3dm\", \"mimetype\": \"application/octet-stream\", \"encoding\": null}")]
        public void ParseAttributeStringTest(string attribute)
        {
            var artifactAttributes = ArtifactAttributes.ParseAttributeJson(attribute, "path");
            Assert.Equal("056-a371", artifactAttributes.Id);
            Assert.Equal("artifact_model.3dm", artifactAttributes.Filename);
            Assert.Equal("application/octet-stream", artifactAttributes.Mimetype);
            Assert.Null(artifactAttributes.Encoding);
            Assert.Equal("path", artifactAttributes.ArtifactDir);
        }

        [Fact]
        public void ParseAttributeStringTest_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ArtifactAttributes.ParseAttributeJson(null, null));
        }

        [Theory]
        [InlineData("{\"artifact\": \"-a371\")")]
        public void ParseAttributeStringTest_ThrowsArgumentException(string attribute)
        {
            Assert.Throws<ArgumentException>(() => ArtifactAttributes.ParseAttributeJson(attribute, null));
        }
    }
}
