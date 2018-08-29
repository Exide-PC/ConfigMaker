using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace ConfigMaker.Mvvm.Models
{
    public class InputItemsControllerModel: EntryModel
    {
        string _input;
        bool _addButtonEnabled = false;
        bool _deleteButtonEnabled = false;
        Predicate<string> _inputValidator = (input) => true;
        LeakAwareCollection itemsHolder;

        public event EventHandler OnAddition;
        public event EventHandler OnDeleting;

        public ObservableCollection<ItemModel> Items => this.itemsHolder.Items;

        public InputItemsControllerModel(Predicate<string> inputValidator): this(inputValidator, null)
        {

        }

        public InputItemsControllerModel(Predicate<string> inputValidator, EventHandler clickHandler)
        {
            this.itemsHolder = new LeakAwareCollection(clickHandler);
            this.itemsHolder.Items.CollectionChanged += (_, __) => this.RaisePropertyChanged(nameof(Items));

            this._inputValidator = inputValidator;

            this.PropertyChanged += (_, arg) =>
           {
                if (arg.PropertyName == nameof(Input))
                    this.AdditionEnabled = this._inputValidator(this._input);
            };

            this.Items.CollectionChanged += (_, arg) =>
            {
                switch (arg.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            this.DeletingEnabled = true;
                            // Проверим заново доступность кнопки, т.к. возможно дубликаты недопустимы
                            this.AdditionEnabled = this._inputValidator(this._input);

                            break;
                        }
                    case NotifyCollectionChangedAction.Remove:
                        {
                            this.DeletingEnabled = this.Items.Count > 0;
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

        public bool AdditionEnabled
        {
            get => this._addButtonEnabled;
            set => this.SetProperty(ref _addButtonEnabled, value);
        }

        public bool DeletingEnabled
        {
            get => this._deleteButtonEnabled;
            set => this.SetProperty(ref _deleteButtonEnabled, value);
        }

        public int GetFirstSelectedIndex()
        {
            ItemModel firstSelectedItem = this.GetSelectedItem();
            return firstSelectedItem != null ? this.Items.IndexOf(firstSelectedItem) : -1;
        }

        public void InvokeAddition()
        {
            this.Items.Add(new ItemModel() { Text = this.Input });
            this.Input = string.Empty;
            this.OnAddition?.Invoke(this, null);
        }

        public void InvokeDeleting()
        {
            int selectedIndex = GetFirstSelectedIndex();
            if (selectedIndex == -1) return;

            this.Items.RemoveAt(selectedIndex);
            this.OnDeleting?.Invoke(this, null);
        }

        public ItemModel GetSelectedItem()
        {
            return this.Items.FirstOrDefault(i => i.IsSelected);
        }
    }
}
