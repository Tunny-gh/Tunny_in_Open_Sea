using System;

using Grasshopper.Kernel.Types;

using Rhino.FileIO;
using Rhino.Geometry;

using Tunny.Core.Util;

namespace Tunny.Util
{
    public static class GooConverter
    {
        public static IGH_GeometricGoo GeometryBaseToGoo(GeometryBase geometryBase)
        {
            TLog.MethodStart();
            switch (geometryBase)
            {
                case Mesh mesh:
                    return new GH_Mesh(mesh);
                case Curve curve:
                    return new GH_Curve(curve);
                case Brep brep:
                    return new GH_Brep(brep);
                case Surface surface:
                    return new GH_Surface(surface);
                case SubD subD:
                    return new GH_SubD(subD);
                case Point point:
                    return new GH_Point(point.Location);
                default:
                    throw new ArgumentException("Only supports mesh, curve, brep, surface, subD, point.");
            }
        }

        public static string GooToString(IGH_Goo goo, bool isGeometryBaseToJson)
        {
            TLog.MethodStart();
            var option = new SerializationOptions();
            return goo.GooToString(isGeometryBaseToJson, option);
        }

        private static string GooToString(this IGH_Goo goo, bool isGeometryBaseToJson, SerializationOptions option)
        {
            TLog.MethodStart();
            if (isGeometryBaseToJson)
            {
                switch (goo)
                {
                    case GH_Mesh mesh:
                        return mesh.Value.ToJSON(option);
                    case GH_Brep brep:
                        return brep.IsValid ? brep.Value.ToJSON(option) : string.Empty;
                    case GH_Curve curve:
                        return curve.Value.ToJSON(option);
                    case GH_Surface surface:
                        return surface.Value.ToJSON(option);
                    case GH_SubD subd:
                        return subd.Value.ToJSON(option);
                    default:
                        return goo.ToString();
                }
            }
            else
            {
                return goo.ToString();
            }
        }
    }
}
