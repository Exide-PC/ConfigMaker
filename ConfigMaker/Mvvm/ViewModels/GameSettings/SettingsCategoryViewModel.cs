using ConfigMaker.Mvvm.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Mvvm.ViewModels
{
    public class SettingsCategoryViewModel: ViewModelBase<SettingsCategoryModel>
    {
        public SettingsCategoryViewModel(SettingsCategoryModel model) : base(model)
        {
            foreach (DynamicEntryModel entry in model.Items)
            {
                this.Items.Add(new DynamicEntryViewModel(entry));
            }
        }

        public string Name => this.Model.Name;
        public bool IsVisible => this.Model.IsVisible;

        public ObservableCollection<DynamicEntryViewModel> Items { get; set; } =
            new ObservableCollection<DynamicEntryViewModel>();
    }
}
