using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Mvvm.ViewModels
{
    public class VolumeRegulatorControllerViewModel: EntryViewModel
    {
        public VolumeRegulatorControllerViewModel(): base(null) // TODO
        {

        }

        double _from = 0;
        double _fromMinimum = 0;
        double _fromMaximum = 0.99;
        double _to = 1;
        double _toMinimum = 0.01;
        double _toMaximum = 1;
        double _step = 0.25;
        double _stepMinimum = 0.01;
        double _stepMaximum = 1;
        int _mode = 0;

        public double From
        {
            get => this._from;
            set => this.SetProperty(ref _from, value);
        }

        public double FromMinimum
        {
            get => this._fromMinimum;
            set => this.SetProperty(ref _fromMinimum, value);
        }

        public double FromMaximum
        {
            get => this._fromMaximum;
            set => this.SetProperty(ref _fromMaximum, value);
        }

        public double To
        {
            get => this._to;
            set => this.SetProperty(ref _to, value);
        }

        public double ToMinimum
        {
            get => this._toMinimum;
            set => this.SetProperty(ref _toMinimum, value);
        }

        public double ToMaximum
        {
            get => this._toMaximum;
            set => this.SetProperty(ref _toMaximum, value);
        }

        public double Step
        {
            get => this._step;
            set => this.SetProperty(ref _step, value);
        }

        public double StepMinimum
        {
            get => this._stepMinimum;
            set => this.SetProperty(ref _stepMinimum, value);
        }

        public double StepMaximum
        {
            get => this._stepMaximum;
            set => this.SetProperty(ref _stepMaximum, value);
        }

        public int Mode
        {
            get => this._mode;
            set => this.SetProperty(ref _mode, value);
        }
    }
}
