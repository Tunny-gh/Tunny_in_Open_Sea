using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using Optuna.Study;

using Prism.Commands;
using Prism.Mvvm;

using Tunny.CommonUI.Message;
using Tunny.Core.Handler;
using Tunny.Core.Settings;
using Tunny.Core.Storage;
using Tunny.Core.Util;
using Tunny.WPF.Common;
using Tunny.WPF.Models;

namespace Tunny.WPF.ViewModels
{
    internal sealed class TargetStudyNameSelectorViewModel : BindableBase
    {
        private readonly TSettings _settings;

        private DelegateCommand _oKCommand;
        public ICommand OKCommand
        {
            get
            {
                if (_oKCommand == null)
                {
                    _oKCommand = new DelegateCommand(OK);
                }

                return _oKCommand;
            }
        }

        private void OK()
        {
            TLog.MethodStart();
            if (SelectedStudyName == null || string.IsNullOrEmpty(SelectedStudyName.Name))
            {
                TunnyMessageBox.Error_NoStudyNameSelected();
                return;
            }

            var designExplorer = new DesignExplorer(SelectedStudyName.Name, _settings.Storage);
            designExplorer.Run();
            Close();
        }

        private DelegateCommand _cancelCommand;
        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new DelegateCommand(Cancel);
                }

                return _cancelCommand;
            }
        }

        private void Cancel()
        {
            TLog.MethodStart();
            Close();
        }

        private void Close()
        {
            TLog.MethodStart();
            Window window = Application.Current.Windows.OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this);
            window?.Close();
        }

        private ObservableCollection<NameComboBoxItem> _studyNameItems;
        public ObservableCollection<NameComboBoxItem> StudyNameItems { get => _studyNameItems; set => SetProperty(ref _studyNameItems, value); }
        private NameComboBoxItem _selectedStudyName;
        public NameComboBoxItem SelectedStudyName { get => _selectedStudyName; set => SetProperty(ref _selectedStudyName, value); }

        public TargetStudyNameSelectorViewModel()
        {
            _settings = SharedItems.Instance.Settings;
            StudyNameItems = StudyNamesFromStorage(_settings.Storage.Path);
            if (StudyNameItems.Count == 0)
            {
                TunnyMessageBox.Error_NoStudyFound();
                Close();
                return;
            }
            SelectedStudyName = StudyNameItems[0];
        }

        private static ObservableCollection<NameComboBoxItem> StudyNamesFromStorage(string storagePath)
        {
            StudySummary[] summaries = new StorageHandler().GetStudySummaries(storagePath);
            return Utils.StudyNamesFromStudySummaries(summaries);
        }
    }
}
