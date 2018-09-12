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
                if (this._selectedIndex != value)
                {
                    this._selectedIndex = value;
                    this.SelectedIndexChanged?.Invoke(this, null);
                }
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
                            // Сбросим выделения у всех пузырьков
                            this.Items.Where(b => b.IsSelected).ToList().ForEach(b =>
                            {
                                b.IsSelected = false;
                            });

                            // И выделим первый добавленный
                            ((ItemModel)arg.NewItems[0]).IsSelected = true;
                            this.SelectedIndex = arg.NewStartingIndex;
                            //this.DeleteButtonEnabled = true;

                            foreach (object item in arg.NewItems)
                            {
                                ItemModel castedItem = (ItemModel)item;
                                castedItem.Click += ItemClickHandler;
                                //if (clickHandler != null) castedItem.Click += clickHandler;
                            }

                            // Проверим доступность кнопки добавления, т.к. возможно дублирование запрещено
                            //this.AddButtonEnabled = this._inputValidator(this._input);

                            this.itemsCopy = this.Items.AsEnumerable();
                            break;
                        }
                    case NotifyCollectionChangedAction.Remove:
                        {
                            this.Items.Where(b => b.IsSelected).ToList()
                                .ForEach(b => b.IsSelected = false);

                            if (this.Items.Count > 0)
                            {
                                this.Items[0].IsSelected = true;
                                this.SelectedIndex = 0;
                            }   

                            ItemModel item = (ItemModel)arg.OldItems[0];
                            item.Dispose();

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
            for (int i = 0; i < this.Items.Count; i++)
            {
                ItemModel item = this.Items[i];

                if (sender == item)
                {
                    ((ItemModel)sender).IsSelected = true;
                    this.SelectedIndex = i;
                }   
                else
                    item.IsSelected = false;
            }
        }
    }
}
