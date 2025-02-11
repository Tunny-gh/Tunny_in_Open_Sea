using System;

using Eto.Drawing;
using Eto.Forms;
using Eto.Serialization.Xaml;

using Tunny.Eto.Models;
using Tunny.Eto.ViewModels;

namespace Tunny.Eto.Views
{
    internal sealed class TargetStudyNameSelector : Dialog
    {
        public DropDown StudyNameDropDown { get; set; }
        public TargetStudyNameSelectorViewModel ViewModel { get; }

        public TargetStudyNameSelector()
        {
            ViewModel = new TargetStudyNameSelectorViewModel(this);
            XamlReader.Load(this);

            DataContext = ViewModel;

            LoadComplete += TargetStudyNameSelector_LoadComplete;
        }

        private void TargetStudyNameSelector_LoadComplete(object sender, EventArgs e)
        {
            if (StudyNameDropDown != null)
            {
                StudyNameDropDown.DataStore = ViewModel.StudyNameItems;
                StudyNameDropDown.ItemTextBinding = Binding.Property<NameComboBoxItem, string>(r => r.Name);

                StudyNameDropDown.SelectedValueChanged += (_, ev) =>
                {
                    ViewModel.SelectedStudyName = StudyNameDropDown.SelectedValue as NameComboBoxItem;
                };

                ViewModel.PropertyChanged += (_, ev) =>
                {
                    if (ev.PropertyName == nameof(TargetStudyNameSelectorViewModel.StudyNameItems))
                    {
                        StudyNameDropDown.DataStore = ViewModel.StudyNameItems;
                    }
                    else if (ev.PropertyName == nameof(TargetStudyNameSelectorViewModel.SelectedStudyName))
                    {
                        StudyNameDropDown.SelectedValue = ViewModel.SelectedStudyName;
                    }
                };
            }
        }
    }
}
