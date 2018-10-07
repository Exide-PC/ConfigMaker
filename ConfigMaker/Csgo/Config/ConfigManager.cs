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

using ConfigMaker.Csgo.Commands;
using ConfigMaker.Csgo.Config.Entries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;
using ConfigMaker.Csgo.Config.Enums;
using ConfigMaker.Csgo.Config.Entries.interfaces;

namespace ConfigMaker.Csgo.Config
{
    public class ConfigManager: IXmlSerializable
    {
        //public string TargetPath { get; set; }

        Dictionary<KeySequence, List<BindEntry>> entries =
            new Dictionary<KeySequence, List<BindEntry>>();        

        public Dictionary<KeySequence, List<BindEntry>> Entries =>
            this.entries.ToDictionary(p => p.Key, p => p.Value);

        List<Entry> defaultEntries = new List<Entry>();

        public List<Entry> DefaultEntries => defaultEntries.ToList();

        public void AddEntry(Entry cfgEntry)
        {
            if (cfgEntry.Type == EntryType.Semistatic)
                this.UpdateSemistaticEntry(cfgEntry);

            // Определим по какому индексу вставить элемент
            Entry replacedEntry = this.defaultEntries.FirstOrDefault(e => e.PrimaryKey == cfgEntry.PrimaryKey);
            // Если есть заменяемый элемент, то вставляем новый на его место, иначе вставляем в конец
            int targetIndex = replacedEntry != null ? this.defaultEntries.IndexOf(replacedEntry) : this.defaultEntries.Count;

            // Удаляем прошлый элемент и вставляем на нужный индекс новый
            this.RemoveEntry(cfgEntry);
            this.defaultEntries.Insert(targetIndex, cfgEntry);
        }

        public void AddEntry(KeySequence keySequence, BindEntry cfgEntry)
        {
            int count = keySequence.Keys.Length;
            if (count == 0 || count > 2) // Проверим, что задано верное число клавиш
                throw new InvalidOperationException($"Кол-во кнопок в последовательности не может быть {count}");

            // Если элемент полустатический, то обновим его в менеджере
            if (cfgEntry.Type == EntryType.Semistatic)
                this.UpdateSemistaticEntry(cfgEntry);

            // Добавляем список для последовательности клавиш, если такой еще нет
            if (this.entries.ContainsKey(keySequence))
            {
                // Получим коллекцию элементов, привязанных к клавише/сочетанию клавиш
                List<BindEntry> entries = this.entries[keySequence];

                // Найдем первый несовместимый элемент на место которого будем вставлять новый
                BindEntry replacedEntry = entries.FirstOrDefault(e => !AreCompatible(cfgEntry, e));
                int targetIndex = replacedEntry != null ? entries.IndexOf(replacedEntry) : entries.Count;

                // Обновляем список элементов для последовательности клавиш, оставив только совместимые с новым элементы
                entries = entries.Where(e => AreCompatible(e, cfgEntry)).ToList();
                entries.Insert(targetIndex, cfgEntry);

                //this.entries[keysequence] = this.entries[keysequence]
                //        .where(e => arecompatible(e, cfgentry))
                //        .tolist();

                // Вставляем по вычисленному индексу наш элемент
                this.entries[keySequence] = entries; // .Insert(targetIndex, cfgEntry);
            }
            else
            {
                this.entries.Add(keySequence, new List<BindEntry>());
                // Добавляем в список наш элемент
                this.entries[keySequence].Add(cfgEntry);
            }                
        }

        public void RemoveEntry(Entry cfgEntry)
        {
            this.defaultEntries = this.defaultEntries.Where(entry => entry.PrimaryKey != cfgEntry.PrimaryKey).ToList();
        }

        void UpdateArg(IEntry source, IEntry target)
        {
            Type sourceType = source.GetType();
            Type targetType = target.GetType();

            if (!sourceType.IsGenericType || !targetType.IsGenericType)
                throw new InvalidOperationException("Процедура переноса аргумента недопустима для необобщенного элемента");

            Type nongenericTargetType = typeof(IParametrizedEntry<>);
            // Узнаем тип T и зададим его как обобщенный аргумент в новом типе
            Type genericArgType = sourceType.GenericTypeArguments[0];
            Type targetGenericType = nongenericTargetType.MakeGenericType(genericArgType);

            foreach (System.Reflection.PropertyInfo prop in targetGenericType.GetProperties())
            {
                object arg = prop.GetValue(source);
                prop.SetValue(target, arg);
            }

            target.Dependencies = source.Dependencies;
        }

