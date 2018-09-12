using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ConfigMaker.Mvvm.Models
{
    public class ItemModel: BindableBase, IDisposable
    {
        string _text = string.Empty;
        bool _isSelected = false;
        object _tag = null;

        public ItemModel()
        {
            this.SelectCommand = new DelegateCommand((obj) =>
            {
                this._click?.Invoke(this, null);
            });
        }

        event EventHandler _click;

        public event EventHandler Click
        {
            add { _click += value; }
            remove { _click -= value; }
        }

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

        public object Tag
        {
            get => this._tag;
            set => this.SetProperty(ref _tag, value);
        }

        /// <summary>
        /// Метод, очищающий событие Click от слушателей во избежание утечки памяти
        /// </summary>
        public void Dispose()
        {
            this._click = null;
        }
    }
}
