using ConfigMaker.Mvvm.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Mvvm.ViewModels
{
    public class LeakAwareItemCollection
    {
        public ObservableCollection<ItemModel> Items { get; } =
            new ObservableCollection<ItemModel>();

        private IEnumerable<ItemModel> itemsCopy = null;

        public LeakAwareItemCollection(EventHandler clickHandler)
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
                            //this.DeleteButtonEnabled = true;

                            foreach (object item in arg.NewItems)
                            {
                                ItemModel castedItem = (ItemModel)item;
                                castedItem.Click += ItemClickHandler;
                                if (clickHandler != null) castedItem.Click += clickHandler;
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
                                this.Items[0].IsSelected = true;

                            ItemModel item = (ItemModel)arg.OldItems[0];
                            item.Click -= ItemClickHandler;
                            if (clickHandler != null) item.Click -= clickHandler;

                            this.itemsCopy = this.Items.AsEnumerable();
                            break;
                        }
                    case NotifyCollectionChangedAction.Reset:
                        {
                            if (this.itemsCopy == null) return;

                            this.itemsCopy.ToList().ForEach(i => 
                            {
                                ItemModel item = (ItemModel)i;
                                item.Click -= ItemClickHandler;
                                if (clickHandler != null) item.Click -= clickHandler;
                            });
                            break;
                        }
                }
            };
        }

        void ItemClickHandler(object sender, EventArgs args)
        {
            foreach (ItemModel item in this.Items)
                item.IsSelected = false;

            ((ItemModel)sender).IsSelected = true;
        }
    }
}
