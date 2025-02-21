using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;

using Tunny.Component.Params;
using Tunny.Resources;

namespace Tunny.Component.Operation
{
    public class ConstructFishAttribute : GH_Component, IGH_VariableParameterComponent
    {
        private const string ConstraintDescription
            = "A value strictly larger than 0 means that a constraints is violated. A value equal to or smaller than 0 is considered feasible. ";
        private const string AttrDescription
            = "Attributes to each trial. Attribute name will be the nickname of the input, so change it to any value.";
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        public ConstructFishAttribute()
          : base("Construct Fish Attribute", "ConstrFA",
            "Construct Fish Attribute.",
            "Tunny", "Operation")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Constraint", "Constraint", ConstraintDescription, GH_ParamAccess.list);
            pManager.AddGenericParameter("Attr1", "Attr1", AttrDescription, GH_ParamAccess.list);
            pManager.AddGenericParameter("Attr2", "Attr2", AttrDescription, GH_ParamAccess.list);
            Params.Input[0].Optional = true;
            Params.Input[1].Optional = true;
            Params.Input[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new Param_FishAttribute(), "Attributes", "Attrs", "Attributes to each Trial", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int paramCount = Params.Input.Count;
            var dict = new Dictionary<string, object>();

            if (CheckIsNicknameDuplicated())
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Attribute nickname must be unique.");
                return;
            }

            GetInputData(DA, paramCount, dict);
            DA.SetData(0, dict);
        }

        private void GetInputData(IGH_DataAccess DA, int paramCount, Dictionary<string, object> dict)
        {
            for (int i = 0; i < paramCount; i++)
            {
                string key = Params.Input[i].NickName;
                switch (i)
                {
                    case 0:
                        SetConstraintDataList(DA, dict, i, key);
                        break;
                    default:
                        SetGenericDataList(DA, dict, i, key);
                        break;
                }
            }
        }

        private static void SetGenericDataList(IGH_DataAccess DA, Dictionary<string, object> dict, int i, string key)
        {
            var list = new List<object>();
            if (!DA.GetDataList(i, list))
            {
                return;
            }
            dict.Add(key, list);
        }

        private static void SetConstraintDataList(IGH_DataAccess DA, Dictionary<string, object> dict, int i, string key)
        {
            var constraint = new List<double>();
            if (DA.GetDataList(i, constraint))
            {
                dict.Add(key, constraint);
            }
        }

        //FIXME: Should be modified to capture and check for change events.
        private bool CheckIsNicknameDuplicated()
        {
            var nicknames = Params.Input.Select(x => x.NickName).ToList();
            var hashSet = new HashSet<string>();

            foreach (string nickname in nicknames)
            {
                if (hashSet.Add(nickname) == false)
                {
                    return true;
                }
            }
            return false;
        }

        public bool CanInsertParameter(GH_ParameterSide side, int index) => side != GH_ParameterSide.Output && (Params.Input.Count == 0 || index >= 1);

        public bool CanRemoveParameter(GH_ParameterSide side, int index) => side != GH_ParameterSide.Output && index >= 1;

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            if (side == GH_ParameterSide.Input)
            {
                return SetGenericParameterInput();
            }
            return null;
        }

        private Param_GenericObject SetGenericParameterInput()
        {
            int count = 1;
            IEnumerable<string> attrInput = Params.Input.Select(x => x.NickName).Where(x => x.Contains("Attr"));
            string nickname = $"Attr{count}";
            while (attrInput.Contains(nickname))
            {
                count++;
                nickname = $"Attr{count}";
            }
            var p = new Param_GenericObject();
            p.Name = p.NickName = nickname;
            p.Description = AttrDescription;
            p.Access = GH_ParamAccess.list;
            p.Optional = true;
            return p;
        }

        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            return true;
        }

        public void VariableParameterMaintenance()
        {
            foreach (IGH_Param param in Params)
            {
                param.ObjectChanged -= ParamChangedHandler;
                param.ObjectChanged += ParamChangedHandler;
            }
        }

        private void ParamChangedHandler(IGH_DocumentObject sender, GH_ObjectChangedEventArgs e)
        {
            if (e.Type == GH_ObjectEventType.NickName)
            {
                if (sender.NickName.Length == 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Attribute nickname length must be longer than 0.");
                }
                else
                {
                    ExpireSolution(true);
                }
            }
        }

        protected override System.Drawing.Bitmap Icon => Resource.ConstructFishAttribute;
        public override Guid ComponentGuid => new Guid("9406666d-db8a-4955-8a6f-4200031e84aa");

        public override void CreateAttributes()
        {
            m_attributes = new ConstructFishAttributeAttributes(this);
        }
    }
}
