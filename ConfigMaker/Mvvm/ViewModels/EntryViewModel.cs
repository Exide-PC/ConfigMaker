using ConfigMaker.Mvvm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ConfigMaker.Mvvm.ViewModels
{
    public class EntryViewModel: ViewModelBase<EntryModel>
    {
        public EntryViewModel(EntryModel entryModel): base(entryModel)
        {
            this.SelectCommand = new DelegateCommand((obj) =>
            {
                this.Model.SelectCommand.Execute(null);
            });
        }

        public ICommand SelectCommand { get; }
        
        public bool IsEnabled
        {
            get => this.Model.IsEnabled;
            set => this.Model.IsEnabled = value;
        }

        public bool IsChecked
        {
            get => this.Model.IsChecked;
            set => this.Model.IsChecked = value;
        }

        public bool IsSelectable
        {
            get => this.Model.IsSelectable;
            set => this.Model.IsSelectable = value;
        }

        public string Content
        {
            get => this.Model.Content;
            set => this.Model.Content = value;
        }

        public bool IsFocused
        {
            get => this.Model.IsFocused;
            set => this.Model.IsFocused = value;
        }
    }
}
