using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using Rhino;
using Rhino.Display;
using Rhino.Geometry;

using Tunny.Component.Params;
using Tunny.Core.Util;
using Tunny.Resources;
using Tunny.Type;
using Tunny.Util;

namespace Tunny.Component.Visualize
{
    [LoggingAspect]
    public class FishMarket : GH_Component
    {
        private readonly List<Plane> _tagPlanes = new List<Plane>();
        private List<GH_Fish> _fishes = new List<GH_Fish>();
        private double _textHeight = 1;
        private Settings _settings = new Settings();

        public override GH_Exposure Exposure => GH_Exposure.quarternary;

        public FishMarket()
          : base("Fish Market", "FMarket",
            "A place to lay out the solutions we caught.",
            "Tunny", "Visualize")
        {
        }

        public override void ClearData()
        {
            base.ClearData();
            _tagPlanes.Clear();
            _fishes.Clear();
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_Fish(), "Fishes", "Fishes", "Fishes caught by the optimization nets.", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Plane", "Plane", "Plane", GH_ParamAccess.item, Plane.WorldXY);
            pManager.AddIntegerParameter("xNum", "xNum", "Number of x.", GH_ParamAccess.item, 5);
            pManager.AddNumberParameter("xInterval", "xIntvl", "Interval of x.", GH_ParamAccess.item, 100);
            pManager.AddNumberParameter("yInterval", "yIntvl", "Interval of y.", GH_ParamAccess.item, 100);
            pManager.AddNumberParameter("TextHeight", "txtH", "Font hight.", GH_ParamAccess.item, 1);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometries", "Geom", "Geometries of the fish market.", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var fishObjects = new List<object>();
            var plane = new Plane();
            int xNum = 0;
            double xInterval = 0;
            double yInterval = 0;
            if (!DA.GetDataList(0, fishObjects)) { return; }
            if (!DA.GetData(1, ref plane)) { return; }
            if (!DA.GetData(2, ref xNum)) { return; }
            if (!DA.GetData(3, ref xInterval)) { return; }
            if (!DA.GetData(4, ref yInterval)) { return; }
            if (!DA.GetData(5, ref _textHeight)) { return; }

            _settings = new Settings(plane, xNum, xInterval, yInterval);
            if (xNum <= 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "xNum must lager than 0");
                return;
            }
            DA.SetDataTree(0, ArrayedGeometries(fishObjects));
            ExpirePreview(true);
        }

        private GH_Structure<IGH_GeometricGoo> ArrayedGeometries(IEnumerable<object> fishObjects)
        {
            int countY = 0;
            var arrayedGeometries = new GH_Structure<IGH_GeometricGoo>();
            _fishes = fishObjects.Select(x => (GH_Fish)x).ToList();

            while (true)
            {
                var fishGeometries = _fishes.Select(x => x.Value.GetGeometries()).ToList();
                bool remainGeometry = SetGeometryToResultArray(countY, arrayedGeometries, fishGeometries);
                if (!remainGeometry)
                {
                    break;
                }
                countY++;
            }

            return arrayedGeometries;
        }

        private bool SetGeometryToResultArray(int countY, GH_Structure<IGH_GeometricGoo> arrayedGeometries, List<GeometryBase[]> fishGeometries)
        {
            Vector3d yVec = _settings.Plane.YAxis * (_settings.YInterval * countY);
            for (int countX = 0; countX < _settings.XNum; countX++)
            {
                int index = countX + (_settings.XNum * countY);
                if (index == _fishes.Count)
                {
                    return false;
                }
                Vector3d moveVec = (_settings.Plane.XAxis * (_settings.XInterval * countX)) + yVec;
                if (fishGeometries[index] != null)
                {
                    MoveGeometries(index, moveVec, fishGeometries, arrayedGeometries);
                }
            }
            return true;
        }

