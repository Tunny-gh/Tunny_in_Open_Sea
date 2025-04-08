using Eto.Forms;

using Tunny.Core.Util;

namespace Tunny.Eto.Message
{
    internal static partial class TunnyMessageBox
    {
        internal static void Info_OutputFinish()
        {
            Show(
                "Output result to fish completed successfully.",
                "Tunny",
                MessageBoxButtons.OK,
                MessageBoxType.Information
            );
        }

        internal static DialogResult Info_PythonAlreadyInstalled()
        {
            return Show(
                "It appears that the Tunny Python environment is already installed.\nWould you like to reinstall it?",
                "Python is already installed",
                MessageBoxButtons.OKCancel,
                MessageBoxType.Information
            );
        }

        internal static void Info_About()
        {
            Show(
                "Tunny\nVersion: " + TEnvVariables.Version + "\n\n🐟Tunny🐟 is Grasshopper's optimization component using Optuna, an open source hyperparameter auto-optimization framework.\n\nTunny is developed by hrntsm.\nFor more information, visit https://tunny-docs.deno.dev/",
                "About Tunny",
                MessageBoxButtons.OK,
                MessageBoxType.Information
            );
        }

        internal static void Info_PythonInstallStart()
        {
            Show(
                "Tunny is installing the Python environment.\n\nThe progress bar at the bottom of TunnyUI is progress.\nThis may take a few minutes.",
                "Installing Python",
                MessageBoxButtons.OK,
                MessageBoxType.Information
            );
        }

        internal static void Info_OptunaDashboardAlreadyInstalled()
        {
            Show("optuna-dashboard is not installed.\nFirst install optuna-dashboard from the Tunny component.",
                "optuna-dashboard is not installed",
                MessageBoxButtons.OK,
                MessageBoxType.Error
            );
        }

        internal static void Info_ResultFileHasNoStudy()
        {
            Show(
                "There is no study to visualize.\nPlease set 'Target Study'",
                "Tunny",
                MessageBoxButtons.OK,
                MessageBoxType.Information
            );
        }
    }
}
