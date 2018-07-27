using ConfigMaker.Csgo.Commands;
using ConfigMaker.Csgo.Config.Entries.interfaces;
using ConfigMaker.Csgo.Config.Enums;

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
