using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Configuration;
using Cwl.API.Attributes;
using Cwl.API.Custom;
using Cwl.Helper.FileUtil;
using Cwl.Helper.Unity;
using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;
using Cwl.LangMod;
using DG.Tweening.Plugins.Core;
using GBF.Modinfo;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;


namespace GBF.Modinfo
{
    [BepInPlugin("com.cygame.gbf.info", "GBFPCRweapon", "1.0.0")]
    public class GBF_and_PCR_Equipment : BaseUnityPlugin
    {
        public static ConfigEntry<bool>? allowInvokes;    // 允许施法技能生成 / Allow spell skill generation / 呪文スキル生成を許可
        public static ConfigEntry<bool>? allowAbsorbs;    // 允许吸收附魔 / Allow absorption enchants / 吸収エンチャントを許可
        public static ConfigEntry<bool>? allowVital;      // 允许生命/法力/精力附魔 / Allow life/mana/stamina enchants / 生命/マナ/スタミナエンチャントを許可
        public static ConfigEntry<bool>? allowDefence;    // 允许防御附魔 / Allow defense enchants / 防御エンチャントを許可
        public static ConfigEntry<bool>? allowOffence;    // 允许攻击附魔 / Allow offense enchants / 攻撃エンチャントを許可
        public static ConfigEntry<bool>? globalExp;       // 全局经验共享 / Global experience sharing / グローバル経験値共有
        public static ConfigEntry<int>? maxEnchRoll;      // 最大附魔抽取数量 / Maximum enchant roll count / 最大エンチャント抽選数
        public static ConfigEntry<int>? elementRarity1;   // 元素稀有度 / Element rarity / エレメントレア度

        private void Start()
        {
            // 施法技能生成配置 / Spell skill generation configuration / 呪文スキル生成設定
            allowInvokes = ((BaseUnityPlugin)this).Config.Bind<bool>("GBFlimitbeark", "allowInvokes", false, "Allows spellcasting skills to be generated on weapons,\n" +
    "but most mods’ spellcasting skills can cause issues with weapons,\n" +
    "so it is not recommended to enable.\n" +
    "All spellcasting skills in this mod have been tag-filtered to prevent such problems.\n" +
    "允许施法技能生成在武器上，\n" +
    "但大多数mod的施法技能可能会导致武器出问题，\n" +
    "所以不建议打开，\n" +
    "本mod的施法技能均做了tag屏蔽处理");
            
            // 吸收附魔配置 / Absorption enchant configuration / 吸収エンチャント設定
            allowAbsorbs = ((BaseUnityPlugin)this).Config.Bind<bool>("GBFlimitbeark", "allowAbsorbs", false, "Allow abosrb life/mana/stamina enchants spawn in selector/允许吸血吸蓝吸精力");
            
            // 生命属性附魔配置 / Vital attribute enchant configuration / 生命属性エンチャント設定
            allowVital = ((BaseUnityPlugin)this).Config.Bind<bool>("GBFlimitbeark", "allowVital", false, "Allow life/mana/stamina score enchants spawn in selector/允许血条蓝条精力条附魔");
            
            // 防御附魔配置 / Defense enchant configuration / 防御エンチャント設定
            allowDefence = ((BaseUnityPlugin)this).Config.Bind<bool>("GBFlimitbeark", "allowDefence", false, "Allow DV/PV/FPV enchants spawn in selector/允许DV，PV，致命伤防御附魔");
            
            // 攻击附魔配置 / Offense enchant configuration / 攻撃エンチャント設定
            allowOffence = ((BaseUnityPlugin)this).Config.Bind<bool>("GBFlimitbeark", "allowOffence", true, "Allow critical/vorpal/penetration/Perfect Evasion enchants spawn in selector/允许暴击，完全贯穿/贯穿，完美闪避");
            
            // 全局经验配置 / Global experience configuration / グローバル経験値設定
            globalExp = ((BaseUnityPlugin)this).Config.Bind<bool>("GBFlimitbeark", "globalExp", true, "Allow all equipped items gain exp from enemy death/允许所有装备获得经验");
            
            // 最大附魔数量配置 / Maximum enchant count configuration / 最大エンチャント数設定
            maxEnchRoll = ((BaseUnityPlugin)this).Config.Bind<int>("GBFlimitbeark", "maxEnchRoll", 5, "Set the maximum number of enchants in selector/随机抽取附魔数量");
            
            // 元素稀有度配置 / Element rarity configuration / エレメントレア度設定
            elementRarity1 = ((BaseUnityPlugin)this).Config.Bind<int>("GBFlimitbeark", "limitbeark_Rarity", 200, "Set the element rarity of weapon, 1 in x chance/掉落带这个词条的概率");
            
            new Harmony("GBFelement").PatchAll();  // 应用Harmony补丁 / Apply Harmony patches / Harmonyパッチを適用
            Logger.LogInfo("GBF元素已激活");  // 记录激活信息 / Log activation info / アクティベーション情報を記録
        }
    }
}











