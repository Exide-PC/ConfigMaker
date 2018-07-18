using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ConfigMaker.Csgo.Commands
{
    public class BindCmd : Executable
    {
        public string Key { get; set; }
        public CommandCollection Commands { get; set; }

        // Для интерфейса IXmlSerializable
        public BindCmd() { }

        public BindCmd(string key, IEnumerable<Executable> commands)
        {
            this.Key = key;
            this.Commands = new CommandCollection(commands);
        }

        public BindCmd(string key, Executable command): this(key, new Executable[] { command }) { }

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();

            b.Append($"bind {this.Key} ");
            string executableString = string.Join("; ", this.Commands.Select(cmd => cmd.ToString()));
           // Оборачиваем тело в кавычки, если надо
            b.Append(Executable.NeedToWrap(this.Commands) ? $"\"{executableString}\"" : $"{executableString}");

            return b.ToString();
        }
    }
}
