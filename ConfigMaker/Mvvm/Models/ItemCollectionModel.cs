using ConfigMaker.Mvvm.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Mvvm.Models
{
    public class ItemCollectionModel
    {
        public ObservableCollection<ItemModel> Items { get; } =
            new ObservableCollection<ItemModel>();

        int _selectedIndex = -1;
        public int SelectedIndex
        {
            get => this._selectedIndex;
            set
            {
                // Сбросим все выделения
                foreach (ItemModel item in this.Items)
                    item.IsSelected = false;

                if (value != -1)
                    this.Items[value].IsSelected = true;

                bool selectionChanged = this._selectedIndex != value;
                this._selectedIndex = value;

                if (selectionChanged)
                    this.SelectedIndexChanged?.Invoke(this, null);
            }
        }

        public event EventHandler SelectedIndexChanged;
        IEnumerable<ItemModel> itemsCopy = null;

        public ItemCollectionModel()
        {
            this.Items.CollectionChanged += (_, arg) =>
            {
                switch (arg.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            this.SelectedIndex = arg.NewStartingIndex;

                            foreach (object item in arg.NewItems)
                            {
                                ItemModel castedItem = (ItemModel)item;
                                castedItem.Click += ItemClickHandler;
                            }
                            
                            this.itemsCopy = this.Items.AsEnumerable();
                            break;
                        }
                    case NotifyCollectionChangedAction.Remove:
                        {
                            ItemModel item = (ItemModel)arg.OldItems[0];
                            item.Dispose();

                            if (this.Items.Count > 0)
                                this.SelectedIndex = 0;
                            else
                                this.SelectedIndex = -1;

                            this.itemsCopy = this.Items.AsEnumerable();
                            break;
                        }
                    case NotifyCollectionChangedAction.Reset:
                        {
                            if (this.itemsCopy == null) return;

                            this.itemsCopy.ToList().ForEach(i => 
                            {
                                ItemModel item = (ItemModel)i;
                                item.Dispose();
                            });
                            this.SelectedIndex = -1;
                            break;
                        }
                }
            };
        }

        void ItemClickHandler(object sender, EventArgs args)
        {
            this.SelectedIndex = this.Items.IndexOf((ItemModel)sender);
        }
    }
}
