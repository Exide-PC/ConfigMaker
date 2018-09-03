using ConfigMaker.Mvvm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Mvvm.ViewModels
{
    public class CycleCrosshairViewModel: EntryViewModel
    {
        public CycleCrosshairViewModel(CycleCrosshairModel model) : base(model)
        {

        }

        public int CrosshairCount
        {
            get => ((CycleCrosshairModel)this.Model).CrosshairCount;
            set => ((CycleCrosshairModel)this.Model).CrosshairCount = value;
        }

        public int MinimumCount
        {
            get => ((CycleCrosshairModel)this.Model).MinimumCount;
            set => ((CycleCrosshairModel)this.Model).MinimumCount = value;
        }

        public int MaximumCount
        {
            get => ((CycleCrosshairModel)this.Model).MaximumCount;
            set => ((CycleCrosshairModel)this.Model).MaximumCount = value;
        }
    }
}
