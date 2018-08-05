using ConfigMaker.Csgo.Config.Entries.interfaces;
using ConfigMaker.Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ConfigMaker.MainWindow;

namespace ConfigMaker.Utils
{
    class EntryControllerV2
    {
        public EntryViewModel AttachedViewModel { get; set; }
        public Action<IEntry> UpdateUI { get; set; }
        public Func<IEntry> Generate { get; set; }
        public Action Focus { get; set; } = () => { };
        public Action Restore { get; set; }
        public Action<EntryStateBinding> HandleState { get; set; } = (state) => { };
    }
}
