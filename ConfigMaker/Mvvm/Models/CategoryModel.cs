using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Mvvm.Models
{
    public class CategoryModel: BindableBase
    {
        string _name = string.Empty;
        bool _isVisible = true;

        public ObservableCollection<EntryModel> Items { get; } = new ObservableCollection<EntryModel>();

        public CategoryModel()
        {
            this.Items.CollectionChanged += (_, __) => this.RaisePropertyChanged(nameof(Items));
        }

        public string Name
        {
            get => this._name;
            set => this.SetProperty(ref _name, value);
        }

        public bool IsVisible
        {
            get => this._isVisible;
            set => this.SetProperty(ref _isVisible, value);
        }
    }
}
