using ConfigMaker.Mvvm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ConfigMaker.Mvvm.Models.VirtualKeyboardModel;

namespace ConfigMaker.Mvvm.ViewModels
{
    public class VirtualKeyboardViewModel: ViewModelBase<VirtualKeyboardModel>
    {
        public Dictionary<string, KeyState> KeyStates => this.Model.KeyStates;

        public VirtualKeyboardViewModel(VirtualKeyboardModel model) : base(model) { }

        public void Raise()
        {
            this.RaisePropertyChanged(nameof(KeyStates));
        }
    }
}
