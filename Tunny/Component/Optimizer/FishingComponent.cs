using System;
using System.Drawing;
using System.Linq;

using Grasshopper.Kernel;

using Tunny.Component.Params;
using Tunny.Resources;

namespace Tunny.Component.Optimizer
{
    public class FishingComponent : UIOptimizeComponentBase
    {
        public FishingComponent()
          : base("Tunny Fishing", "Fishing",
            "Tunny is an optimization component wrapped in optuna."
            )
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Variables", "Vars", "Connect variable number slider here.", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Objectives", "Objs", "Connect objective number component here.", GH_ParamAccess.tree);
            pManager.AddParameter(new Param_FishAttribute(), "Attributes", "Attrs", "Connect model attribute like some geometry or values here. Not required.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Artifacts", "Artfs", "Connect artifacts here. Not required.", GH_ParamAccess.tree);
            Params.Input[0].Optional = true;
            Params.Input[1].Optional = true;
            Params.Input[2].Optional = true;
            Params.Input[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Info", "Info", "Optimization information.", GH_ParamAccess.item);
            pManager.AddParameter(new Param_Fish(), "Fishes", "Fishes", "Fishes caught by the optimization nets.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CheckVariablesInput(Params.Input[0].Sources.Select(ghParam => ghParam.InstanceGuid));
            CheckObjectivesInput(Params.Input[1].Sources.Select(ghParam => ghParam.InstanceGuid));
            CheckArtifactsInput(Params.Input[3].Sources.Select(ghParam => ghParam.InstanceGuid));

            DA.SetData(0, Info);
            DA.SetDataList(1, Fishes);
        }

        protected override Bitmap Icon => Resource.TunnyIcon;
        public override Guid ComponentGuid => new Guid("2c094af4-81c9-4830-b866-fbab735c122a");
    }
}
