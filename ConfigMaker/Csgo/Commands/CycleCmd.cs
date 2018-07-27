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
    public class CycleCmd : CommandCollection
    {
        public Executable Name { get; }

        public CycleCmd(string name, IEnumerable<CommandCollection> commandLines): this(name, commandLines, null) { }

        public CycleCmd(string name, IEnumerable<CommandCollection> commandLines, string[] cycleNames)
        {
            if (cycleNames != null && commandLines.Count() != cycleNames.Length)
                throw new InvalidOperationException("Число имен итераций не совпадает с количеством итераций");

            this.Name = new SingleCmd(name);
            int iterCount = commandLines.Count();

            if (cycleNames == null)
            {
                cycleNames = new string[commandLines.Count()];

                for (int i = 0; i < commandLines.Count(); i++)
                    cycleNames[i] = GenerateAliasName(i);
            }

            

            // Генерируем начало цикла
            AliasCmd headerAlias = new AliasCmd(name, new SingleCmd(cycleNames[0]));
            this.Add(headerAlias);

            // Генерируем циклические алиасы
            for (int i = 0; i < commandLines.Count(); i++)
            {
                // Копируем команды, которые будут выполняться в новый экземпляр
                CommandCollection currentBody = new CommandCollection(commandLines.ElementAt(i));

                // И к ним добавим команды управления циклом
                // Если команда последняя, то следующая команда - та, что первая в коллекции
                string nextIterationName = i == commandLines.Count() - 1 ? cycleNames[0] : cycleNames[i + 1];
                // Управляющий алиас, отвечающий за цикл
                AliasCmd transferAlias = new AliasCmd(this.Name.ToString(), new SingleCmd(nextIterationName));

                // Добавляем его в конец
                currentBody.Add(transferAlias);

                // Генерируем непосредственно строку с объявлением имени алиаса и его телом
                AliasCmd iterationAlias = new AliasCmd(cycleNames[i], currentBody);

                this.Add(iterationAlias);
            }
        }

        string GenerateAliasName(int iteration)
        {
            return $"{this.Name}_{iteration + 1}";
        }
    }
}
