using System;
using System.Drawing;
using System.Linq;

using Grasshopper.Kernel;

using Tunny.Component.Optimizer;
using Tunny.Component.Params;
using Tunny.Core.Util;
using Tunny.Resources;

namespace Tunny.Component.Obsolete
{
    [LoggingAspect]
    [Obsolete("This component is obsolete in v0.11.1 and will be removed in v1.0. Please use ConstructFishAttribute instead.")]
    public class FishingComponent_v0_11_1 : UIOptimizeComponentBase
    {
        public override GH_Exposure Exposure => GH_Exposure.hidden;

        public FishingComponent_v0_11_1()
          : base("Tunny", "Tunny",
            "Tunny is an optimization component wrapped in optuna."
            )
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Variables", "Vars", "Connect variable number slider here.", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Objectives", "Objs", "Connect objective number component here.", GH_ParamAccess.tree);
            pManager.AddParameter(new Param_FishAttribute(), "Attributes", "Attrs", "Connect model attribute like some geometry or values here. Not required.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Artifacts", "Artfs", "Connect artifacts here. Not required.", GH_ParamAccess.item);
            Params.Input[0].Optional = true;
            Params.Input[1].Optional = true;
            Params.Input[2].Optional = true;
            Params.Input[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new Param_Fish(), "Fishes", "Fishes", "Fishes caught by the optimization nets.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CheckVariablesInput(Params.Input[0].Sources.Select(ghParam => ghParam.InstanceGuid));
            CheckObjectivesInput(Params.Input[1].Sources.Select(ghParam => ghParam.InstanceGuid));
            CheckArtifactsInput(Params.Input[3].Sources.Select(ghParam => ghParam.InstanceGuid));

            DA.SetDataList(0, Fishes);
        }

        protected override Bitmap Icon => Resource.TunnyIcon;
        public override Guid ComponentGuid => new Guid("701d2c47-1440-4d09-951c-386200e29b28");
    }
}
