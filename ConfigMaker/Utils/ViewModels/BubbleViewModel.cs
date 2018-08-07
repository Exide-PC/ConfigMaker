using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ConfigMaker.Utils.ViewModels
{
    public class BubbleViewModel: BindableBase
    {
        string _text = string.Empty;
        bool _isSelected = false;
        double _fontSize = 12;

        public BubbleViewModel()
        {
            this.SelectCommand = new DelegateCommand(() =>
            {
                this.Click?.Invoke(this, null);
            });
        }

        public event EventHandler Click;

        public ICommand SelectCommand { get; }
        public string Text
        {
            get => this._text;
            set => this.SetProperty(ref _text, value);
        }

        public bool IsSelected
        {
            get => this._isSelected;
            set => this.SetProperty(ref _isSelected, value);
        }

        public double FontSize
        {
            get => this._fontSize;
            set => this.SetProperty(ref _fontSize, value);
        }
    }
}
