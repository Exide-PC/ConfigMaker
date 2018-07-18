using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Csgo.Commands
{
    public class AliasCmd : Executable
    {
        public Executable Name { get; set; }
        public CommandCollection Commands { get; set; }

        // Для интерфейса IXmlSerializable
        public AliasCmd() { }

        public AliasCmd(string name, IEnumerable<Executable> commands)
        {
            this.Name = new SingleCmd(name);
            this.Commands = new CommandCollection(commands);
        }

        public AliasCmd(string name, Executable command)
        {
            this.Name = new SingleCmd(name);
            this.Commands = new CommandCollection
            {
                command
            };
        }

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();

            b.Append($"alias {this.Name} ");
            string executableString = string.Join("; ", this.Commands.Select(cmd => cmd.ToString()));
            // Оборачиваем тело алиаса в кавычки, если надо
            b.Append(Executable.NeedToWrap(this.Commands) ? $"\"{executableString}\"": $"{executableString}");

            return b.ToString();
        }
    }
}
