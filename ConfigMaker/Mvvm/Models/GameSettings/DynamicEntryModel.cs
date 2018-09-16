using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Mvvm.Models
{
    public class DynamicEntryModel: EntryModel
    {
        bool _needToggle = false;

        public DynamicEntryModel(BindableBase controllerViewModel)
        {
            this.ControllerModel = controllerViewModel;
        }

        public bool NeedToggle
        {
            get => this._needToggle;
            set => this.SetProperty(ref _needToggle, value);
        }

        public bool IsInteger { get; set; }
        public double From { get; set; }
        public double To { get; set; }

        public BindableBase ControllerModel { get; }
    }
}
