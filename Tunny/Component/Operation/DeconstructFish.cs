using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using Tunny.Component.Params;
using Tunny.Core.Util;
using Tunny.Resources;
using Tunny.Type;

namespace Tunny.Component.Operation
{
    [LoggingAspect]
    public class DeconstructFish : GH_Component
    {
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        public DeconstructFish()
          : base("Deconstruct Fish", "DeconF",
              "Deconstruct Fish.",
              "Tunny", "Operation")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_Fish(), "Fishes", "Fishes", "Fishes", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("NumberVariables", "NumVars", "Number Variables", GH_ParamAccess.tree);
            pManager.AddTextParameter("TextVariables", "TxtVars", "Text Variables", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Objectives", "Objs", "Objectives", GH_ParamAccess.tree);
            pManager.AddParameter(new Param_FishAttribute(), "Attributes", "Attrs", "Attributes", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var fishObjects = new List<object>();
            if (!DA.GetDataList(0, fishObjects)) { return; }

            var fishes = fishObjects.Select(x => (GH_Fish)x).ToList();

            var numberVariables = new GH_Structure<GH_Number>();
            var textVariables = new GH_Structure<GH_String>();
            var objectives = new GH_Structure<GH_Number>();
            var attributes = new GH_Structure<GH_FishAttribute>();

            foreach (GH_Fish fish in fishes)
            {
                Fish value = fish.Value;
                var path = new GH_Path(0, value.TrialNumber);
                SetVariables(numberVariables, textVariables, value, path);
                SetObjectives(objectives, value, path);
                SetAttributes(attributes, value, path);
            }

            DA.SetDataTree(0, numberVariables);
            DA.SetDataTree(1, textVariables);
            DA.SetDataTree(2, objectives);
            DA.SetDataTree(3, attributes);
        }

        private static void SetVariables(GH_Structure<GH_Number> number, GH_Structure<GH_String> text, Fish value, GH_Path path)
        {
            foreach (KeyValuePair<string, object> variable in value.Variables)
            {
                if (variable.Value is double v)
                {
                    number.Append(new GH_Number(v), path);
                }
                else if (variable.Value is string s && double.TryParse(s, out double parsedValue))
                {
                    number.Append(new GH_Number(parsedValue), path);
                }
                else
                {
                    text.Append(new GH_String(variable.Value.ToString()), path);
                }
            }
        }

        private static void SetObjectives(GH_Structure<GH_Number> objectives, Fish value, GH_Path path)
        {
            if (value.Objectives == null)
            {
                return;
            }

            foreach (KeyValuePair<string, double> objective in value.Objectives)
            {
                objectives.Append(new GH_Number(objective.Value), path);
            }
        }

        private static void SetAttributes(GH_Structure<GH_FishAttribute> attributes, Fish value, GH_Path path)
        {
            var attr = new GH_FishAttribute(new Dictionary<string, object>());
            foreach (KeyValuePair<string, object> attribute in value.Attributes)
            {
                attr.Value.Add(attribute.Key, attribute.Value);
            }
            attributes.Append(attr, path);
        }

        protected override System.Drawing.Bitmap Icon => Resource.DeconstructFish;
        public override Guid ComponentGuid => new Guid("1D0EA5A9-9499-4D70-8505-6AB4B05889F4");
    }
}
