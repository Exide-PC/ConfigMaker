using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ConfigMaker.Csgo.Commands
{
    [XmlInclude(typeof(CycleCmd))]
    [XmlInclude(typeof(MetaCmd))]
    public class CommandCollection: List<Executable>
    {
        public CommandCollection()
        {

        }

        public CommandCollection(string commands): this(Executable.SplitCommands(commands))
        {

        }

        public CommandCollection(Executable command)
        {
            this.Add(command);
        }
        
        public CommandCollection(IEnumerable<Executable> commands)
        {
            foreach (var cmd in commands)
                this.Add(cmd);
        }
        public override string ToString()
        {
            return $"{string.Join("; ", this.Select(cmd => cmd.ToString()))}";
        }
    }
}
