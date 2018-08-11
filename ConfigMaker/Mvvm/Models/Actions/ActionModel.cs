namespace ConfigMaker.Mvvm.Models
{
    public class ActionModel: EntryModel
    {
        string _toolTip;

        public string ToolTip
        {
            get => this._toolTip;
            set => this.SetProperty(ref _toolTip, value);
        }
    }
}
