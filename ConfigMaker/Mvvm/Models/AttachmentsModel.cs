using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Mvvm.Models
{
    public class AttachmentsModel: BindableBase
    {
        public ObservableCollection<ItemModel> Items { get; } =
            new ObservableCollection<ItemModel>();

        //LeakAwareCollection itemsHolder = new LeakAwareCollection(null);
        string _hint = string.Empty;

        public AttachmentsModel()
        {
            //this.itemsHolder.Items.CollectionChanged += (_, __) => this.RaisePropertyChanged(nameof(Items));
        }

        public string Hint
        {
            get => this._hint;
            set => this.SetProperty(ref _hint, value);
        }
    }
}
