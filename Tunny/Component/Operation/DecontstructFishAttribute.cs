using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;

using Rhino.Geometry;

using Tunny.Component.Params;
using Tunny.Resources;
using Tunny.Type;
using Tunny.Util;

namespace Tunny.Component.Operation
{
    public class DeconstructFishAttribute : GH_Component, IGH_VariableParameterComponent
    {
        private int _outputCount;
        private List<string> _keys = new List<string>();
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        public DeconstructFishAttribute()
          : base("Deconstruct Fish Attribute", "DeconFA",
              "Deconstruct Fish Attribute.",
              "Tunny", "Operation")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_FishAttribute(), "Attributes", "Attrs", "Attributes to each Trial", GH_ParamAccess.tree);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            // No output parameters
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!DA.GetDataTree(0, out GH_Structure<GH_FishAttribute> fishAttributeStructure)) { return; }

            var nicknames = Params.Output.Select(x => x.NickName).ToList();
            var outputValues = new Dictionary<string, GH_Structure<IGH_Goo>>();
            for (int i = 0; i < fishAttributeStructure.PathCount; i++)
            {
                GH_Path path = fishAttributeStructure.Paths[i];
                Dictionary<string, object> value = fishAttributeStructure.Branches[i][0].Value;

                SetOutputValues(nicknames, outputValues, path, value);
            }

            SetDataTreeToDataAccess(DA, nicknames, outputValues);
        }

        private void SetOutputValues(List<string> nicknames, Dictionary<string, GH_Structure<IGH_Goo>> outputValues, GH_Path path, Dictionary<string, object> dict)
        {
            //FIXME: This process should be done once, but foreach executes it every time.
            _keys = dict.Keys.ToList();
            foreach (KeyValuePair<string, object> pair in dict.Where(pair => nicknames.Contains(pair.Key)))
            {
                if (!outputValues.TryGetValue(pair.Key, out GH_Structure<IGH_Goo> value))
                {
                    value = new GH_Structure<IGH_Goo>();
                    outputValues.Add(pair.Key, value);
                }

                IEnumerable<IGH_Goo> goo = GetGooFromAttributeObject(pair.Value);
                foreach (IGH_Goo g in goo)
                {
                    value.Append(g, path);
                }
            }
        }

        private void SetDataTreeToDataAccess(IGH_DataAccess DA, IReadOnlyCollection<string> nicknames, Dictionary<string, GH_Structure<IGH_Goo>> outputValues)
        {
            if (Params.Output.Count == 0)
            {
                return;
            }

            for (int i = 0; i < Params.Output.Count; i++)
            {
                foreach (string nickName in nicknames.Where(nickName => Params.Output[i].NickName == nickName))
                {
                    DA.SetDataTree(i, outputValues[nickName]);
                }
            }
        }

        private static List<IGH_Goo> GetGooFromAttributeObject(object value)
        {
            switch (value)
            {
                case List<string> str:
                    return str.Select(s => new GH_String(s)).Cast<IGH_Goo>().ToList();
                case List<GeometryBase> geom:
                    return geom.Select(GooConverter.GeometryBaseToGoo).Cast<IGH_Goo>().ToList();
                default:
                    return new List<IGH_Goo>();
            }
        }

        public bool CanInsertParameter(GH_ParameterSide side, int index) => side != GH_ParameterSide.Input && (Params.Output.Count == 0 || index == Params.Output.Count);

        public bool CanRemoveParameter(GH_ParameterSide side, int index) => side != GH_ParameterSide.Input && index == Params.Output.Count - 1;

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            if (side == GH_ParameterSide.Output && _keys.Count > index)
            {
                var p = new Param_GenericObject();
                p.Name = p.NickName = _keys[index];
                p.MutableNickName = false;
                p.Access = GH_ParamAccess.tree;
                return p;
            }
            else
            {
                return null;
            }
        }

        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            return true;
        }

        public void VariableParameterMaintenance()
        {
            if (_outputCount == Params.Output.Count)
            {
                return;
            }

            _outputCount = Params.Output.Count;
            ExpireSolution(true);
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu) => Menu_AppendItem(menu, "Match outputs", new EventHandler(Menu_AutoCreateTree_Clicked));

        private void Menu_AutoCreateTree_Clicked(object sender, EventArgs e)
        {
            if (Params.Output.Count == _keys.Count)
            {
                return;
            }
            RecordUndoEvent("FishAttribute Match");
            if (Params.Output.Count < _keys.Count)
            {
                while (Params.Output.Count < _keys.Count)
                {
                    Params.RegisterOutputParam(CreateParameter(GH_ParameterSide.Output, Params.Output.Count));
                }
            }
            else if (Params.Output.Count > _keys.Count)
            {
                while (Params.Output.Count > _keys.Count)
                {
                    Params.UnregisterOutputParameter(Params.Output[Params.Output.Count - 1]);
                }
            }
            Params.OnParametersChanged();
            VariableParameterMaintenance();
            ExpireSolution(true);
        }

        protected override System.Drawing.Bitmap Icon => Resource.DeconstructFishAttribute;
        public override Guid ComponentGuid => new Guid("D3B7B64A-71BA-41BE-9B2E-B0F459886B36");
    }
}
