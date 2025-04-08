using System;
using System.Collections.Generic;
using System.Drawing;

using Grasshopper.Kernel;

using Tunny.Core.Util;
using Tunny.Resources;
using Tunny.Type;

namespace Tunny.Component.Params
{
    [LoggingAspect]
    public class Param_FishAttribute : GH_PersistentParam<GH_FishAttribute>
    {
        public override GH_Exposure Exposure => GH_Exposure.primary;

        public Param_FishAttribute()
          : base("Fish Attribute", "FishAttr",
            "Attribute information to be added to each trial of optimization.",
            "Tunny", "Params")
        {
        }

        protected override GH_GetterResult Prompt_Singular(ref GH_FishAttribute value) => GH_GetterResult.success;
        protected override GH_GetterResult Prompt_Plural(ref List<GH_FishAttribute> values) => GH_GetterResult.success;
        protected override Bitmap Icon => Resource.ParamFishAttribute;
        public override Guid ComponentGuid => new Guid("bfbd03eb-492d-4c57-8311-e7452d78a78e");
    }
}
