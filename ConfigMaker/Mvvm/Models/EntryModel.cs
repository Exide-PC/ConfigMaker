using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ConfigMaker.Mvvm.Models
{
    public class EntryModel: BindableBase
    {
        bool _isEnabled = true;
        bool _isChecked = false;
        bool _isSelectable = true;
        string _content = string.Empty;
        string _key = null;
        bool _isFocused = false;
        object _arg = null;

        public event EventHandler Click;

        public EntryModel()
        {
            this.SelectCommand = new DelegateCommand((obj) =>
            {
                this.Click?.Invoke(this, null);
            });
        }

        public ICommand SelectCommand { get; }

        public string Key
        {
            get => _key;
            set => this.SetProperty(ref _key, value);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => this.SetProperty(ref _isEnabled, value);
        }

        public bool IsChecked
        {
            get => _isChecked;
            set => this.SetProperty(ref _isChecked, value);
        }

        public bool IsSelectable
        {
            get => _isSelectable;
            set => this.SetProperty(ref _isSelectable, value);
        }

        public string Content
        {
            get => _content;
            set => this.SetProperty(ref _content, value);
        }

        public bool IsFocused
        {
            get => this._isFocused;
            set
            {
                if (!value) return;

                this.SetProperty(ref _isFocused, true);
                // Сбросим свойство обратно, чтобы можно было фокусить элемент снова
                //this.SetProperty(ref _isFocused, false);
            }
        }

        public object Arg
        {
            get => this._arg;
            set => this.SetProperty(ref _arg, value);
        }
    }
}
