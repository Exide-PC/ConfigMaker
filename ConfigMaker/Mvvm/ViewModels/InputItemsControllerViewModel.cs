using ConfigMaker.Mvvm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Mvvm.ViewModels
{
    public class InputItemsControllerViewModel: ViewModelBase<InputItemsControllerModel>
    {
        public InputItemsControllerViewModel(InputItemsControllerModel model): base(model)
        {

        }

        public IEnumerable<ItemViewModel> Items
        {
            get
            {
                List<ItemViewModel> items = new List<ItemViewModel>();

                foreach (ItemModel model in this.Model.Items)
                    items.Add(new ItemViewModel(model));

                return items;
            }
        }

        public string Input
        {
            get => this.Model.Input;
            set => this.Model.Input = value;
        }

        public bool AdditionEnabled
        {
            get => this.Model.AdditionEnabled;
            set => this.Model.AdditionEnabled = value;
        }

        public bool DeletingEnabled
        {
            get => this.Model.DeletingEnabled;
            set => this.Model.DeletingEnabled = value;
        }
    }
}
