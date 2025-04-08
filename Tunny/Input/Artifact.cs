using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using Optuna.Trial;

using Python.Runtime;

using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;

using Tunny.Core.Util;

namespace Tunny.Input
{
    [LoggingAspect]
    public class Artifact
    {
        public List<GeometryBase> Geometries { get; set; } = new List<GeometryBase>();
        public List<Bitmap> Images { get; set; } = new List<Bitmap>();
        public List<string> ArtifactPaths { get; private set; } = new List<string>();

        public void AddFilePathToArtifact(string path)
        {
            if (File.Exists(path))
            {
                ArtifactPaths.Add(path);
            }
        }

        public int Count()
        {
            return Geometries.Count + Images.Count + ArtifactPaths.Count;
        }

        public void UploadArtifacts(dynamic artifactBackend, TrialWrapper trial)
        {
            string fileName = $"artifact_trial_{trial.Number}";
            string basePath = Path.Combine(TEnvVariables.TmpDirPath, fileName);
            SaveAllArtifacts(basePath);

            dynamic optuna = Py.Import("optuna");
            foreach (string path in ArtifactPaths)
            {
                optuna.artifacts.upload_artifact(trial.PyInstance, path, artifactBackend);
            }
        }

        private void SaveAllArtifacts(string basePath)
        {
            if (Geometries.Count > 0)
            {
                SaveRhino3dm(basePath);
            }
            if (Images.Count > 0)
            {
                SaveImage(basePath);
            }
        }

        private void SaveRhino3dm(string basePath)
        {
            string path = basePath + "_model.3dm";
            var rhinoDoc = RhinoDoc.CreateHeadless("");
            foreach (GeometryBase geom in Geometries)
            {
                rhinoDoc.Objects.Add(geom);
            }

            foreach (RhinoObject obj in rhinoDoc.Objects)
            {
                obj.CreateMeshes(MeshType.Render, new MeshingParameters(), false);
            }

            var option = new Rhino.FileIO.FileWriteOptions
            {
                FileVersion = 7,
                IncludeRenderMeshes = true
            };

            rhinoDoc.Write3dmFile(path, option);
            rhinoDoc.Dispose();

            ArtifactPaths.Add(path);
        }

        private void SaveImage(string basePath)
        {
            for (int i = 0; i < Images.Count; i++)
            {
                Bitmap bitmap = Images[i];
                string path = basePath + $"_image_{i}.png";
                bitmap?.Save(path, ImageFormat.Png);
                ArtifactPaths.Add(path);
            }
        }
    }
}
