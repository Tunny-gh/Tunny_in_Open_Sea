using Prism.Mvvm;

namespace Tunny.Eto.Models
{
    internal sealed class ObjectiveSettingItem : BindableBase
    {
        private bool _minimize;
        public bool Minimize
        {
            get => _minimize;
            set
            {
                if (SetProperty(ref _minimize, value) && value)
                {
                    Maximize = false;
                }
            }
        }

        private bool _maximize;
        public bool Maximize
        {
            get => _maximize;
            set
            {
                if (SetProperty(ref _maximize, value) && value)
                {
                    Minimize = false;
                }
            }
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
    }
}
