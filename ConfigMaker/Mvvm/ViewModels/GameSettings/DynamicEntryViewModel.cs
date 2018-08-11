using ConfigMaker.Mvvm.Models;

namespace ConfigMaker.Mvvm.ViewModels
{
    public class DynamicEntryViewModel: EntryViewModel
    {
        public DynamicEntryViewModel(DynamicEntryModel model): base(model)
        {
            var controller = model.ControllerModel;

            if (controller is IntervalControllerModel intervalController)
            {
                this.ControllerViewModel = new IntervalControllerViewModel(intervalController);
            }
            else if (controller is ComboBoxControllerModel comboboxController)
            {
                this.ControllerViewModel = new ComboBoxControllerViewModel(comboboxController);
            }
            else if (controller is TextboxControllerModel textboxController)
            {
                this.ControllerViewModel = new TextboxControllerViewModel(textboxController);
            }
        }

        public bool IsVisible => ((DynamicEntryModel)this.Model).IsVisible;

        public bool NeedToggle => ((DynamicEntryModel)this.Model).NeedToggle;

        public BindableBase ControllerViewModel { get; }
    }
}
