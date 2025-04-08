using System;
using System.Drawing;

using Grasshopper.Kernel;

using Rhino;
using Rhino.Display;

using Tunny.Component.Params;
using Tunny.Core.Util;
using Tunny.Resources;

namespace Tunny.Component.Print
{
    [LoggingAspect]
    public class FishPrintByCapture : GH_Component
    {
        public FishPrintByCapture()
          : base("Fish Print by Capture", "FPCap",
              "Creates a capture of the currently active viewport.",
              "Tunny", "Print")
        {
        }

        public override bool IsPreviewCapable => true;

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new Param_FishPrint(), "FishPrint", "FPrint", "FishPrint", GH_ParamAccess.item);
        }

        private readonly RhinoDoc _doc = RhinoDoc.ActiveDoc;

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            RhinoView activeView = _doc.Views.ActiveView;
            Bitmap bitmap = activeView.CaptureToBitmap();

            DA.SetData(0, bitmap);
        }

        protected override Bitmap Icon => Resource.FishPrintByCapture;
        public override Guid ComponentGuid => new Guid("5CFA19E3-2FBE-45D3-8E40-EF459C7FF491");
    }
}
