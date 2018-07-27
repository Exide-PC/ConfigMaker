using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Csgo.Config.Enums
{
    /// <summary>
    /// Во избежание лишнего кода по обслуживанию перевода примем, что название любого элемента из данного перечисления является ключом локализации в ресурсах
    /// </summary>
    public enum ConfigEntry
    {
        // static
        Forward,
        Back,
        Moveleft,
        Moveright,
        Jump,
        Duck,
        Speed,
        Use,
        ToggleInventoryDisplay,
        Fire,
        SecondaryFire,
        SelectPreviousWeapon,
        Reload,
        SelectNextWeapon,
        LastWeaponUsed,
        DropWeapon,
        InspectWeapon,
        GraffitiMenu,
        CommandRadio,
        StandartRadio,
        ReportRadio,
        TeamMessage,
        ChatMessage,
        Microphone,
        BuyMenu,
        AutoBuy,
        Rebuy,
        Scoreboard,
        PrimaryWeapon,
        SecondaryWeapon,
        Knife,
        CycleGrenades,
        Bomb,
        HEGrenade,
        Flashbang,
        Smokegrenade,
        DecoyGrenade,
        Molotov,
        Zeus,
        CallVote,
        ChooseTeam,
        ShowTeamEquipment,
        ToggleConsole,
        God,
        Noclip,
        Impulse101,
        Cleardecals,
        Clear,

        // dynamic
        BuyScenario,
        ExtraAlias,
        ExtraAliasSet,
        ExecCustomCmds,

        // mouse
        sensitivity,
        zoom_sensitivity_ratio_mouse,
        m_rawinput,
        m_customaccel,
        m_customaccel_exponent,

        // cl
        cl_autowepswitch,
        cl_bob_lower_amt,
        cl_bobamt_lat,
        cl_bobamt_vert,
        cl_bobcycle,
        cl_clanid,
        cl_color,
        cl_dm_buyrandomweapons,
        cl_hud_color,
        cl_hud_healthammo_style,
        cl_hud_radar_scale,
        cl_hud_background_alpha,
        cl_hud_playercount_pos,
        cl_hud_playercount_showcount,
        cl_loadout_colorweaponnames,
        cl_mute_enemy_team,
        cl_pdump,
        cl_radar_always_centered,
        cl_radar_icon_scale_min,
        cl_radar_scale,
        cl_righthand,
        cl_showfps,
        cl_show_clan_in_death_notice,
        cl_showpos,
        cl_teammate_colors_show,
        cl_teamid_overhead_always,
        cl_teamid_overhead_name_alpha,
        cl_teamid_overhead_name_fadetime,
        cl_timeout,
        cl_use_opens_buy_menu,
        cl_viewmodel_shift_left_amt,
        cl_viewmodel_shift_right_amt,
        cl_draw_only_deathnotices,

        // crosshair
        cl_crosshair_drawoutline,
        cl_crosshair_dynamic_maxdist_splitratio,
        cl_crosshair_dynamic_splitalpha_innermod,
        cl_crosshair_dynamic_splitalpha_outermod,
        cl_crosshair_dynamic_splitdist,
        cl_crosshair_outlinethickness,
        cl_crosshair_sniper_width,
        cl_crosshairalpha,
        cl_crosshairdot,
        cl_crosshairgap,
        cl_crosshair_t,
        cl_crosshairgap_useweaponvalue,
        cl_crosshairscale,
        cl_crosshairsize,
        cl_crosshairstyle,
        cl_crosshairthickness,
        cl_crosshairusealpha,
        cl_fixedcrosshairgap,

        // net
        net_graph,
        net_graphheight,
        net_graphpos,
        net_graphproportionalfont,

        // text args
        say,
        say_team,
        connect,
        
        // utils
        Jumpthrow,
        DisplayDamageOn,
        DisplayDamageOff,
        CycleCrosshair,
        VolumeRegulator
    }
}
