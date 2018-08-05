using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Utils.ViewModels
{
    public class BuyViewModel: EntryViewModel
    {
        public ObservableCollection<WeaponCategoryViewModel> Categories { get; } =
            new ObservableCollection<WeaponCategoryViewModel>();
    }
}
