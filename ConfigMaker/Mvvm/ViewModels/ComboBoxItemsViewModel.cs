using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Mvvm.ViewModels
{
    public class ComboBoxItemsViewModel: BindableBase
    {
        public ObservableCollection<string> Items { get; set; } = 
            new ObservableCollection<string>();

        int _selectedIndex = 0;

        public int SelectedIndex
        {
            get => this._selectedIndex;
            set => this.SetProperty(ref _selectedIndex, value);
        }
    }
}
