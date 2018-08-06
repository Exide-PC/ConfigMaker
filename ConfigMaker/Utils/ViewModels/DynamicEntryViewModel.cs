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
        bool _isVisible = true;
        bool _needToggle = false;
        BindableBase _controllerViewModel;

        public bool IsVisible
        {
            get => this._isVisible;
            set => this.SetProperty(ref _isVisible, value);
        }

        public bool NeedToggle
        {
            get => this._needToggle;
            set => this.SetProperty(ref _needToggle, value);
        }

        public BindableBase ControllerViewModel
        {
            get => this._controllerViewModel;
            set => this.SetProperty(ref _controllerViewModel, value);
        }
    }
}
