using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Mvvm.ViewModels
{
    public class AliasControllerViewModel: InputItemsControllerViewModel
    {
        public AliasControllerViewModel(Predicate<string> inputValidator): base(inputValidator)
        {

        }
    }
}
