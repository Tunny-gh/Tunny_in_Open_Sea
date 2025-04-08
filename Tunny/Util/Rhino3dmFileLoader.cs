using System.Collections.Generic;

using Rhino.FileIO;
using Rhino.Geometry;

using Tunny.Core.Util;

namespace Tunny.Util
{
    [LoggingAspect]
    public class Rhino3dmFileLoader
    {
        public static List<GeometryBase> Load(string path)
        {
            var geometries = new List<GeometryBase>();
            var rhinoFile = File3dm.Read(path);
            foreach (File3dmObject rhObj in rhinoFile.Objects)
            {
                GeometryBase geometry = rhObj.Geometry;
                if (geometry != null)
                {
                    geometries.Add(geometry);
                }
            }

            return geometries;
        }
    }
}
