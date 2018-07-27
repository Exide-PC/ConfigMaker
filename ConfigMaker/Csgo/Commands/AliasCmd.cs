/*
MIT License

Copyright (c) 2018 Exide-PC

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

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
