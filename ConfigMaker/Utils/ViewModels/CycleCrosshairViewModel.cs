using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Utils.ViewModels
{
    public class CycleCrosshairViewModel: EntryViewModel
    {
        int _minumumCount = 2;
        int _maximumCount = 10;
        int _crosshairCount = 2;

        public int CrosshairCount
        {
            get => this._crosshairCount;
            set => this.SetProperty(ref _crosshairCount, value);
        }

        public int MinimumCount
        {
            get => this._minumumCount;
            set => this.SetProperty(ref _minumumCount, value);
        }

        public int MaximumCount
        {
            get => this._maximumCount;
            set => this.SetProperty(ref _maximumCount, value);
        }
    }
}
