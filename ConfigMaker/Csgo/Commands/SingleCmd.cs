using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Csgo.Commands
{
    public class SingleCmd : Executable
    {
        public string Name { get; set; }
        //public List<string> Args { get; set; } = new List<string>();

        public SingleCmd() { }

        public SingleCmd(string name, IEnumerable<string> args): this(name)
        {
            //this.Args = args.ToList();
        }

        public SingleCmd(string name)
        {
            this.Name = name;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder($"{this.Name}");
            // Добавляем аргументы только если они есть
            //if (this.Args != null && this.Args.Count > 0)
            //    builder.Append($"{string.Join("; ", this.Args.Select(arg => arg.ToString()))}");
            return builder.ToString();
        }
    }
}
