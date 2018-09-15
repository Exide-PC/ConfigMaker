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
            get
            {
                // Выведем или спрячем стрелки для перемещения элементов влево и вправо
                this.IsMovementEnabled = this.Model.Items.Count > 1;
                // И только после этого вернем сами элементы
                return this.Model.Items.Select(m => new ItemViewModel(m) { FontSize = 13, Height = 18 });
            }
        }
            
        bool _isSelected = false;
        bool _isMovementEnabled = false;
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
        
        public bool IsMovementEnabled
        {
            get => this._isMovementEnabled;
            set => this.SetProperty(ref _isMovementEnabled, value);
        }
    }
}
