using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Utils.ViewModels
{
    // Класс для поддержания строгой типизации, иначе в BuyViewModel было бы ObservableCollection<ObservableCollection<object>>
    public class WeaponCategoryViewModel: BindableBase
    {
        public ObservableCollection<EntryViewModel> Weapons { get; } = new ObservableCollection<EntryViewModel>();

        string _categoryName = string.Empty;
        
        public string Name
        {
            get => this._categoryName;
            set => this.SetProperty(ref _categoryName, value);
        }

        
    }
}
