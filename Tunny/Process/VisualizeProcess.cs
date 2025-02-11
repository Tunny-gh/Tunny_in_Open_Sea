using System;
using System.IO;

using Optuna.Study;
using Optuna.Visualization;

using Python.Runtime;

using Tunny.Core.Settings;
using Tunny.Core.Util;
using Tunny.Eto.Message;

namespace Tunny.Process
{
    internal sealed class VisualizeProcess : PythonInit
    {
        internal static void Save(Storage storage, PlotSettings plotSettings, string htmlPath)
        {
            Plot(storage, plotSettings, htmlPath);
        }

        internal static string Plot(Storage storage, PlotSettings plotSettings, string htmlPath = "")
        {
            TLog.MethodStart();
            InitializePythonEngine();
            using (Py.GIL())
            {
                dynamic optunaStorage = storage.CreateNewOptunaStorage(false);
                StudyWrapper study = StudyWrapper.LoadStudy(optunaStorage, plotSettings.TargetStudyName);
                if (study == null || study.PyInstance == null)
                {
                    return string.Empty;
                }

                try
                {
                    PlotlyFigure figure = CreateFigure(study, plotSettings);
                    if (string.IsNullOrEmpty(htmlPath))
                    {
                        htmlPath = Path.Combine(TEnvVariables.TmpDirPath, "plot.html");
                        figure.UpdateLayout(new FigureLayout { PaperBgColor = "rgba(0,0,0,0)" });
                    }
                    figure.WriteHtml(htmlPath);
                }
                catch (Exception)
                {
                    htmlPath = string.Empty;
                    TunnyMessageBox.Error_VisualizationTypeNotSupported();
                }
            }
            PythonEngine.Shutdown();
            return htmlPath;
        }

        private static PlotlyFigure CreateFigure(StudyWrapper study, PlotSettings settings)
        {
            TLog.MethodStart();
            switch (settings.PlotTypeName)
            {
                case "contour":
                    return Visualization.PlotContour(study, settings.TargetObjectiveName[0], settings.TargetObjectiveIndex[0], settings.TargetVariableName);
                case "EDF":
                    return Visualization.PlotEDF(study, settings.TargetObjectiveName[0], settings.TargetObjectiveIndex[0]);
                case "optimization history":
                    return Visualization.PlotOptimizationHistory(study, settings.TargetObjectiveName[0], settings.TargetObjectiveIndex[0]);
                case "parallel coordinate":
                    return Visualization.PlotParallelCoordinate(study, settings.TargetObjectiveName[0], settings.TargetObjectiveIndex[0], settings.TargetVariableName);
                case "param importances":
                    return Visualization.PlotParamImportances(study, settings.TargetObjectiveName[0], settings.TargetObjectiveIndex[0], settings.TargetVariableName, settings.ImportanceEvaluator);
                case "pareto front":
                    PlotlyFigure fig = Visualization.PlotParetoFront(study, settings.TargetObjectiveName, settings.TargetObjectiveIndex, settings.IncludeDominatedTrials);
                    return Visualization.TruncateParetoFrontPlotHover(study, fig);
                case "slice":
                    return Visualization.PlotSlice(study, settings.TargetObjectiveName[0], settings.TargetObjectiveIndex[0], settings.TargetVariableName);
                case "hypervolume":
                    return Visualization.PlotHypervolume(study, settings.ReferencePoint);
                case "rank":
                    return Visualization.PlotRank(study, settings.TargetObjectiveName[0], settings.TargetObjectiveIndex[0], settings.TargetVariableName);
                case "timeline":
                    return Visualization.PlotTimeline(study);
                //case "TerminatorImprovement":
                //    return Visualization.PlotTerminatorImprovement(study);
                //case "clustering":
                //    return Visualization.PlotClustering(study, settings.ClusterCount, settings.TargetObjectiveIndex, settings.TargetVariableIndex);
                default:
                    throw new ArgumentException("Invalid plot type");
            }
        }
    }
}