        //FIXME: Possibly heavy because deep copying here and doing it for all geometry every time.
        private void MoveGeometries(int index, Vector3d moveVec, List<GeometryBase[]> fishGeometries, GH_Structure<IGH_GeometricGoo> arrayedGeometries)
        {
            var movedGeometries = new GeometryBase[fishGeometries[index].Length];
            Point3d modelMinPt = GetUnionBoundingBoxMinPt(fishGeometries[index]);
            foreach ((GeometryBase geometry, int i) in fishGeometries[index].Select((g, i) => (g, i)))
            {
                GeometryBase dupGeometry = geometry.Duplicate();
                dupGeometry.Rotate(Vector3d.VectorAngle(Vector3d.XAxis, _settings.Plane.XAxis), Vector3d.ZAxis, modelMinPt);
                dupGeometry.Translate(moveVec + new Vector3d(_settings.Plane.Origin) - new Vector3d(modelMinPt));
                arrayedGeometries.Append(GooConverter.GeometryBaseToGoo(dupGeometry), new GH_Path(0, _fishes[index].Value.TrialNumber));
                movedGeometries[i] = dupGeometry;
            }
            modelMinPt = GetUnionBoundingBoxMinPt(movedGeometries);
            _tagPlanes.Add(new Plane(
                modelMinPt - (_settings.Plane.YAxis * 2.5 * _textHeight),
                _settings.Plane.XAxis,
                _settings.Plane.YAxis
                )
            );
        }

        private static Point3d GetUnionBoundingBoxMinPt(IEnumerable<GeometryBase> geometryBases)
        {
            var minPt = new Point3d(double.MaxValue, double.MaxValue, double.MaxValue);
            foreach (BoundingBox boundingBox in geometryBases.Select(geometry => geometry.GetBoundingBox(Plane.WorldXY)))
            {
                if (boundingBox.Min.X < minPt.X)
                {
                    minPt.X = boundingBox.Min.X;
                }
                if (boundingBox.Min.Y < minPt.Y)
                {
                    minPt.Y = boundingBox.Min.Y;
                }
                if (boundingBox.Min.Z < minPt.Z)
                {
                    minPt.Z = boundingBox.Min.Z;
                }
            }
            return minPt;
        }

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            base.DrawViewportWires(args);
            for (int i = 0; i < _fishes.Count; i++)
            {
                var text3d = new Text3d(_fishes[i].ToString(), _tagPlanes[i], _textHeight);
                args.Display.Draw3dText(text3d, Color.Black);
            }
        }

        public override void BakeGeometry(RhinoDoc doc, List<Guid> obj_ids)
        {
            base.BakeGeometry(doc, obj_ids);

            for (int i = 0; i < _fishes.Count; i++)
            {
                doc.Objects.AddText(_fishes[i].ToString(), _tagPlanes[i], _textHeight, "Meiryo", false, false, TextJustification.TopLeft);
                BoundingBox a = doc.Objects.BoundingBox;
            }
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            ToolStripMenuItem item = Menu_AppendItem(menu, "Bake with Text", BakeGeometryFromMenu);
            item.ToolTipText = @"Bake not only geometry, but also result texts.";
        }

        private void BakeGeometryFromMenu(object sender, EventArgs e)
        {
            RhinoDoc doc = RhinoDoc.ActiveDoc;
            var objIds = new List<Guid>();
            BakeGeometry(doc, objIds);
            doc.Views.Redraw();
        }

        protected override Bitmap Icon => Resource.FishMarket;
        public override Guid ComponentGuid => new Guid("46E05DFF-8EFD-418A-AC5B-7C9F24559B2E");

        private sealed class Settings
        {
            public Plane Plane { get; set; }
            public int XNum { get; set; }
            public double XInterval { get; set; }
            public double YInterval { get; set; }

            public Settings()
            {
            }

            public Settings(Plane plane, int xNum, double xInterval, double yInterval)
            {
                Plane = plane;
                XNum = xNum;
                XInterval = xInterval;
                YInterval = yInterval;
            }
        }
    }
}
