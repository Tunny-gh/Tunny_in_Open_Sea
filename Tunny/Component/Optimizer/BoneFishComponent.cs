using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;

using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;

using Optuna.Study;

using Tunny.Component.Params;
using Tunny.Core.Handler;
using Tunny.Core.Settings;
using Tunny.Core.Util;
using Tunny.Eto.Common;
using Tunny.Type;

namespace Tunny.Component.Optimizer
{
    [LoggingAspect]
    public class BoneFishComponent : OptimizeComponentBase
    {
        private int _count;
        private bool _running;
        private Fish[] _allFishes;
        private string _state;
        private static CommonSharedItems CoSharedItems => CommonSharedItems.Instance;

        public BoneFishComponent()
          : base("Bone Fish", "Born",
            "Bone Fish is an optimization component wrapped in optuna without UI."
            )
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Variables", "Vars", "Connect variable number slider here.", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Objectives", "Objs", "Connect objective number component here.", GH_ParamAccess.tree);
            pManager.AddParameter(new Param_FishAttribute(), "Attributes", "Attrs", "Connect model attribute like some geometry or values here. Not required.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Artifacts", "Artfs", "Connect artifacts here. Not required.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Start", "Start", "Start optimization.", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Stop", "Stop", "Stop optimization.", GH_ParamAccess.item, false);
            Params.Input[0].Optional = true;
            Params.Input[1].Optional = true;
            Params.Input[2].Optional = true;
            Params.Input[3].Optional = true;
            Params.Input[5].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Info", "Info", "Information about the optimization.", GH_ParamAccess.item);
            pManager.AddParameter(new Param_Fish(), "All Fishes", "All", "All optimization results", GH_ParamAccess.list);
            pManager.AddParameter(new Param_Fish(), "Best Fishes", "Best", "Best optimization results", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CheckVariablesInput(Params.Input[0].Sources.Select(ghParam => ghParam.InstanceGuid));
            CheckObjectivesInput(Params.Input[1].Sources.Select(ghParam => ghParam.InstanceGuid));
            CheckArtifactsInput(Params.Input[3].Sources.Select(ghParam => ghParam.InstanceGuid));

            TSettings.TryLoadFromJson(out TSettings settings);
            string tunnyAssembleVersion = TEnvVariables.Version.ToString();
            if (settings.CheckPythonLibraries || settings.Version != tunnyAssembleVersion)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The optimization environment has not been built; launch the UI of the Tunny component once and install Python.");
                return;
            }

            bool start = false;
            bool stop = false;
            if (!DA.GetData(4, ref start)) { return; }
            if (!DA.GetData(5, ref stop)) { return; }

            if (!start)
            {
                Message = "No Opt";
                SetInfo("No optimization has been performed yet.");
            }

            if (start && _state == "Finish")
            {
                _state = "Start";
                DA.SetData(0, Info);
                DA.SetDataList(1, _allFishes);
                DA.SetDataList(2, Fishes);
                return;
            }

            if (start && stop)
            {
                CoSharedItems.IsForcedStopOptimize = true;
            }

            if (start && !_running)
            {
                _running = true;
                _count = 0;
                MakeFishPrintByCaptureToTopOrder();
                GH_DocumentEditor ghCanvas = Instances.DocumentEditor;
                ghCanvas?.DisableUI();
                Params.Output[1].ClearData();
                Params.Output[2].ClearData();

                CoSharedItems.Settings = settings;
            }

            DA.SetData(0, Info);
        }

        private void OptimizeProgressChangedHandler(object sender, ProgressChangedEventArgs e)
        {
            _count++;
            var progressState = (ProgressState)e.UserState;
            UpdateGrasshopper(progressState);
            Message = $"Trial {_count}";
        }

        private void StopOptimize(object sender, RunWorkerCompletedEventArgs e)
        {
            _running = false;
            CoSharedItems.IsForcedStopOptimize = true;

            Message = "Outputting";
            Study[] studies = CoSharedItems.Settings.Storage.GetAllStudies();
            Study study = studies.FirstOrDefault(x => x.StudyName == CoSharedItems.Settings.Optimize.StudyName);

            string versionString = (study.UserAttrs["tunny_version"] as string[])[0];
            var version = new Version(versionString);
            string[] metricNames = version <= TEnvVariables.OldStorageVersion
                ? study.UserAttrs["objective_names"] as string[]
                : study.SystemAttrs["study:metric_names"] as string[];
            _allFishes = study.Trials.Select(trial => new Fish(trial, metricNames)).ToArray();
            Params.Output[1].AddVolatileDataList(new GH_Path(0), _allFishes.Select(x => new GH_Fish(x)));
            Fishes = study.BestTrials.Select(trial => new Fish(trial, metricNames)).ToArray();
            Params.Output[2].AddVolatileDataList(new GH_Path(0), Fishes.Select(x => new GH_Fish(x)));

            _state = "Finish";
            ExpireSolution(true);

            GH_DocumentEditor ghCanvas = Instances.DocumentEditor;
            Message = "Finish";
            ghCanvas?.EnableUI();
        }

        protected override Bitmap Icon => Resources.Resource.BoneFish;
        public override Guid ComponentGuid => new Guid("c6b7612e-ee01-47ad-bc7d-713d76b274ab");
    }
}
