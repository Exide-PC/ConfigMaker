using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Utils.ViewModels
{
    public class EntryViewModel: BindableBase
    {
        bool _isEnabled = true;
        bool _isChecked = false;
        string _content = string.Empty;
        string _key = null;
        bool _isFocused = false;
        bool isRestoreMode = false;

        public event EventHandler Click;

        public EntryViewModel()
        {
            this.PropertyChanged += (_, arg) =>
            {
                if (this._isEnabled && !this.isRestoreMode && arg.PropertyName == nameof(EntryViewModel.IsChecked))
                    this.Click?.Invoke(this, null);
            };
        }

        /// <summary>
        /// Необходимый метод, т.к. без моделей представления можно свободно управлять
        /// свойством IsChecked у чекбоксов без последующего вызова метода HandleEntryClick
        /// </summary>
        public void UpdateIsChecked(bool isChecked)
        {
            // Выставим флаг особого режима при котором метод Click не будет вызываться
            this.isRestoreMode = true;
            // И зададим интересующее свойство
            this.IsChecked = isChecked;
            // Сбросим флаг обратно
            this.isRestoreMode = false;
        }

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
    }
}
