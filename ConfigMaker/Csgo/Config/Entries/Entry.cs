using ConfigMaker.Csgo.Commands;
using System.Xml.Serialization;
using ConfigMaker.Csgo.Config.Enums;
using ConfigMaker.Csgo.Config.Entries.interfaces;

namespace ConfigMaker.Csgo.Config.Entries
{
    [XmlInclude(typeof(ParametrizedEntry<string[]>))]
    [XmlInclude(typeof(ParametrizedEntry<string>))]
    [XmlInclude(typeof(ParametrizedEntry<int[]>))]
    [XmlInclude(typeof(ParametrizedEntry<int>))]
    [XmlInclude(typeof(ParametrizedEntry<double[]>))]
    [XmlInclude(typeof(ParametrizedEntry<double>))]
    [XmlInclude(typeof(ParametrizedEntry<Entry[]>))]
    [XmlInclude(typeof(ParametrizedEntry<Entry>))]
    public class Entry : IEntry
    {
        public ConfigEntry PrimaryKey { get; set; }
        public Executable Cmd { get; set; }
        public EntryType Type { get; set; }
        public bool IsMetaScript { get; set; }
        public CommandCollection Dependencies { get; set; } = new CommandCollection();

        
    }
}
