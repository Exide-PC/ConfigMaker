using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Csgo.Localization
{
    class RussianLocale : Locale
    {
        // static
        public override string Forward => "Вперед";
        public override string Back => "Назад";
        public override string Moveleft => "Влево";
        public override string Moveright => "Вправо";
        public override string Jump => "Прыжок";
        public override string Duck => "Пригнуться";
        public override string Speed => "Идти";
        public override string Use => "Использовать";        
        public override string ToggleInventoryDisplay => "Показ инвентаря";
        public override string Fire => "Огонь";
        public override string SecondaryFire => "Альтернативный огонь";
        public override string SelectPreviousWeapon => "Предыдущее оружие";
        public override string Reload => "Перезарядка";
        public override string SelectNextWeapon => "Следующее оружие";
        public override string LastWeaponUsed => "Последнее оружие";
        public override string DropWeapon => "Бросить оружие";
        public override string InspectWeapon => "Осмотреть оружие";
        public override string GraffitiMenu => "Граффити";
        public override string CommandRadio => "Отдать приказ по радио";
        public override string StandartRadio => "Стандартные радиосообщения";
        public override string ReportRadio => "Информационные радиосообщения";
        public override string TeamMessage => "Сообщение команде";
        public override string ChatMessage => "Сообщение всем";
        public override string Microphone => "Голосовая связь";
        public override string BuyMenu => "Меню закупки";
        public override string AutoBuy => "Автозакупка";
        public override string Rebuy => "Купить снова";
        public override string Scoreboard => "Таблица очков";
        public override string PrimaryWeapon => "Основное оружие";
        public override string SecondaryWeapon => "Дополнительное оружие";
        public override string Knife => "Нож";
        public override string CycleGrenades => "Смена гранат";
        public override string Bomb => "Бомба";
        public override string HEGrenade => "Осколочная граната";
        public override string Flashbang => "Световая граната";
        public override string Smokegrenade => "Дымовая граната";
        public override string DecoyGrenade => "Ложная граната";
        public override string Molotov => "Молотов";
        public override string Zeus => "Зевс";
        public override string CallVote => "Голосование";
        public override string ChooseTeam => "Выбрать команду";
        public override string ShowTeamEquipment => "Снаряжение команды";
        public override string ToggleConsole => "Консоль";
        public override string God => "Режим бога";
        public override string Noclip => "Noclip";
        public override string Impulse101 => "Получить деньги";
        public override string Cleardecals => "Убрать следы";
        public override string Clear => "Очистить консоль";

        // dynamic
        public override string BuyScenario => "Закупка оружия";
        public override string ExtraAlias => "Пользовательская команда";
        public override string ExtraAliasSet => "Пользовательские команды";
        public override string ExecCustomCmds => "Выполнить команду";

        // utils
        public override string DisplayDamageOn => "Вкл. показ урона";
        public override string DisplayDamageOff => "Выкл. показ урона";

        // mouse
        public override string sensitivity => "Чувствительность мыши";
        public override string zoom_sensitivity_ratio_mouse => "Коэф. чувствительности в прицеле";
        public override string m_rawinput => "Прямой ввод";
        public override string m_customaccel => "Ускорение мыши";
        public override string m_customaccel_exponent => "Коэффициент ускорения";

        // weapons
        public override string BuyScenarioDescription => "Добавить закупку на выбранную клавишу";
        public override string Pistols => "Пистолеты";
        public override string Pistol1 => "Glock/USP";
        public override string Pistol2 => "Беретты";
        public override string Pistol3 => "P250";
        public override string Pistol4 => "CZ75/Fiveseven/Tec9";
        public override string Pistol5 => "Deagle";
        public override string Heavy => "Тяжёлое";
        public override string Heavy1 => "Nova";
        public override string Heavy2 => "XM1014";
        public override string Heavy3 => "Sawed-Off/Mag7";
        public override string Heavy4 => "M249";
        public override string Heavy5 => "Negev";
        public override string SMGs => "ПП";
        public override string Smg1 => "Mac10/MP9";
        public override string Smg2 => "MP7";
        public override string Smg3 => "UMP45";
        public override string Smg4 => "P90";
        public override string Smg5 => "PP-Bizon";
        public override string Rifles => "Винтовки";
        public override string Rifle1 => "Galil/Famas";
        public override string Rifle2 => "AK47/M4";
        public override string Rifle3 => "SSG08";
        public override string Rifle4 => "SG553/AUG";
        public override string Rifle5 => "AWP";
        public override string Rifle6 => "G3SG1/SCAR";
        public override string Gear => "Снаряжение";
        public override string Gear1 => "Броня";
        public override string Gear2 => "Броня + каска";
        public override string Gear3 => "Zeus";
        public override string Grenades => "Гранаты";
        public override string Grenade1 => "Молотов";
        public override string Grenade2 => "Ложная";
        public override string Grenade3 => "Световая";
        public override string Grenade4 => "Осколочная";
        public override string Grenade5 => "Дымовая";

        // todo later
        public override string Jumpthrow => "Jumpthrow скрипт";
        public override string CycleCrosshair => "Переключение прицелов";

        // ui
        public override string ActionMovement => "Движение";
        public override string ActionEquipment => "Снаряжение";
        public override string ActionCommunication => "Коммуникация";
        public override string CommonActions => "Общие действия";
        public override string ActionWarmup => "Разминка";
        public override string Other => "Прочее";

        public override string On => "Вкл";
        public override string Off => "Выкл";

        public override string ClientCommands => "Команды клиента";
        public override string Default => "Стандартный";
        public override string White => "Белый";
        public override string LightBlue => "Светло-синий";
        public override string Blue => "Синий";
        public override string Purple => "Фиолетовый";
        public override string Pink => "Розовый";
        public override string Red => "Красный";
        public override string Orange => "Оранжевый";
        public override string Yellow => "Желтый";
        public override string Green => "Зеленый";
        public override string Aqua => "Аквамарин";

        public override string Health_Style0 => "Стиль 1";
        public override string Health_Style1 => "Стиль 2";

        public override string Top => "Сверху";
        public override string Bottom => "Снизу";
        public override string ShowAvatars => "Показывать аватары";
        public override string ShowCount => "Показывать число живых";
        public override string Crosshair => "Прицел";

        public override string KeyDown1Format => "Нажатие клавиши {0}";
        public override string KeyUp1Format => "Отжатие клавиши {0}";
        public override string KeyDown2Format => "Нажатие {0} при нажатой {1}";
        public override string KeyUp2Format => "Отжатие {0} при нажатой {1}";
        public override string DefaultCmds => "Команды, вызываемые по умолчанию";
        public override string AliasCmds => "Команды, привязанные к алиасу";


        public override string MouseSettings => "Настройки мыши";
    }
}