        void UpdateSemistaticEntry(IEntry newEntry)
        {
            List<IEntry> collisionEntries = this.entries
                .SelectMany(p => p.Value).Where(e => e.PrimaryKey == newEntry.PrimaryKey)
                .Select(e => (IEntry) e).ToList();

            Entry defaultCollisionEntry = this.defaultEntries.FirstOrDefault(e => e.PrimaryKey == newEntry.PrimaryKey);

            if (defaultCollisionEntry != null)
                collisionEntries.Add(defaultCollisionEntry);

            collisionEntries.ForEach(targetEntry =>
            {
                UpdateArg(newEntry, targetEntry);
            });
        }

        static bool AreCompatible(BindEntry first, BindEntry second)
        {
            // Если разные ключи, то совместимы точно.
            if (first.PrimaryKey != second.PrimaryKey) return true;
            // Если одинаковые ключи и оба - метаскрипты, то несовместимы
            if (first.IsMetaScript && second.IsMetaScript) return false;

            if (first.IsMetaScript != second.IsMetaScript)
                throw new InvalidDataException($"По одинаковому ключу ({first.PrimaryKey.ToString()}) имеется мета-скрипт и просто команда");

            // Если одинаковые ключи, оба не мета-скрипты и обрабатываются разные состояния, то совместимы
            // Если первый обрабатывает только нажатие, а второй только отжатие, то совместимы
            if (first.OnKeyDown != null && first.OnKeyRelease == null && second.OnKeyDown == null && second.OnKeyRelease != null)
                return true;
            if (first.OnKeyDown == null && first.OnKeyRelease != null && second.OnKeyDown != null && second.OnKeyRelease == null)
                return true;

            return false;

        }
        
        public void RemoveEntry(KeySequence keySequence, BindEntry cfgEntry)
        {
            this.entries[keySequence] = this.entries[keySequence]
                .Where(e => AreCompatible(e, cfgEntry)).ToList();

            if (this.entries[keySequence].Count == 0)
                this.entries.Remove(keySequence);
        }

