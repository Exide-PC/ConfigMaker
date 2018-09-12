using ConfigMaker.Mvvm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ConfigMaker.Mvvm.ViewModels
{
    public class AliasSetViewModel: EntryViewModel
    {
        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }

        public AliasSetViewModel(AliasSetModel model): base(model)
        {
            this.AddCommand = new DelegateCommand((obj) => ((InputItemsControllerModel)this.Model).InvokeAddition());
            this.DeleteCommand = new DelegateCommand((obj) => ((InputItemsControllerModel)this.Model).InvokeDeleting());
        }

        public IEnumerable<ItemViewModel> Items
        {
            get
            {
                List<ItemViewModel> items = new List<ItemViewModel>();

                foreach (ItemModel model in ((InputItemsControllerModel)this.Model).Items)
                    items.Add(new ItemViewModel(model));

                return items;
            }
        }

        public string Input
        {
            get => ((AliasSetModel)this.Model).Input;
            set => ((AliasSetModel)this.Model).Input = value;
        }

        public bool AdditionEnabled
        {
            get => ((AliasSetModel)this.Model).AdditionEnabled;
            set => ((AliasSetModel)this.Model).AdditionEnabled = value;
        }

        public bool DeletingEnabled
        {
            get => ((AliasSetModel)this.Model).DeletingEnabled;
            set => ((AliasSetModel)this.Model).DeletingEnabled = value;
        }
    }
}
