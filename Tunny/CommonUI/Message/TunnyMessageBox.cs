using Eto.Forms;

using Tunny.Core.Util;

namespace Tunny.CommonUI.Message
{
    internal static partial class TunnyMessageBox
    {
        internal static DialogResult Show(string messageBoxText, string caption, MessageBoxButtons button = MessageBoxButtons.OK, MessageBoxType icon = MessageBoxType.Information)
        {
            DialogResult msgResult = DialogResult.None;

            Application.Instance.Invoke(() =>
            {
                WriteLog(messageBoxText, icon);
                msgResult = MessageBox.Show(Application.Instance.MainForm, messageBoxText, caption, button, icon);
                if (msgResult != DialogResult.None && msgResult != DialogResult.Ok)
                {
                    TLog.Info($"Dialog result: {msgResult}");
                }
            });

            return msgResult;
        }

        private static void WriteLog(string message, MessageBoxType icon)
        {
            string noLineBreakMessage = message.Replace("\n", " ");
            switch (icon)
            {
                case MessageBoxType.Error:
                    TLog.Error(noLineBreakMessage);
                    break;
                case MessageBoxType.Warning:
                    TLog.Warning(noLineBreakMessage);
                    break;
                case MessageBoxType.Information:
                    TLog.Info(noLineBreakMessage);
                    break;
                default:
                    TLog.Debug(noLineBreakMessage);
                    break;
            }
        }
    }
}
