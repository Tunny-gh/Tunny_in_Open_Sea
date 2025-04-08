using System;

using Tunny.Core.TEnum;
using Tunny.Core.Util;

namespace Tunny.Core.Settings
{
    [LoggingAspect]
    public class Optimize
    {
        public string StudyName { get; set; } = string.Empty;
        public Sampler Sampler { get; set; } = new Sampler();
        public int NumberOfTrials { get; set; } = 100;
        public bool ContinueStudy { get; set; }
        public bool CopyStudy { get; set; }
        public SamplerType SamplerType { get; set; }
        public double Timeout { get; set; }
        public GcAfterTrial GcAfterTrial { get; set; } = GcAfterTrial.HasGeometry;
        public bool ShowRealtimeResult { get; set; }
        public bool IgnoreDuplicateSampling { get; set; }
        public bool DisableViewportDrawing { get; set; }
        public bool MinimizeRhinoWindow { get; set; }

        public void ComputeAutoValue()
        {
            Sampler.Tpe.ComputeAutoValue(NumberOfTrials);
            Sampler.GP.ComputeAutoValue(NumberOfTrials);
            Sampler.BoTorch.ComputeAutoValue(NumberOfTrials);

            if (string.IsNullOrEmpty(StudyName) || StudyName.Equals("AUTO", StringComparison.OrdinalIgnoreCase))
            {
                StudyName = "no-name-" + Guid.NewGuid().ToString("D");
            }
        }
    }
}
