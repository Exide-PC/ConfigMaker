using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ConfigMaker.Utils.ViewModels
{
    public class DynamicEntryViewModel: EntryViewModel
    {
        Visibility _visibility = Visibility.Visible;
        bool _needToggle = false;
        BindableBase _controllerViewModel;

        public DynamicEntryViewModel(BindableBase controllerViewModel)
        {
            this._controllerViewModel = controllerViewModel;
        }

        public Visibility Visibility
        {
            get => this._visibility;
            set => this.SetProperty(ref _visibility, value);
        }

        public bool NeedToggle
        {
            get => this._needToggle;
            set => this.SetProperty(ref _needToggle, value);
        }

        public BindableBase ControllerViewModel
        {
            get => this._controllerViewModel;
        }
    }
}
