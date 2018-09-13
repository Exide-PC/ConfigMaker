using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Mvvm.Models
{
    public class SettingsCategoryModel: BindableBase
    {
        bool _isVisible = true;

        public string Name { get; set; }

        public bool IsVisible
        {
            get => this._isVisible;
            set => this.SetProperty(ref _isVisible, value);
        }

        public ObservableCollection<DynamicEntryModel> Items { get; set; } =
            new ObservableCollection<DynamicEntryModel>();
    }
}