        public void GenerateCfg(string filePath)
        {
            // Сначала выпишем все зависимости для элементов
            // Обработаем все команды, привязанные к одиночным кнопкам и не являющимся первыми в каких-либо комбинациях
            // 
            
            // Очередь по добавлению зависимостей
            Queue<Dependency> dependencies = new Queue<Dependency>();
            
            // Откроем файловый поток
            using (StreamWriter writer = File.CreateText(filePath)) // TODO: Проверить на конфигах разной длины
            {
                // Сначала запишем все зависимости
                // Т.к. один и тот же скрипт с зависимостями может быть на разных сочетаниях - сгруппируем все элементы по именам
                // Исходим из того, что одно и то же имя (первичный ключ) соответствует идентичным скриптам 
                // и достаточно записать их зависимости один раз
                IEnumerable<IEntry> castedEntries = this.Entries.Values
                    .SelectMany(list => list.Select(e => (IEntry)e))
                    .Concat(this.defaultEntries);

                // Группируем по первичному ключу статические и полустатические элементы, т.к. зависимости у них одинаковые
                IEnumerable<IGrouping<string, IEntry>> groupedByPK =
                    castedEntries.Where(e => e.Type != EntryType.Dynamic).GroupBy(entry => entry.PrimaryKey);
                
                // Пройдемся по каждой группе и выпишем их зависимости
                foreach (IGrouping<string, IEntry> group in groupedByPK)
                {
                    IEntry firstElement = group.ElementAt(0);
                    dependencies.Enqueue(new Dependency() { Name = firstElement.PrimaryKey.ToString(), Commands = firstElement.Dependencies });
                }

                // Теперь пройдемся по нестатическим элементам и тоже сохраним зависимости
                foreach (IEntry entry in castedEntries.Where(e => e.Type == EntryType.Dynamic))
                {
                    //string keyDescription = (entry.OnKeyDown != null ? entry.OnKeyDown : entry.OnKeyRelease).ToString();
                    //string dependencyName = $"{entry.PrimaryKey.ToString()} - {keyDescription}";

                    dependencies.Enqueue(
                        new Dependency()
                        {
                            Name = $"{entry.PrimaryKey.ToString()}{(entry.Cmd != null? $" - {entry.Cmd}": "")}",//dependencyName,
                            Commands = entry.Dependencies
                        });
                }

                // Соберем все привязываемые элементы вместе
                IEnumerable<BindEntry> allBindEntries = new List<BindEntry>();

                // Объединяем все коллекции в единую
                allBindEntries = this.entries.Values.SelectMany(list => list);

                // Словарь с биндами по дефолту для ВСЕХ кнопок
                Dictionary <string, Executable> defaultKeyBinds = new Dictionary<string, Executable>();
                // Словарь динамических биндов, которые формируются по ходу итераций
                Dictionary<string, DynamicCmd> dynamicBinds = new Dictionary<string, DynamicCmd>();
                // Узнаем все клавиши, которые задействованы в конфиге
                HashSet<string> keySet = new HashSet<string>();
                foreach (KeySequence keySequence in this.entries.Keys)
                    foreach (string key in keySequence.Keys)
                        keySet.Add(key);
                // Пусть каждая из них по умолчанию разбиндена
                foreach (string key in keySet)
                    dynamicBinds.Add(key, new DynamicCmd(new SingleCmd($"unbind {key}")));

                // Обработаем сначала все команды, привязанные к одиночным кнопкам 
                // и не являющимся первыми в каких-либо комбинациях, их мы обработаем позже
                foreach (var pair in this.entries.Where(e => e.Key.Keys.Length == 1
                    && this.entries.Where(innerEntry => innerEntry.Key.Keys.Length == 2 && innerEntry.Key[0] == e.Key[0]).Count() == 0))
                {
                    string key = pair.Key[0];
                    List<BindEntry> entries = pair.Value;

                    // Если ничего не привязано, то ничего и не делаем
                    if (entries.Count == 0) continue;

                    // Узнаем статический ли он (может ли бинд на клавишу измениться)
                    // То есть проверяем есть ли сочетания, где вторая клавиша - текущая
                    bool isStaticKey = this.entries.Where(entry => entry.Key.Keys.Length == 2)
                        .Where(innerEntry => innerEntry.Key.Keys[1] == key).Count() == 0;

                    // Проверим нужно ли генерировать мета-алиас к клавише. 
                    // Нужно, если хотя бы одна команда задана при отжатии клавиши, либо установлено только отжатие,
                    // либо если клавиша не статическая и необходимо упростить название бинда в любом случае
                    bool needMetaScript = entries.Any(e => e.OnKeyDown == null || e.OnKeyRelease != null) || !isStaticKey;

                    if (needMetaScript)
                    {
                        // Если ко всей клавише привязан один мета-скрипт, то обходимся без генерации зависимостей
                        if (entries.Count == 1 && entries[0].IsMetaScript)//this.IsMetaScript(entries[0]))
                        {
                            // Просто создаем команду по типу: bind e +use
                            // Обновляя динамический бинд
                            dynamicBinds[key].Cmd = new BindCmd(key, entries[0].OnKeyDown);
                        }
                        else
                        {
                            string metaName = $"meta_{key}";
                            // Формируем мета-алиас
                            MetaCmd metaCmd = this.GenerateMetaCmd(metaName, entries);
                            // И вешаем его на клавишу
                            // Запоминаем для нестатической клавиши бинд по умолчанию, обновляя динамический бинд
                            dynamicBinds[key].Cmd = new BindCmd(key, metaCmd.AliasOnKeyDown.Name);
                            // Добавляем зависимость
                            dependencies.Enqueue(new Dependency() { Name = metaName, Commands = metaCmd });
                        }
                    }
                    else
                    {
                        // Клавиша гарантированно статическая, обрабатывается только нажатие и нет мета-скриптов
                        BindCmd bind = new BindCmd(key, entries.Select(e => e.OnKeyDown));
                        // Обновляем динамическую команду с биндом
                        dynamicBinds[key].Cmd = bind;
                    }
                }


                // Обработали все одиночные кнопки, обработаем теперь все парные
                // Переведем бинды, привязанные к сочетанию клавиш в нормальный вид.
                // Получим все элементы словаря, где ключ состоит из 2 клавиш
                IEnumerable<KeyValuePair<KeySequence, List<BindEntry>>> combinedKeysEntries =
                     this.entries.Where(e => e.Key.Keys.Length == 2);

                // Группируем их по первой клавише
                IEnumerable<IGrouping<string, KeyValuePair<KeySequence, List<BindEntry>>>> groupedByFirstKey =
                    combinedKeysEntries.GroupBy(dictEntry => dictEntry.Key[0]);

                // Теперь нужно сформировать мета-скрипты, с именемами +meta_key1_key2/-meta_key1_key2
                // При чем каждая группа будет различаться значением key2
                foreach (var group in groupedByFirstKey)
                {
                    // Необходимо сформировать мета-скрипты для первой клавиши
                    // Для этого нужно получить набор команд привязанный чисто к этой клавише и туда добавить мета-скрипты
                    // Либо сформировать новую коллекцию
                    string key1 = group.Key;
                    KeySequence monoKeySequence = new KeySequence(key1);
                    
                    // Мета-скрипт для первой кнопки, который мы постепенно заполним командами
                    MetaCmd key1MetaCmd;
                    string key1MetaScriptName = $"meta_{key1}";

                    // Если есть такой ключ
                    if (this.entries.ContainsKey(monoKeySequence))
                        key1MetaCmd = GenerateMetaCmd(key1MetaScriptName, this.entries[monoKeySequence]);
                    else
                        key1MetaCmd = new MetaCmd(key1MetaScriptName, new CommandCollection(), new CommandCollection());
                    
                    // Пройдемся по комбинациям парам кнопок, у которых первая общая и заполним мета-скрипт первой кнопки командами
                    foreach (KeyValuePair<KeySequence, List<BindEntry>> pair in group)
                    {
                        KeySequence keySequence = pair.Key;
                        List<BindEntry> cfgEntries = pair.Value;

                        // Действие при нажатии определим далее
                        Executable onKey1DownKey2action;
                        // Действие при отжатии говорим брать из динамической команды
                        Executable onKey1ReleaseKey2action = dynamicBinds[keySequence[1]];

                        bool isWholeKeyMetaScript = cfgEntries.Count == 1 && cfgEntries[0].IsMetaScript;//this.IsMetaScript(cfgEntries[0]);

                        // Если ко всей клавише привязан один мета-скрипт, то обходимся без генерации зависимостей
                        if (isWholeKeyMetaScript)
                        {
                            onKey1DownKey2action = new BindCmd(keySequence[1], cfgEntries[0].OnKeyDown);
                        }
                        else
                        {
                            string metaScriptName = $"meta_{keySequence[0]}_{keySequence[1]}";
                            // Формируем мета-скрипт
                            MetaCmd metaCmd = this.GenerateMetaCmd(metaScriptName, cfgEntries);

                            // Добавляем в очередь зависимостей новую для сочетания кнопок
                            dependencies.Enqueue(new Dependency() { Name = metaScriptName, Commands = metaCmd });

                            // Добавим бинд мета-скрипта на кнопку key2
                            // Два бинда на привязку
                            onKey1DownKey2action = new BindCmd(keySequence[1], metaCmd.AliasOnKeyDown.Name);
                        }

                        key1MetaCmd.AliasOnKeyDown.Commands.Add(onKey1DownKey2action);
                        key1MetaCmd.AliasOnKeyRelease.Commands.Add(onKey1ReleaseKey2action);
                    }

                    //// Сформировали набор команд при нажатии первой кнопки
                    //// Создадим мета-скрипт
                    BindCmd key1Bind = new BindCmd(key1, key1MetaCmd.AliasOnKeyDown.Name);
                    // Обновляем динамическую команду с биндом на 1-ю клавишу
                    dynamicBinds[key1].Cmd = key1Bind;
                    // Добавляем зависимость для первой клавиши
                    dependencies.Enqueue(new Dependency() { Name = key1MetaScriptName, Commands = key1MetaCmd });
                }


                // --- Сгенерируем сам конфиг ---
                
                // Выпишем зависимости
                if (dependencies.Where(d => d.Commands.Count > 0).Count() > 0)
                {
                    writer.WriteLine("// --- Dependencies ---");
                    writer.WriteLine();

                    while (dependencies.Count > 0)
                    {
                        Dependency dependency = dependencies.Dequeue();

                        if (dependency.Commands.Count == 0) continue;

                        writer.WriteLine($"// {dependency.Name}");
                        foreach (Executable cmd in dependency.Commands)
                            writer.WriteLine(cmd.ToString());
                        writer.WriteLine();
                    }
                    writer.WriteLine();
                }

                // Выпишем бинды
                IEnumerable<KeyValuePair<string, DynamicCmd>> dynamicBindPairs = 
                    dynamicBinds.Where(p => p.Value.ToString().StartsWith("bind"));

                if (dynamicBindPairs.Count() > 0)
                {
                    writer.WriteLine("// --- Binds ---");
                    writer.WriteLine();

                    // Записываем только те команды, которые начинаются со слова bind
                    foreach (KeyValuePair<string, DynamicCmd> dynamicBindPair in dynamicBindPairs)
                    {
                        writer.WriteLine(dynamicBindPair.Value.ToString());
                    }

                    writer.WriteLine();
                }

                // Выпишем комментарий
                IEnumerable<IEntry> defaultEntries = this.defaultEntries.Where(e => e.Cmd != null);
                if (defaultEntries.Count() > 0) 
                {
                    writer.WriteLine("// --- Default settings ---");
                    writer.WriteLine();

                    // Выпишем дефолтные параметры
                    foreach (IEntry entry in defaultEntries)
                        writer.WriteLine(entry.Cmd.ToString());
                    
                    writer.WriteLine();
                }

                // Выведем в консоль фирменный комментарий
                writer.WriteLine("echo \"\"");
                writer.WriteLine("echo \"Config is created by Exide's Config Maker\"");
                writer.WriteLine("echo \"----------- blog.exideprod.com ----------\"");
                writer.WriteLine("echo \"------------ vk.com/exideprod -----------\"");
                writer.WriteLine("echo \"\"");
            } // Закрываем файловый поток      
        }

