using ConfigMaker.Mvvm.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ConfigMaker.Mvvm.ViewModels
{
    public class AttachmentsViewModel : ViewModelBase<AttachmentsModel>
    {
        public ObservableCollection<ItemViewModel> Items { get; } = new ObservableCollection<ItemViewModel>();

        bool _isSelected = false;
        object _tag;

        public ICommand SelectAttachmentsCommand { get; }
        public object Tag
        {
            get => this._tag;
            set => this.SetProperty(ref _tag, value);
        }

        public AttachmentsViewModel(AttachmentsModel model): base(model)
        {
            this.SelectAttachmentsCommand = new DelegateCommand((obj) =>
            {

            });

            model.Items.CollectionChanged += (_, arg) =>
            {
                switch (arg.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            ItemModel newItem = (ItemModel)arg.NewItems[0];
                            ItemViewModel newItemVM = new ItemViewModel(newItem) { FontSize = 12 };
                            this.Items.Add(newItemVM);
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

        public bool IsSelected
        {
            get => this._isSelected;
            set => this.SetProperty(ref _isSelected, value);
        }   
    }
}
