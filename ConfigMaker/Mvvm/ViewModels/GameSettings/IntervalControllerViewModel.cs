using ConfigMaker.Mvvm.Models;

namespace ConfigMaker.Mvvm.ViewModels
{
    public class IntervalControllerViewModel: ViewModelBase<IntervalControllerModel>
    {
        public IntervalControllerViewModel(IntervalControllerModel model) : base(model)
        {

        }
        
        public double From
        {
            get => this.Model.From;
            set => this.Model.From = value;
        }

        public double To
        {
            get => this.Model.To;
            set => this.Model.To = value;
        }

        public double Step
        {
            get => this.Model.Step;
            set => this.Model.Step = value;
        }

        public double Value
        {
            get => this.Model.Value;
            set => this.Model.Value = value;
        }
    }
}
