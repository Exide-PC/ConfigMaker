using ConfigMaker.Csgo.Commands;
using ConfigMaker.Csgo.Config.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ConfigMaker.Csgo.Config.Entries
{
    [XmlInclude(typeof(ParametrizedBindEntry<string[]>))]
    [XmlInclude(typeof(ParametrizedBindEntry<string>))]
    [XmlInclude(typeof(ParametrizedBindEntry<int[]>))]
    [XmlInclude(typeof(ParametrizedBindEntry<int>))]
    [XmlInclude(typeof(ParametrizedBindEntry<double[]>))]
    [XmlInclude(typeof(ParametrizedBindEntry<double>))]
    public class BindEntry: IEntry
    {
        public ConfigEntry PrimaryKey { get; set; }

        public Executable OnKeyDown { get; set; } = null;
        public Executable OnKeyRelease { get; set; } = null;

        public bool IsMetaScript { get; set; }
        public EntryType Type { get; set; } = EntryType.Static;
        
        Executable IEntry.Cmd
        {
            get => OnKeyDown != null ? OnKeyDown : OnKeyRelease;
            set { } // OnKeyDown/OnKeyRelease задается отдельно 
        }

        // По умолчанию будет пустая коллекция зависимостей
        public CommandCollection Dependencies { get; set; } = new CommandCollection();

        void UpdateIsMetaScript()
        {
            if (this.OnKeyDown == null || this.OnKeyRelease == null)
            {
                this.IsMetaScript = false;
                return;
            }                

            string onKeyDownCmd = this.OnKeyDown.ToString();
            string onKeyReleaseCmd = this.OnKeyRelease.ToString();

            if (onKeyDownCmd[0] != '+' || onKeyReleaseCmd[0] != '-')
            {
                this.IsMetaScript = false;
                return;
            }

            if (onKeyDownCmd.Substring(1) != onKeyReleaseCmd.Substring(1))
            {
                this.IsMetaScript = false;
                return;
            }

            this.IsMetaScript = true;
        }

        public BindEntry() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="onKeyDown">Команда, выполняемая при нажатии кнопки. Может быть имя алиаса, а так же его полное объявление. null означает отсутствие действий</param>
        /// <param name="onKeyRelease">Команда, выполняемая при отжатии кнопки. Может быть имя алиаса, а так же его полное объявление. null означает отсутствие действий</param>
        public BindEntry(ConfigEntry primaryKey, Executable onKeyDown, Executable onKeyRelease)
        {
            if (onKeyDown == null && onKeyRelease == null)
                throw new InvalidOperationException("onKeyDown == null && onKeyRelease == null");

            this.PrimaryKey = primaryKey;
            this.OnKeyDown = onKeyDown;
            this.OnKeyRelease = onKeyRelease;
            this.UpdateIsMetaScript();

            // Не для мета-скрипта допустима привязка лишь к 1 действию
            if (!this.IsMetaScript)
            {
                if (this.OnKeyDown != null && this.OnKeyRelease != null)
                    throw new InvalidOperationException("Для не мета-скриптов допустима привязка только к 1 действию");
            }
        }

        public BindEntry(ConfigEntry primaryKey, Executable onKeyDown, Executable onKeyRelease, CommandCollection dependencies):
            this(primaryKey, onKeyDown, onKeyRelease)
        {
            foreach (Executable cmd in dependencies)
            {
                this.Dependencies.Add(cmd);
            }
        }

        public static explicit operator BindEntry(Entry entry)
        {
            Type sourceType = entry.GetType();

            IEnumerable<PropertyInfo> propsToCopy = null;
            object targetInstance = null;

            if (sourceType.IsGenericType)
            {
                // Конечный тип - ParametrizedEntry<T>. Приводим к ParametrizedBindEntry<T>
                // Нам необходимо настроить тип ParametrizedBindEntry<T>
                Type nongenericTargetType = typeof(ParametrizedBindEntry<>);
                // Узнаем тип T и зададим его как обобщенный аргумент в новом типе
                Type genericArgType = sourceType.GenericTypeArguments[0];
                Type targetGenericType = nongenericTargetType.MakeGenericType(genericArgType);

                // Узнаем имя нужного параметра
                //string argPropertyName = nameof(IParametrizedEntry<int>.Arg);
                //object genericValue = sourceType.GetProperty(argPropertyName).GetValue(entry);

                // Получим тип, описывающий обобщенный интерфейс
                Type iParametrizedEntryType = typeof(IParametrizedEntry<>).MakeGenericType(genericArgType);
                // Экземпляр ParametrizedBindEntry<T>
                targetInstance = Activator.CreateInstance(targetGenericType);

                // Получим все реализуемые интерфейсы
                propsToCopy = iParametrizedEntryType.GetInterfaces()
                    .SelectMany(i => i.GetProperties()) // Из каждого выделим свойства
                    .Concat(iParametrizedEntryType.GetProperties()); // И объединим со свойстами текущего
            }
            else
            {
                // Конечный тип - Entry. Необходимо привести к BindEntry
                propsToCopy = typeof(IEntry).GetProperties();
                // Экземпляр BindEntry
                targetInstance = Activator.CreateInstance(typeof(BindEntry));
            }

            // Cкопируем все значения свойств из ParametrizedEntry<T> в ParametrizedBindEntry<T>
            foreach (var prop in propsToCopy)
            {
                object sourceValue = prop.GetValue(entry);
                prop.SetValue(targetInstance, sourceValue);
            }

            BindEntry bindEntry = (BindEntry)targetInstance;
            return bindEntry;
        }
    }
}
