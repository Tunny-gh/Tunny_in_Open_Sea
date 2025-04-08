using System;
using System.Drawing;
using System.IO;

using Grasshopper.Kernel;

using Tunny.Component.Params;
using Tunny.Core.Util;
using Tunny.Resources;

namespace Tunny.Component.Print
{
    [LoggingAspect]
    public class FishPrintByPath : GH_Component
    {
        public FishPrintByPath()
          : base("Fish Print by Path", "FPPath",
              "Create Fish Print by file path.",
              "Tunny", "Print")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Path", "Path", "Create Fish Print by file path", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new Param_FishPrint(), "FishPrint", "FPrint", "FishPrint", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string path = string.Empty;
            DA.GetData(0, ref path);

            if (!CheckFilePath(path))
            {
                return;
            }

            var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            try
            {
                var bitmap = Image.FromStream(fs) as Bitmap;
                DA.SetData(0, bitmap);
            }
            catch (Exception)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Only the path of files that can be converted to Bitmap can be input.");
            }
            finally
            {
                fs.Close();
            }
        }

        private bool CheckFilePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Path is empty.");
                return false;
            }
            if (!File.Exists(path))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "File not found.");
                return false;
            }

            return true;
        }

        protected override Bitmap Icon => Resource.FishPrintByPath;
        public override Guid ComponentGuid => new Guid("8ea46ce9-d546-4fdc-a950-976134af8227");
    }
}
