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
