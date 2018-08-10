using ConfigMaker.Mvvm.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Mvvm.ViewModels
{
    public class BuyMenuViewModel: EntryViewModel
    {
        public BuyMenuViewModel(BuyMenuModel buyMenuModel): base(buyMenuModel)
        {
            foreach (WeaponCategoryModel category in buyMenuModel.Categories)
            {
                WeaponCategoryViewModel weaponCategoryVM = new WeaponCategoryViewModel(category);
                this.Categories.Add(weaponCategoryVM);
            }
        }

        public ObservableCollection<WeaponCategoryViewModel> Categories { get; } =
            new ObservableCollection<WeaponCategoryViewModel>();
    }
}
