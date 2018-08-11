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
        public string Name { get; set; }
        public ObservableCollection<DynamicEntryModel> Items { get; set; } =
            new ObservableCollection<DynamicEntryModel>();
    }
}
