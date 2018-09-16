using ConfigMaker.Mvvm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Mvvm.ViewModels
{
    public class CategoryViewModel: ViewModelBase<CategoryModel>
    {
        public CategoryViewModel(CategoryModel model) : base(model)
        {

        }

        public IEnumerable<EntryViewModel> Items
        {
            get
            {
                if (this.Model.Items.Count == 0) return null;

                return this.Model.Items.Select(item =>
                {
                    if (item is DynamicEntryModel)
                        return new DynamicEntryViewModel((DynamicEntryModel)item);
                    else if (item is ActionModel)
                        return new ActionViewModel((ActionModel)item);
                    else
                        return new EntryViewModel(item);

                });
            }
        }

        public string Name
        {
            get => this.Model.Name;
        }

        public bool IsVisible
        {
            get => this.Model.IsVisible;
        }
    }
}
