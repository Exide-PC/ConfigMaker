using ConfigMaker.Mvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Mvvm.Models
{
    public class CustomCmdControllerModel: InputItemsControllerModel
    {
        public CustomCmdControllerModel(Predicate<string> inputValidator): base(inputValidator)
        {

        }
    }
}
