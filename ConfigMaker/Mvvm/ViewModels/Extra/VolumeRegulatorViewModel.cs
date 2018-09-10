using ConfigMaker.Mvvm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Mvvm.ViewModels
{
    public class VolumeRegulatorViewModel: EntryViewModel
    {
        public VolumeRegulatorViewModel(VolumeRegulatorModel model): base(model)
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
            get => ((VolumeRegulatorModel)this.Model).From;
            set => ((VolumeRegulatorModel)this.Model).From = value;
        }

        public double FromMinimum
        {
            get => ((VolumeRegulatorModel)this.Model).FromMinimum;
            set => ((VolumeRegulatorModel)this.Model).FromMinimum = value;
        }

        public double FromMaximum
        {
            get => ((VolumeRegulatorModel)this.Model).FromMaximum;
            set => ((VolumeRegulatorModel)this.Model).FromMaximum = value;
        }

        public double To
        {
            get => ((VolumeRegulatorModel)this.Model).To;
            set => ((VolumeRegulatorModel)this.Model).To = value;
        }

        public double ToMinimum
        {
            get => ((VolumeRegulatorModel)this.Model).ToMinimum;
            set => ((VolumeRegulatorModel)this.Model).ToMinimum = value;
        }

        public double ToMaximum
        {
            get => ((VolumeRegulatorModel)this.Model).ToMaximum;
            set => ((VolumeRegulatorModel)this.Model).ToMaximum = value;
        }

        public double Step
        {
            get => ((VolumeRegulatorModel)this.Model).Step;
            set => ((VolumeRegulatorModel)this.Model).Step = value;
        }

        public double StepMinimum
        {
            get => ((VolumeRegulatorModel)this.Model).StepMinimum;
            set => ((VolumeRegulatorModel)this.Model).StepMinimum = value;
        }

        public double StepMaximum
        {
            get => ((VolumeRegulatorModel)this.Model).StepMaximum;
            set => ((VolumeRegulatorModel)this.Model).StepMaximum = value;
        }

        public int Mode
        {
            get => ((VolumeRegulatorModel)this.Model).Mode;
            set => ((VolumeRegulatorModel)this.Model).Mode = value;
        }
    }
}
