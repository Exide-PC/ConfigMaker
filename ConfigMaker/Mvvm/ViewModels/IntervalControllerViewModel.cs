using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Mvvm.ViewModels
{
    public class IntervalControllerViewModel: BindableBase
    {
        public double _from;
        public double _to;
        public double _step;
        public double _value;
        
        public double From
        {
            get => this._from;
            set => this.SetProperty(ref _from, value);
        }

        public double To
        {
            get => this._to;
            set => this.SetProperty(ref _to, value);
        }

        public double Step
        {
            get => this._step;
            set => this.SetProperty(ref _step, value);
        }

        public double Value
        {
            get => this._value;
            set => this.SetProperty(ref _value, value);
        }
    }
}
