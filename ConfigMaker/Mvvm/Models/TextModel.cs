﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Mvvm.Models
{
    class TextModel: BindableBase
    {
        string _text = string.Empty;

        public string Text
        {
            get => this._text;
            set => this.SetProperty(ref _text, value);
        }
    }
}
