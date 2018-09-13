using ConfigMaker.Mvvm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Mvvm.ViewModels
{
    public class SearchViewModel: ViewModelBase<SearchModel>
    {
        public SearchViewModel(SearchModel model): base(model)
        {
            this.Model.PropertyChanged += (_, arg) =>
            {
                if (arg.PropertyName == nameof(SearchModel.IsAdditionPossible) ||
                    arg.PropertyName == nameof(SearchModel.IsUnknownCommand))
                    this.RaisePropertyChanged(nameof(IsAddButtonVisible));
            };
        }

        public string SearchInput
        {
            get => this.Model.SearchInput;
            set => this.Model.SearchInput = value;
        }

        public string Hint
        {
            get => this.Model.Hint;
        }

        public bool IsAddButtonVisible
        {
            get => this.Model.IsUnknownCommand && this.Model.IsAdditionPossible;
        }
    }
}
