using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Utils.ViewModels
{
    public class ExtraEntryViewModel: EntryViewModel
    {
        BindableBase _controllerViewModel;

        public ExtraEntryViewModel(BindableBase controllerViewModel)
        {
            this._controllerViewModel = controllerViewModel;
        }

        public BindableBase ControllerViewModel
        {
            get => this._controllerViewModel;
        }
    }
}
