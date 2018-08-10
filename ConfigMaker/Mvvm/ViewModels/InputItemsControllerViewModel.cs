using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Mvvm.ViewModels
{
    public class InputItemsControllerViewModel: EntryViewModel
    {
        string _input;
        bool _addButtonEnabled = false;
        bool _deleteButtonEnabled = false;
        Predicate<string> _inputValidator = (input) => true;
        LeakAwareItemCollection itemsHolder;

        public ObservableCollection<ItemViewModel> Items => null; //this.itemsHolder.Items; TODO:

        public InputItemsControllerViewModel(Predicate<string> inputValidator): this(inputValidator, null)
        {

        }

        public InputItemsControllerViewModel(Predicate<string> inputValidator, EventHandler clickHandler): base(null) // TODO
        {
            this.itemsHolder = new LeakAwareItemCollection(clickHandler);
            this._inputValidator = inputValidator;

            this.PropertyChanged += (_, arg) =>
           {
                if (arg.PropertyName == nameof(Input))
                    this.AddButtonEnabled = this._inputValidator(this._input);
            };

            this.Items.CollectionChanged += (_, arg) =>
            {
                switch (arg.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            this.DeleteButtonEnabled = true;
                            // Проверим заново доступность кнопки, т.к. возможно дубликаты недопустимы
                            this.AddButtonEnabled = this._inputValidator(this._input);

                            break;
                        }
                    case NotifyCollectionChangedAction.Remove:
                        {
                            this.DeleteButtonEnabled = this.Items.Count > 0;
                            break;
                        }
                }
            };
        }

       

        public string Input
        {
            get => this._input;
            set => this.SetProperty(ref _input, value);
        }

        public bool AddButtonEnabled
        {
            get => this._addButtonEnabled;
            set => this.SetProperty(ref _addButtonEnabled, value);
        }

        public bool DeleteButtonEnabled
        {
            get => this._deleteButtonEnabled;
            set => this.SetProperty(ref _deleteButtonEnabled, value);
        }

        public int GetFirstSelectedIndex()
        {
            ItemViewModel firstSelectedItem = this.GetSelectedItem();
            return firstSelectedItem != null ? this.Items.IndexOf(firstSelectedItem) : -1;
        }

        public ItemViewModel GetSelectedItem()
        {
            return this.Items.FirstOrDefault(i => i.IsSelected);
        }
    }
}
