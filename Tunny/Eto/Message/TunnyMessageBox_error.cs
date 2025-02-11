
using Eto.Forms;

using Tunny.Core.Util;

namespace Tunny.Eto.Message
{
    internal static partial class TunnyMessageBox
    {
        internal static void Error_NoArtifactFound()
        {
            TLog.MethodStart();
            Show(
                "Failed to load Artifact. Please check the value.",
                "Tunny",
                MessageBoxButtons.OK,
                MessageBoxType.Error);
        }

        internal static void Error_IncorrectVariableInput()
        {
            TLog.MethodStart();
            Show(
                "Input variables must be either a number slider or a gene pool.\nError input will automatically remove.",
                "Tunny",
                MessageBoxButtons.OK,
                MessageBoxType.Error);
        }

        internal static void Error_NoSourceStudySelected()
        {
            TLog.MethodStart();
            Show(
                "Please set CopySourceStudy.",
                "Tunny",
                MessageBoxButtons.OK,
                MessageBoxType.Error);
        }

        internal static void Error_CopySourceAndDestinationAreSame()
        {
            TLog.MethodStart();
            Show(
                "Copy source and destination StudyName are the same.",
                "Tunny",
                MessageBoxButtons.OK,
                MessageBoxType.Error);
        }

        internal static void Error_PrunerPath()
        {
            TLog.MethodStart();
            Show(
                "PrunerPath has something wrong. Please check.",
                "Tunny",
                MessageBoxButtons.OK,
                MessageBoxType.Error);
        }

        internal static void Error_IncorrectObjectiveInput()
        {
            TLog.MethodStart();
            Show(
                "Objective must be either a number or a FishPrint.\nError input will automatically remove.",
                "Tunny",
                MessageBoxButtons.OK,
                MessageBoxType.Error);
        }

        internal static void Error_IncorrectAttributeInput()
        {
            TLog.MethodStart();
            Show(
                "Inputs to Attribute should be grouped together into one FishAttribute.",
                "Tunny",
                MessageBoxButtons.OK,
                MessageBoxType.Error);
        }

        internal static void Error_ObjectiveNicknamesMustUnique()
        {
            TLog.MethodStart();
            Show(
                "Objective nicknames must be unique.",
                "Tunny",
                MessageBoxButtons.OK,
                MessageBoxType.Error);
        }

        internal static void Error_NoVariableInput()
        {
            TLog.MethodStart();
            Show(
                "No input variables found. \nPlease connect a number slider to the input of the component.",
                "Tunny",
                MessageBoxButtons.OK,
                MessageBoxType.Error);
        }

        internal static bool Error_ShowNoObjectiveFound()
        {
            TLog.MethodStart();
            Show(
                "No objective found.\nPlease connect number or FishPrint to the objective.",
                "Tunny",
                MessageBoxButtons.OK,
                MessageBoxType.Error);
            return false;
        }

        internal static void Error_DirectionCountNotMatch()
        {
            TLog.MethodStart();
            Show(
                "The number of the direction in FishAttr must be the same as the number of the objective.",
                "Tunny",
                MessageBoxButtons.OK,
                MessageBoxType.Error);
        }

        internal static void Error_DirectionValue()
        {
            TLog.MethodStart();
            Show(
                "Direction must be either 1(maximize) or -1(minimize).",
                "Tunny",
                MessageBoxButtons.OK,
                MessageBoxType.Error
            );
        }

        internal static void Error_ComponentLoadFail()
        {
            TLog.MethodStart();
            Show(
                "Fail to load Grasshopper data into Tunny",
                "Tunny",
                MessageBoxButtons.OK,
                MessageBoxType.Error
            );
        }

        internal static void Error_NoStudyNameSelected()
        {
            TLog.MethodStart();
            Show(
                "Please set StudyName.",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxType.Error
            );
        }

        internal static void Error_ResultFileNotExist()
        {
            TLog.MethodStart();
            Show(
                "Please set exist result file path.",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxType.Error
            );
        }

        internal static void Error_VisualizationTypeNotSupported()
        {
            TLog.MethodStart();
            Show(
                "This visualization type is not supported in this study case.",
                "Tunny",
                MessageBoxButtons.OK,
                MessageBoxType.Error
            );
        }

        internal static void Error_PlotParameterSet()
        {
            TLog.MethodStart();
            Show(
                "Your plot parameter is not set correctly.",
                "Tunny",
                MessageBoxButtons.OK,
                MessageBoxType.Error
            );
        }

        internal static void Error_NoStudyFound()
        {
            Show(
                "No study found.",
                "Tunny",
                MessageBoxButtons.OK,
                MessageBoxType.Error
            );
        }

        internal static DialogResult Error_PythonDllNotFound()
        {
            return Show(
                "Tunny Python Runtime is not found. \nDo you want to install the Tunny Python environment?",
                "Tunny",
                MessageBoxButtons.YesNo,
                MessageBoxType.Error
            );
        }
    }
}
