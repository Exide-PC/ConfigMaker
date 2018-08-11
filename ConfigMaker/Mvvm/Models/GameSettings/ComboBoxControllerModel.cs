using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Mvvm.Models
{
    public class ComboBoxControllerModel: BindableBase
    {
        public ObservableCollection<string> Items { get; } = new ObservableCollection<string>();

        int _selectedIndex = -1;
        object _selectedItem = null;

        public int SelectedIndex
        {
            get => this._selectedIndex;
            set => this.SetProperty(ref _selectedIndex, value);
        }

        public object SelectedItem
        {
            get => this._selectedItem;
            set => this.SetProperty(ref _selectedItem, value);
        }
    }
}
