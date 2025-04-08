using System;
using System.Collections.Generic;
using System.Globalization;

using Grasshopper.Kernel;

using Tunny.Component.Params;
using Tunny.Core.Input;
using Tunny.Core.Util;
using Tunny.Type;
using Tunny.Util;

namespace Tunny.Component.Operation
{
    [LoggingAspect]
    public class ConstructFishEgg : GH_Component
    {
        private readonly List<FishEgg> _fishEggs = new List<FishEgg>();
        private bool _skipIfExist;
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        public ConstructFishEgg()
          : base("Construct Fish Egg", "ConstrFEgg",
            "You can specify the initial individual that Tunny will run. Try your golden egg!",
            "Tunny", "Operation")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Variables", "Vars", "Variables pair to enqueue optimize.", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Lay Egg", "Lay", "If true, add an egg", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Clear", "Clear", "If true, clear eggs", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Skip If Exist", "Skip", "If true, skip if the same egg already exists", GH_ParamAccess.item, true);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new Param_FishEgg(), "FishEgg", "FishEgg", "These eggs are enqueued for optimization and become fish.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool lay = false;
            bool clear = false;
            if (!DA.GetData(1, ref lay)) { return; }
            if (!DA.GetData(2, ref clear)) { return; }
            if (!DA.GetData(3, ref _skipIfExist)) { return; }

            if (clear)
            {
                _fishEggs.Clear();
                return;
            }

            if (lay)
            {
                LayFishEgg();
            }

            DA.SetDataList(0, _fishEggs);
        }

        private void LayFishEgg()
        {
            var ghIO = new GrasshopperInOut(this, getVariableOnly: true);
            List<VariableBase> variables = ghIO.Variables;
            AddVariablesToFishEgg(variables);
        }

        private void AddVariablesToFishEgg(IEnumerable<VariableBase> variables)
        {
            var egg = new FishEgg(_skipIfExist);
            foreach (VariableBase variable in variables)
            {
                string name = variable.NickName;
                switch (variable)
                {
                    case NumberVariable number:
                        egg.AddParam(name, number.Value.ToString(CultureInfo.InvariantCulture));
                        break;
                    case CategoricalVariable category:
                        egg.AddParam(name, category.SelectedItem);
                        break;
                }
            }
            _fishEggs.Add(egg);
        }

        public override void CreateAttributes()
        {
            m_attributes = new ConstructFishEggAttributes(this);
        }

        protected override System.Drawing.Bitmap Icon => Resources.Resource.ConstructFishEgg;
        public override Guid ComponentGuid => new Guid("00CC0C86-687F-4A28-93CF-30A1E361A7D5");
    }
}
