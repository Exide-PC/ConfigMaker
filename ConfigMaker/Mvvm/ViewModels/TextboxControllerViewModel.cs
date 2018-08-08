using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Mvvm.ViewModels
{
    public class TextboxControllerViewModel: BindableBase
    {
        string _text;

        public string Text
        {
            get => this._text;
            set => this.SetProperty(ref _text, value);
        }
    }
}
