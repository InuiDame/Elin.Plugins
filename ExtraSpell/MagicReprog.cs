using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace ExtraSpell;

[BepInPlugin(ModInfos.ModInfo.Guid, ModInfos.ModInfo.Name, ModInfos.ModInfo.Version)]
internal class MagicReprog : BaseUnityPlugin
{
    private static Harmony _harmony = new Harmony(ModInfos.ModInfo.Guid);
    public static ConfigEntry<int> Refresh;
    public static ConfigEntry<float> MPcost, SPower, Sdistance, SBookValue;

    private void Start()
    {
        Refresh = Config.Bind("Shop", "Refresh_Num", 1, "刷新数量");
        EFix();
        _harmony.PatchAll();
    }

    private void EFix()
    {
        // 修补文件变量
        SPower = Config.Bind("Spell", "Spell_Power_Ratio", 1f, "魔法强度倍率");
        Sdistance = Config.Bind("Spell", "Spell_Distance_Ratio", 1f, "魔法范围倍率");
        MPcost = Config.Bind("Spell", "MP_cost_Ratio", 1f, "魔力消耗倍率");
        SBookValue = Config.Bind("Shop", "Spell_Book_Value_Ratio", 1f, "魔法价格倍率");
        SpellTools.FixMagic();
    }
}