using ConfigMaker.Csgo.Commands;
using ConfigMaker.Csgo.Config.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Csgo.Config.Entries
{
    public class ParametrizedBindEntry<T> : BindEntry, IParametrizedEntry<T>
    {
        public T Arg { get; set; }

        public ParametrizedBindEntry()
        {
            this.Type = EntryType.Dynamic;
        }

        public ParametrizedBindEntry(
            ConfigEntry primaryKey,
            Executable onKeyDown,
            Executable onKeyRelease,
            CommandCollection dependencies,
            T args) : base(primaryKey, onKeyDown, onKeyRelease, dependencies)
        {
            this.Type = EntryType.Dynamic;
        }
    }
}
