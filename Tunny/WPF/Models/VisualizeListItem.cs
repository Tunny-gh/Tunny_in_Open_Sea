using Prism.Mvvm;

using Tunny.Core.Util;

namespace Tunny.WPF.Models
{
    [LoggingAspect]
    internal sealed class VisualizeListItem : BindableBase
    {
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
    }
}
