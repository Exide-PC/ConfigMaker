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
    public class AttachmentsViewModel: ViewModelBase<AttachmentsModel>
    {
        public ObservableCollection<ItemViewModel> Items { get; } = new ObservableCollection<ItemViewModel>();

        public AttachmentsViewModel(AttachmentsModel model): base(model)
        {
            model.Items.CollectionChanged += (_, arg) =>
            {
                switch (arg.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            ItemModel newItem = (ItemModel)arg.NewItems[0];
                            this.Items.Add(new ItemViewModel(newItem));
                            break;
                        }
                    case NotifyCollectionChangedAction.Remove:
                        {
                            int deletedIndex = arg.OldStartingIndex;
                            this.Items.RemoveAt(deletedIndex);
                            break;
                        }
                    case NotifyCollectionChangedAction.Reset:
                        {
                            this.Items.Clear();
                            break;
                        }
                }
            };
        }

        public string Hint
        {
            get => this.Model.Hint;
            set => this.Model.Hint = value;
        }
    }
}
