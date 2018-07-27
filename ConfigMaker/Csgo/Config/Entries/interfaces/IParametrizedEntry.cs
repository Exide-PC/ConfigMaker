using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Csgo.Config.Entries.interfaces
{
    interface IParametrizedEntry<T>: IEntry
    {
        T Arg { get; set; }
    }
}
