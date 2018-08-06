using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ConfigMaker.Utils.ViewModels
{
    public class TextViewModel: BindableBase
    {
        string _text = string.Empty;
        Thickness _margin = new Thickness(0);
        HorizontalAlignment _horizontalAlignment = HorizontalAlignment.Left;
        VerticalAlignment _verticalAlignment = VerticalAlignment.Top;

        public string Text
        {
            get => this._text;
            set => this.SetProperty(ref _text, value);
        }

        public Thickness Margin
        {
            get => this._margin;
            set => this.SetProperty(ref _margin, value);
        }

        public HorizontalAlignment HorizontalAlignment
        {
            get => this._horizontalAlignment;
            set => this.SetProperty(ref _horizontalAlignment, value);
        }

        public VerticalAlignment VerticalAlignment
        {
            get => this._verticalAlignment;
            set => this.SetProperty(ref _verticalAlignment, value);
        }
    }
}
