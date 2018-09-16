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
            foreach (CategoryModel category in buyMenuModel.Categories)
            {
                CategoryViewModel weaponCategoryVM = new CategoryViewModel(category);
                this.Categories.Add(weaponCategoryVM);
            }
        }

        public ObservableCollection<CategoryViewModel> Categories { get; } =
            new ObservableCollection<CategoryViewModel>();
    }
}
