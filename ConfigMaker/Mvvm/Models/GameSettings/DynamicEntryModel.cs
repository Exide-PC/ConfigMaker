using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Mvvm.Models
{
    public class DynamicEntryModel: EntryModel
    {
        bool _isVisible = true;
        bool _needToggle = false;

        public DynamicEntryModel(BindableBase controllerViewModel)
        {
            this.ControllerModel = controllerViewModel;
        }

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

        public BindableBase ControllerModel { get; }
    }
}
