using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace ConfigMaker.Mvvm.Models
{
    public class InputItemsControllerModel: EntryModel
    {
        string _input = string.Empty;
        bool _addButtonEnabled = false;
        bool _deleteButtonEnabled = false;
        //Predicate<string> _inputValidator = (input) => true;
        ItemCollectionModel itemsHolder;

        public event EventHandler OnAddition;
        public event EventHandler OnDeleting;
        public event EventHandler SelectedIndexChanged;
        public Func<string, ItemModel> ItemCreator = (input) => new ItemModel() { Text = input };

        public Predicate<string> InputValidator { get; set; } = (input) => true;

        public ObservableCollection<ItemModel> Items => this.itemsHolder.Items;
        public int SelectedIndex => this.itemsHolder.SelectedIndex;
        
        public InputItemsControllerModel()
        {
            this.itemsHolder = new ItemCollectionModel();
            this.itemsHolder.Items.CollectionChanged += (_, __) => this.RaisePropertyChanged(nameof(Items));
            this.itemsHolder.SelectedIndexChanged += (_, __) => this.SelectedIndexChanged?.Invoke(this, null);

            //this._inputValidator = inputValidator;

            this.PropertyChanged += (_, arg) =>
           {
                if (arg.PropertyName == nameof(Input))
                    this.AdditionEnabled = this.InputValidator(this._input);
            };

            this.Items.CollectionChanged += (_, arg) =>
            {
                switch (arg.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            this.DeletingEnabled = true;
                            // Проверим заново доступность кнопки, т.к. возможно дубликаты недопустимы
                            this.AdditionEnabled = this.InputValidator(this._input);
                            this.OnAddition?.Invoke(this, null);
                            break;
                        }
                    case NotifyCollectionChangedAction.Remove:
                        {
                            this.DeletingEnabled = this.Items.Count > 0;
                            this.OnDeleting?.Invoke(this, null);
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

        public void InvokeAddition()
        {
            this.InvokeAddition(this.Input);
        }

        public void InvokeAddition(string input)
        {
            ItemModel newItem = this.ItemCreator(input);
            this.InvokeAddition(newItem);
        }

        public void InvokeAddition(ItemModel item)
        {
            // Проверим, что добавление возможно в текущем состоянии. 
            // И если возможно - проверим правильность ввода
            if (this.IsEnabled && this.InputValidator(item.Text) == true)
            {
                // Если элемент не в конфиге, то имитируем нажатие пользователя
                if (this.IsChecked == false)
                {
                    this.IsChecked = true;
                    this.OnClick();
                }

                this.Items.Add(item);
                this.Input = string.Empty;
            }
        }

        public void InvokeDeleting()
        {
            this.Items.RemoveAt(this.SelectedIndex);
        }

        public ItemModel GetSelectedItem()
        {
            return this.SelectedIndex != -1 ? this.Items[this.SelectedIndex] : null;
        }


    }
}
