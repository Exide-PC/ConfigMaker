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
    public class CustomCmdControllerViewModel : EntryViewModel
    {
        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }

        public CustomCmdControllerViewModel(InputItemsControllerModel model): base(model)
        {
            //model.PropertyChanged += (_, arg) => this.RaisePropertyChanged(arg.PropertyName);

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
            get => ((InputItemsControllerModel)this.Model).Input;
            set => ((InputItemsControllerModel)this.Model).Input = value;
        }

        public bool AdditionEnabled
        {
            get => ((InputItemsControllerModel)this.Model).AdditionEnabled;
            set => ((InputItemsControllerModel)this.Model).AdditionEnabled = value;
        }

        public bool DeletingEnabled
        {
            get => ((InputItemsControllerModel)this.Model).DeletingEnabled;
            set => ((InputItemsControllerModel)this.Model).DeletingEnabled = value;
        }
    }
}
