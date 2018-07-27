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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ConfigMaker.Csgo.Commands
{
    /// <summary>
    /// Абстрактный класс, представляющий выполнимую команду. alias key "abc; def" / cl_crosshairsize 0 / 
    /// </summary>
    /// 
    [XmlInclude(typeof(AliasCmd))]
    [XmlInclude(typeof(BindCmd))]
    [XmlInclude(typeof(SingleCmd))]
    public abstract class Executable
    {
        public abstract override string ToString();

        public static bool NeedToWrap(IEnumerable<Executable> commands)
        {
            // Если в коллекции больше одной команды или есть пробелы - оборачиваем тело бинда в кавычки, иначе не оборачиваем
            if (commands.Count() == 0 || commands.Count() > 1) return true;
            if (commands.Count() == 1 && commands.ElementAt(0).ToString().Contains(" ")) return true;
            return false;
        }

        public static string FormatNumber(double value, bool asInteger)
        {
            return (asInteger/* || value % 1 == 0*/) ? ((int)value).ToString() : value.ToString("0.00", CultureInfo.InvariantCulture);
        }

        public static bool TryParseDouble(string str, out double value)
        {
            double valueInTextbox;
            if (!double.TryParse(
                str.Trim(),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out valueInTextbox))
            {
                value = 0;
                return false;
            }
            string formatted = Executable.FormatNumber(valueInTextbox, false);
            value = double.Parse(formatted, CultureInfo.InvariantCulture);
            return true;
        }

        public static double FixDouble(double value)
        {
            double fixedValue;
            TryParseDouble(FormatNumber(value, false), out fixedValue);
            return fixedValue;
        }

        public static bool IsValidAliasName(string name)
        {
            // Проверим, что имя не пустое
            if (name.Length == 0) return false;
            // И убедимся, что все символы из ASCII кодировки
            if (Encoding.UTF8.GetByteCount(name) != name.Length) return false;
            // Укажем недопустимые символы
            char[] forbiddenChars = new char[] { ' ', ';', '\'' }; // TODO: Уточнить
            // Убедимся, что нет недопустимых символов
            if (name.Any(c => forbiddenChars.Contains(c))) return false;

            return true;
        }

        public static string GenerateToggleCmd(string cmd, double[] values, bool asInteger)
        {
            string[] formattedValues = values.Select(value => FormatNumber(value, asInteger)).ToArray();
            return $"toggle {cmd} {string.Join(" ", formattedValues)}";
        }

        public static string GenerateToggleCmd(string cmd, int[] values)
        {
            double[] castedToDouble = values.Select(i => (double)i).ToArray();
            return GenerateToggleCmd(cmd, castedToDouble, true);
        }

            /// <summary>
            /// Функция для преобразования строки по типу "+jump; -attack; -attack2" в коллекцию из трех команд.
            /// </summary>
            /// <param name="commandsStr"></param>
            /// <returns></returns>
            public static CommandCollection SplitCommands(string commandsStr)
        {
            // Разбиваем строку по двоеточиям
            string[] commands = commandsStr.Split(';').Select(part => part.Trim()).ToArray();
            CommandCollection collection = new CommandCollection();
            // Заполняем коллекцию экземплярами одиночных команд
            foreach (string part in commands)
                collection.Add(new SingleCmd(part));
            return collection;
        }
    }
}
