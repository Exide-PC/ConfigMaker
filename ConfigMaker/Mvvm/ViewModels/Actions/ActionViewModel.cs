using ConfigMaker.Mvvm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ConfigMaker.Mvvm.ViewModels
{
    public class ActionViewModel: EntryViewModel
    {
        public ActionViewModel(ActionModel actionModel) : base(actionModel)
        {

        }

        public string ToolTip
        {
            get => ((ActionModel)this.Model).ToolTip;
            set => ((ActionModel)this.Model).ToolTip = value;
        }
    }
}
