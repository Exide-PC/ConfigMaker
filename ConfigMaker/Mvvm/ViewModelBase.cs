using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Mvvm
{
    public class ViewModelBase<T>: BindableBase where T: BindableBase
    {
        protected T Model => (T)_model;

        private T _model;

        public ViewModelBase(T model)
        {
            this._model = model;
            this._model.PropertyChanged += (_, arg) => this.RaisePropertyChanged(arg.PropertyName);
        }
    }
}
