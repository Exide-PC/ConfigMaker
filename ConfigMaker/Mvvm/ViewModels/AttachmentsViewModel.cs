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
        public IEnumerable<ItemViewModel> Items
        {
            get => this.Model.Items.Select(m => new ItemViewModel(m) { FontSize = 13, Height = 18 });
        }
            
        bool _isSelected = false;
        object _tag;

        public object Tag
        {
            get => this._tag;
            set => this.SetProperty(ref _tag, value);
        }

        public AttachmentsViewModel(AttachmentsModel model): base(model)
        {

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
