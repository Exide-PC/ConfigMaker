using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Utils.ViewModels
{
    public class AttachmentsViewModel: BindableBase
    {
        public ObservableCollection<ItemViewModel> Items { get; } = 
            new ObservableCollection<ItemViewModel>();

        string _hint = string.Empty;
        bool _isSelected = false;

        public string Hint
        {
            get => this._hint;
            set => this.SetProperty(ref _hint, value);
        }

        public bool IsSelected
        {
            get => this._isSelected;
            set => this.SetProperty(ref _isSelected, value);
        }
    }
}
