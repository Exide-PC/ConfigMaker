using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Mvvm.Models
{
    public class SearchModel: BindableBase
    {
        string _searchInput = string.Empty;
        string _hint = string.Empty;
        bool _isAdditionPossible = false;
        bool _isUnknownCommand = false;

        public string SearchInput
        {
            get => this._searchInput;
            set => this.SetProperty(ref _searchInput, value);
        }

        public string Hint
        {
            get => this._hint;
            set => this.SetProperty(ref _hint, value);
        }

        public bool IsAdditionPossible
        {
            get => this._isAdditionPossible;
            set => this.SetProperty(ref _isAdditionPossible, value);
        }

        public bool IsUnknownCommand
        {
            get => this._isUnknownCommand;
            set => this.SetProperty(ref _isUnknownCommand, value);
        }
    }
}
