using Eto.Forms;

using Tunny.Core.Util;

namespace Tunny.Eto.Message
{
    internal static partial class TunnyMessageBox
    {
        internal static void Warn_SettingsJsonFileLoadFail()
        {
            TLog.MethodStart();
            Show(
                "Failed to load settings file. Start with default settings.",
                "Tunny",
                MessageBoxButtons.OK,
                MessageBoxType.Warning
            );
        }

        internal static void Warn_VariableMustLargerThanZeroInLogScale()
        {
            TLog.MethodStart();
            Show(
                "Variable value range must be larger than 0 if LogScale is True.",
                "Tunny",
                MessageBoxButtons.OK,
                MessageBoxType.Warning
            );
        }
    }
}
