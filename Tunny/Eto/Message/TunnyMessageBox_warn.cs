using Eto.Forms;

using Tunny.Core.Util;

namespace Tunny.Eto.Message
{
    internal static partial class TunnyMessageBox
    {
        internal static void Warn_SettingsJsonFileLoadFail()
        {
            Show(
                "Failed to load settings file. Start with default settings.",
                "Tunny",
                MessageBoxButtons.OK,
                MessageBoxType.Warning
            );
        }

        internal static void Warn_VariableMustLargerThanZeroInLogScale()
        {
            Show(
                "Variable value range must be larger than 0 if LogScale is True.",
                "Tunny",
                MessageBoxButtons.OK,
                MessageBoxType.Warning
            );
        }

        internal static void Warn_PreferentialGpSupportRange()
        {
            Show(
                "Human-in-the-Loop(Preferential GP optimization) only supports single objective optimization. Optimization is run without considering constraints.",
                "Tunny",
                MessageBoxButtons.OK,
                MessageBoxType.Warning
            );
        }

        internal static DialogResult Warn_ResultFileNameNotMatch()
        {
            return Show(
                "The selected csv file name does not match the target StudyName.\n" +
                "Rhino may crash if results from a different Study are loaded.\n\n" +
                "Do you want to continue loading?",
                "Tunny",
                MessageBoxButtons.YesNo,
                MessageBoxType.Warning
            );
        }
    }
}
