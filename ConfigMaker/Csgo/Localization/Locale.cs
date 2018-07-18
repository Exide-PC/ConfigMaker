using ConfigMaker.Csgo.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ConfigMaker.Csgo.Localization
{
    public abstract class Locale
    {
        Dictionary<ConfigEntry, string> cfgEntryDict = new Dictionary<ConfigEntry, string>();
        ////Dictionary<UiEntry, string> uiEntryDict = new Dictionary<UiEntry, string>();

        public string this[ConfigEntry cfgEntry]
        {
            get
            {
                string result;
                if (!cfgEntryDict.TryGetValue(cfgEntry, out result))
                    result = cfgEntry.ToString();
                return result;
            }
        }

        protected Locale()
        {
            InitCfgDictionary();
            //InitUiDictionary();
        }

        void InitCfgDictionary()
        {
            // Заполним словарь с помощью рефлексии
            Type presetEnumType = typeof(ConfigEntry);
            ConfigEntry[] enumValues = (ConfigEntry[])Enum.GetValues(presetEnumType);

            Type localeType = typeof(Locale);
            PropertyInfo[] properties = localeType.GetProperties();

            foreach (ConfigEntry entry in enumValues)
            {
                string name = Enum.GetName(presetEnumType, entry);
                
                PropertyInfo targetProp = properties.FirstOrDefault(p => p.Name == name); // TODO: убрать
                if (targetProp == null) continue;

                //PropertyInfo targetProp = properties.First(p => p.Name == name);
                string propValue = (string)targetProp.GetValue(this);
                cfgEntryDict.Add(entry, propValue);
            }
        }

        // cfg
        public abstract string Forward { get; }
        public abstract string Back { get; }
        public abstract string Moveleft { get; }
        public abstract string Moveright { get; }
        public abstract string Jump { get; }
        public abstract string Duck { get; }
        public abstract string Speed { get; }
        public abstract string Use { get; }

        public abstract string ToggleInventoryDisplay { get; }
        public abstract string Fire { get; }
        public abstract string SecondaryFire { get; }
        public abstract string SelectPreviousWeapon { get; }
        public abstract string Reload { get; }
        public abstract string SelectNextWeapon { get; }
        public abstract string LastWeaponUsed { get; }
        public abstract string DropWeapon { get; }
        public abstract string InspectWeapon { get; }
        public abstract string GraffitiMenu { get; }
        public abstract string CommandRadio { get; }
        public abstract string StandartRadio { get; }
        public abstract string ReportRadio { get; }
        public abstract string TeamMessage { get; }
        public abstract string ChatMessage { get; }
        public abstract string Microphone { get; }
        public abstract string BuyMenu { get; }
        public abstract string AutoBuy { get; }
        public abstract string Rebuy { get; }
        public abstract string Scoreboard { get; }
        public abstract string PrimaryWeapon { get; }
        public abstract string SecondaryWeapon { get; }
        public abstract string Knife { get; }
        public abstract string CycleGrenades { get; }
        public abstract string Bomb { get; }
        public abstract string HEGrenade { get; }
        public abstract string Flashbang { get; }
        public abstract string Smokegrenade { get; }
        public abstract string DecoyGrenade { get; }
        public abstract string Molotov { get; }
        public abstract string Zeus { get; }
        public abstract string CallVote { get; }
        public abstract string ChooseTeam { get; }
        public abstract string ShowTeamEquipment { get; }
        public abstract string ToggleConsole { get; }
        public abstract string God { get; }
        public abstract string Noclip { get; }
        public abstract string Impulse101 { get; }
        public abstract string Cleardecals { get; }
        public abstract string Clear { get; }

        // dynamic
        public abstract string BuyScenario { get; }
        public abstract string ExtraAlias { get; }
        public abstract string ExtraAliasSet { get; }
        public abstract string ExecCustomCmds { get; }

        // utils
        public abstract string DisplayDamageOn { get; }
        public abstract string DisplayDamageOff { get; }

        // mouse
        public abstract string sensitivity { get; }
        public abstract string zoom_sensitivity_ratio_mouse { get; }
        public abstract string m_rawinput { get; }
        public abstract string m_customaccel { get; }
        public abstract string m_customaccel_exponent { get; }

        // weapons
        public abstract string BuyScenarioDescription { get; }
        public abstract string Pistols { get; }
        public abstract string Pistol1 { get; }
        public abstract string Pistol2 { get; }
        public abstract string Pistol3 { get; }
        public abstract string Pistol4 { get; }
        public abstract string Pistol5 { get; }
        public abstract string Heavy { get; }
        public abstract string Heavy1 { get; }
        public abstract string Heavy2 { get; }
        public abstract string Heavy3 { get; }
        public abstract string Heavy4 { get; }
        public abstract string Heavy5 { get; }
        public abstract string SMGs { get; }
        public abstract string Smg1 { get; }
        public abstract string Smg2 { get; }
        public abstract string Smg3 { get; }
        public abstract string Smg4 { get; }
        public abstract string Smg5 { get; }
        public abstract string Rifles { get; }
        public abstract string Rifle1 { get; }
        public abstract string Rifle2 { get; }
        public abstract string Rifle3 { get; }
        public abstract string Rifle4 { get; }
        public abstract string Rifle5 { get; }
        public abstract string Rifle6 { get; }
        public abstract string Gear { get; }
        public abstract string Gear1 { get; }
        public abstract string Gear2 { get; }
        public abstract string Gear3 { get; }
        public abstract string Grenades { get; }
        public abstract string Grenade1 { get; }
        public abstract string Grenade2 { get; }
        public abstract string Grenade3 { get; }
        public abstract string Grenade4 { get; }
        public abstract string Grenade5 { get; }

        public abstract string Jumpthrow { get; }
        public abstract string CycleCrosshair { get; }

        // ui
        public abstract string ActionMovement { get; }
        public abstract string ActionEquipment { get; }
        public abstract string ActionCommunication { get; }
        public abstract string CommonActions { get; }
        public abstract string ActionWarmup { get; }
        public abstract string Other { get; }

        public abstract string On { get; }
        public abstract string Off { get; }

        public abstract string ClientCommands { get; }
        public abstract string Default { get; }
        public abstract string White { get; }
        public abstract string LightBlue { get; }
        public abstract string Blue { get; }
        public abstract string Purple { get; }
        public abstract string Pink { get; }
        public abstract string Red { get; }
        public abstract string Orange { get; }
        public abstract string Yellow { get; }        
        public abstract string Green { get; }
        public abstract string Aqua { get; }

        public abstract string Health_Style0 { get; }
        public abstract string Health_Style1 { get; }

        public abstract string Top { get; }
        public abstract string Bottom { get; }
        public abstract string ShowAvatars { get; }
        public abstract string ShowCount { get; }
        public abstract string Crosshair { get; }

        public abstract string KeyDown1Format { get; }
        public abstract string KeyUp1Format { get; }
        public abstract string KeyDown2Format { get; }
        public abstract string KeyUp2Format { get; }
        public abstract string DefaultCmds { get; }
        public abstract string AliasCmds { get; }

        public abstract string MouseSettings { get; }
    }
}
