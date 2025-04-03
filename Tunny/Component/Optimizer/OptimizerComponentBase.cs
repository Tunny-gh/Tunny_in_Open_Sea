using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using GalapagosComponents;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;

using Tunny.Component.Params;
using Tunny.Component.Print;
using Tunny.Component.Util;
using Tunny.Core.Handler;
using Tunny.Core.TEnum;
using Tunny.Core.Util;
using Tunny.Type;
using Tunny.Util;

namespace Tunny.Component.Optimizer
{
    public class OptimizeComponentBase : GH_Component
    {
        internal GrasshopperInOut GhInOut;
        public event EventHandler<GrasshopperStates> GrasshopperStatusChanged;
        private GrasshopperStates _grasshopperStates;
        internal GrasshopperStates GrasshopperStatus
        {
            get => _grasshopperStates;
            set
            {
                _grasshopperStates = value;
                GrasshopperStatusChanged?.Invoke(this, value);
            }
        }
        internal Fish[] Fishes;
        internal string Info { get; private set; } = "No optimization has been performed yet.";

        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        public OptimizeComponentBase(string name, string nickname, string description)
          : base(name, nickname, description, "Tunny", "Optimizer")
        {
        }

        public void SetInfo(string info)
        {
            TLog.MethodStart();
            Info = info;
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            TLog.MethodStart();
            base.AppendAdditionalMenuItems(menu);
            Menu_AppendItem(menu, "Open settings.json", Menu_OpenSettingsClicked);
            Menu_AppendItem(menu, "Open environment directory", Menu_OpenDirectoryClicked);
        }

        private void Menu_OpenDirectoryClicked(object sender, EventArgs e)
        {
            TLog.MethodStart();
            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                ? "open" : "explorer.exe";
            process.StartInfo.Arguments = $"\"{TEnvVariables.TunnyEnvPath}\"";
            process.Start();
        }

        private void Menu_OpenSettingsClicked(object sender, EventArgs e)
        {
            TLog.MethodStart();
            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                ? "open" : "notepad.exe";
            process.StartInfo.Arguments = $"\"{TEnvVariables.OptimizeSettingsPath}\"";
            process.Start();
        }

        /// <summary>
        /// Change calculation order to FishPrintByCapture after all components have been calculated.
        /// </summary>
        protected void MakeFishPrintByCaptureToTopOrder()
        {
            TLog.MethodStart();
            IList<IGH_DocumentObject> objs = OnPingDocument().Objects;
            var fishPrints = new List<FishPrintByCapture>();
            foreach (IGH_DocumentObject obj in objs)
            {
                if (obj is FishPrintByCapture fp)
                {
                    fishPrints.Add(fp);
                }
            }

            if (fishPrints.Count > 0)
            {
                OnPingDocument().ArrangeObjects(fishPrints, GH_Arrange.MoveToFront);
            }
        }

        public void GhInOutInstantiate()
        {
            TLog.Info("Instantiate GrasshopperInOut");
            GhInOut = new GrasshopperInOut(this);
        }

        public void UpdateGrasshopper(ProgressState progressState)
        {
            TLog.MethodStart();
            GrasshopperStatus = GrasshopperStates.RequestProcessing;
            TLog.Debug("Requesting Grasshopper to process the solution.");
            if (progressState.IsReportOnly)
            {
                ExpireSolution(true);
            }
            else
            {
                GhInOut.NewSolution(progressState.Parameter);
            }
            GrasshopperStatus = GrasshopperStates.RequestProcessed;
            TLog.Debug("Grasshopper has processed the solution.");
        }

        public override void CreateAttributes()
        {
            TLog.MethodStart();
            m_attributes = new OptimizerAttributeBase(this, Color.DimGray, Color.Black, Color.White);
        }

        protected void CheckVariablesInput(IEnumerable<Guid> inputGuids)
        {
            TLog.MethodStart();
            foreach ((IGH_DocumentObject docObject, int _) in inputGuids.Select((guid, i) => (OnPingDocument().FindObject(guid, false), i)))
            {
                switch (docObject)
                {
                    case TunnyValueList _:
                    case GH_NumberSlider _:
                    case GalapagosGeneListObject _:
                    case Param_FishEgg _:
                        break;
                    default:
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"{docObject} input is not a valid variable.");
                        break;
                }
            }
        }

        protected void CheckObjectivesInput(IEnumerable<Guid> inputGuids)
        {
            TLog.MethodStart();
            foreach ((IGH_DocumentObject docObject, int _) in inputGuids.Select((guid, i) => (OnPingDocument().FindObject(guid, false), i)))
            {
                switch (docObject)
                {
                    case Param_Number number:
                    case Param_FishPrint fPrint:
                        break;
                    default:
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"{docObject} input is not a valid objective.");
                        break;
                }
            }
        }

        protected void CheckArtifactsInput(IEnumerable<Guid> inputGuids)
        {
            TLog.MethodStart();
            foreach ((IGH_DocumentObject docObject, int _) in inputGuids.Select((guid, i) => (OnPingDocument().FindObject(guid, false), i)))
            {
                switch (docObject)
                {
                    case Param_Geometry geometry:
                    case Param_Mesh mesh:
                    case Param_FishPrint fPrint:
                    case Param_String text:
                    case Param_FilePath filePath:
                        break;
                    default:
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"{docObject} input is not a valid artifact.");
                        break;
                }
            }
        }

        public override Guid ComponentGuid => throw new NotImplementedException();

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            throw new NotImplementedException();
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            throw new NotImplementedException();
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            throw new NotImplementedException();
        }
    }
}
