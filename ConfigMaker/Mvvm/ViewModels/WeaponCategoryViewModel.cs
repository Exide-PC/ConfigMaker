using ConfigMaker.Mvvm.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Mvvm.ViewModels
{
    public class WeaponCategoryViewModel: ViewModelBase<WeaponCategoryModel>
    {
        public WeaponCategoryViewModel(WeaponCategoryModel model): base(model)
        {
            foreach (EntryModel entry in model.Weapons)
            {
                this.Weapons.Add(new EntryViewModel(entry)
                {
                    Content = entry.Content
                });
            }
        }

        public ObservableCollection<EntryViewModel> Weapons { get; } =
            new ObservableCollection<EntryViewModel>();

        public string Name => this.Model.Name;
    }
}
