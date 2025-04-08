using Prism.Mvvm;

using Tunny.Core.Util;

namespace Tunny.WPF.Models
{
    [LoggingAspect]
    internal sealed class OutputTrialItem : BindableBase
    {
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        private int _id;
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _objectives;
        public string Objectives
        {
            get => _objectives;
            set => SetProperty(ref _objectives, value);
        }

        private string _variables;
        public string Variables
        {
            get => _variables;
            set => SetProperty(ref _variables, value);
        }
    }
}
