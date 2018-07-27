using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Csgo.Config.Enums
{
    public enum EntryType
    {
        /// <summary>
        /// Элементы с одинаковым ключом имеют разные аргументы и зависимости
        /// </summary>
        Dynamic,
        /// <summary>
        /// Элементы с одинаковыми ключами не имеют аргументов и имеют одинаковые зависимости
        /// </summary>
        Static,
        /// <summary>
        /// Элементы с одинаковыми ключами должны иметь одинаковые аргументы и зависимости
        /// </summary>
        Semistatic
    }
}
