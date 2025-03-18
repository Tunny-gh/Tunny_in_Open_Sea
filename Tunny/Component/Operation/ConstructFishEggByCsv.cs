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
    public class ConstructFishEggByCsv : GH_Component
    {
        private readonly List<FishEgg> _fishEggs = new List<FishEgg>();
        private bool _skipIfExist;
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        public ConstructFishEggByCsv()
          : base("Construct Fish Egg by CSV", "ConstrFEggCsv",
            "You can specify the initial individual by CSV file that Tunny will run. Try your golden egg!",
            "Tunny", "Operation")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Variables", "Vars", "Variables pair to enqueue optimize.", GH_ParamAccess.list);
            pManager.AddTextParameter("CSV File Path", "Path", "CSV file path to enqueue optimize.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Skip If Exist", "Skip", "If true, skip if the same egg already exists", GH_ParamAccess.item, true);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new Param_FishEgg(), "FishEgg", "FishEgg", "These eggs are enqueued for optimization and become fish.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string csvPath = string.Empty;
            if (!DA.GetData(1, ref csvPath)) { return; }
            if (!DA.GetData(2, ref _skipIfExist)) { return; }
            try
            {
                LayFishEgg(csvPath);
            }
            catch (Exception e)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message);
                return;
            }

            DA.SetDataList(0, _fishEggs);
        }

        private void LayFishEgg(string csvPath)
        {
            _fishEggs.Clear();
            var ghIO = new GrasshopperInOut(this, getVariableOnly: true);
            List<VariableBase> variables = ghIO.Variables;
            Dictionary<string, double[]> variableRange = GetVariableRange(variables);

            var reader = new CsvReader(csvPath);
            List<Dictionary<string, string>> csvData = reader.ReadFishEggCsv();

            AddVariablesToFishEgg(variableRange, csvData);
        }

        private static Dictionary<string, double[]> GetVariableRange(List<VariableBase> variables)
        {
            var variableRange = new Dictionary<string, double[]>();
            foreach (VariableBase variable in variables)
            {
                string name = variable.NickName;
                if (variable is NumberVariable numberVariable)
                {
                    variableRange.Add(name, new double[] { numberVariable.LowerBond, numberVariable.UpperBond });
                }
                else if (variable is CategoricalVariable)
                {
                    variableRange.Add(name, Array.Empty<double>());
                }
                else
                {
                    throw new InvalidOperationException("Variable type is not supported.");
                }
            }
            return variableRange;
        }

        private void AddVariablesToFishEgg(Dictionary<string, double[]> variableRange, IEnumerable<Dictionary<string, string>> csvData)
        {
            foreach (Dictionary<string, string> data in csvData)
            {
                var egg = new FishEgg(_skipIfExist);
                foreach (KeyValuePair<string, string> item in data)
                {
                    if (!variableRange.TryGetValue(item.Key, out double[] range))
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, $"There is no slider named \"{item.Key}\" listed in the csv.");
                        continue;
                    }

                    if (range.Length == 0)
                    {
                        egg.AddParam(item.Key, item.Value);
                    }
                    else
                    {
                        double value = double.Parse(item.Value, CultureInfo.InvariantCulture);
                        if (value < range[0] || value > range[1])
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Ignore the value of {item.Key}: {value} since it is outside the range of the slider.");
                            continue;
                        }
                        egg.AddParam(item.Key, item.Value);
                    }
                }
                if (egg.RegisteredParamsCount != 0)
                {
                    _fishEggs.Add(egg);
                }
            }
        }

        public override void CreateAttributes()
        {
            m_attributes = new ConstructFishEggByCsvAttributes(this);
        }

        protected override System.Drawing.Bitmap Icon => Resources.Resource.ConstructFishEggByCsv;
        public override Guid ComponentGuid => new Guid("d94cadbf-88ab-4751-8e48-588b7351dd36");
    }
}
