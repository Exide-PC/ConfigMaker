using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Utils.ViewModels
{
    public class ActionViewModel: EntryViewModel
    {
        string _toolTip;

        public string ToolTip
        {
            get => this._toolTip;
            set => this.SetProperty(ref _toolTip, value);
        }
    }
}
