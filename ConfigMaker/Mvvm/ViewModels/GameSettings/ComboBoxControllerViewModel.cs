using ConfigMaker.Mvvm.Models;
using System.Collections.ObjectModel;

namespace ConfigMaker.Mvvm.ViewModels
{
    public class ComboBoxControllerViewModel: ViewModelBase<ComboBoxControllerModel>
    {
        public ComboBoxControllerViewModel(ComboBoxControllerModel model) : base(model)
        {

        }

        public ObservableCollection<string> Items => this.Model.Items;

        public int SelectedIndex
        {
            get => this.Model.SelectedIndex;
            set => this.Model.SelectedIndex = value;
        }

        public object SelectedItem
        {
            get => this.Model.SelectedItem;
            set => this.Model.SelectedItem = value;
        }
    }
}