        MetaCmd GenerateMetaCmd(string name, IEnumerable<BindEntry> cfgEntries)
        {
            CommandCollection onKeyDown = new CommandCollection();
            CommandCollection onKeyRelease = new CommandCollection();

            // Если передан один

            // Формируем коллекцию команд при нажатии и отжатии клавиш
            foreach (BindEntry entry in cfgEntries)
            {
                if (entry.OnKeyDown != null)
                    onKeyDown.Add(entry.OnKeyDown);
                if (entry.OnKeyRelease != null)
                    onKeyRelease.Add(entry.OnKeyRelease);
            }

            // Формируем мета-алиас
            return new MetaCmd($"{name}", onKeyDown, onKeyRelease);
        }

        public XmlSchema GetSchema() => null;

        public void ReadXml(XmlReader reader)
        {
            reader.Read();

            XmlSerializer keysSerializer = new XmlSerializer(typeof(KeySequence[]));
            KeySequence[] keySeqs = (KeySequence[])keysSerializer.Deserialize(reader);

            XmlSerializer entriesSerializer = new XmlSerializer(typeof(List<List<BindEntry>>));
            List<List<BindEntry>> cfgEntries = (List<List<BindEntry>>)entriesSerializer.Deserialize(reader);

            for (int i = 0; i < keySeqs.Length; i++)
            {
                KeySequence keySeq = keySeqs[i];
                List<BindEntry> bindEntries = cfgEntries[i];
                this.entries.Add(keySeq, bindEntries);
            }

            XmlSerializer defaultSerializer = new XmlSerializer(typeof(List<Entry>));
            this.defaultEntries = (List<Entry>)defaultSerializer.Deserialize(reader);
        }

        public void WriteXml(XmlWriter writer)
        {
            XmlSerializer keysSerializer = new XmlSerializer(typeof(KeySequence[]));
            keysSerializer.Serialize(writer, this.entries.Keys.ToArray());

            XmlSerializer entriesSerializer = new XmlSerializer(typeof(List<List<BindEntry>>));
            entriesSerializer.Serialize(writer, this.entries.Values.ToList());

            XmlSerializer defaultSerializer = new XmlSerializer(typeof(List<Entry>));
            defaultSerializer.Serialize(writer, this.defaultEntries);
        }


        internal class DynamicCmd: Executable
        {
            public Executable Cmd { get; set; } = null;

            public DynamicCmd(Executable startInstance)
            {
                this.Cmd = startInstance;
            }

            public override string ToString() => this.Cmd.ToString();
        }

        class Dependency
        {
            public string Name { get; set; } = string.Empty;
            public CommandCollection Commands { get; set; }
        }
    }
}
