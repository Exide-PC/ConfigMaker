using ConfigMaker.Mvvm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ConfigMaker.Mvvm.ViewModels
{
    public class ItemViewModel: ViewModelBase<ItemModel>
    {
        double _fontSize = 14;

        public ItemViewModel(ItemModel model): base(model)
        {
            this.SelectCommand = new DelegateCommand((obj) =>
            {
                model.SelectCommand.Execute(null);
            });
        }

        public ICommand SelectCommand { get; }

        public string Text
        {
            get => this.Model.Text;
            set => this.Model.Text = value;
        }

        public bool IsSelected
        {
            get => this.Model.IsSelected;
            set => this.Model.IsSelected = value;
        }

        public double FontSize
        {
            get => this._fontSize;
            set => this.SetProperty(ref _fontSize, value);
        }
    }
}
