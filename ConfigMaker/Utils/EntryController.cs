using ConfigMaker.Csgo.Config.Entries.interfaces;
using ConfigMaker.Mvvm.Models;
using ConfigMaker.Mvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ConfigMaker.Mvvm.Models.MainModel;

namespace ConfigMaker.Utils
{
    class EntryController
    {
        public EntryModel Model { get; set; }
        public Action<IEntry> UpdateUI { get; set; }
        public Func<IEntry> Generate { get; set; }
        public Action Focus { get; set; } = () => { };
        public Action Restore { get; set; }
        public Action<EntryStateBinding> HandleState { get; set; } = (state) => { };
    }
}
