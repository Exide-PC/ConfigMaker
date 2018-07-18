using ConfigMaker.Csgo.Config.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Csgo.Config.Entries
{
    public class ParametrizedEntry<T> : Entry, IParametrizedEntry<T>
    {
        public T Arg { get; set; }
    }
}
