using System.Collections.ObjectModel;

using Optuna.Study;

using Tunny.Eto.Models;

namespace Tunny.Eto.Common
{
    internal static class Utils
    {
        internal static ObservableCollection<NameComboBoxItem> StudyNamesFromStudySummaries(StudySummary[] summaries)
        {
            var items = new ObservableCollection<NameComboBoxItem>();
            if (summaries == null)
            {
                return items;
            }
            for (int i = 0; i < summaries.Length; i++)
            {
                StudySummary summary = summaries[i];
                items.Add(new NameComboBoxItem()
                {
                    Id = summary.StudyId,
                    Name = summary.StudyName
                });
            }
            return items;
        }
    }
}
