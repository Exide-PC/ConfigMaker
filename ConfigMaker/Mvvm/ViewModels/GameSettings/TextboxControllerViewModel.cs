using ConfigMaker.Mvvm.Models;

namespace ConfigMaker.Mvvm.ViewModels
{
    public class TextboxControllerViewModel: ViewModelBase<TextboxControllerModel>
    {
        public TextboxControllerViewModel(TextboxControllerModel model) : base(model)
        {

        }

        public string Text
        {
            get => this.Model.Text;
            set => this.Model.Text = value;
        }
    }
}
