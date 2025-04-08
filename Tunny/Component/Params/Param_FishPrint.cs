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
    public class Param_FishPrint : GH_PersistentParam<GH_FishPrint>
    {
        public override GH_Exposure Exposure => GH_Exposure.primary;

        public Param_FishPrint()
          : base("Fish Print", "Print",
            "Image data as the objective function to be used in human-in-the-loop optimization.",
            "Tunny", "Params")
        {
        }

        protected override GH_GetterResult Prompt_Singular(ref GH_FishPrint value) => GH_GetterResult.success;
        protected override GH_GetterResult Prompt_Plural(ref List<GH_FishPrint> values) => GH_GetterResult.success;
        protected override Bitmap Icon => Resource.ParamFishPrintIcon;
        public override Guid ComponentGuid => new Guid("c67011c5-4d92-48df-bcbf-31c466a04828");
    }
}
