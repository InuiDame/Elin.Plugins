using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using UnityEngine;
using Cwl.API.Processors;
using Microsoft.CodeAnalysis;
using HarmonyLib;
using BepInEx;
using BepInEx.Configuration;
using Cwl;
using Cwl.API.Custom;
using Cwl.LangMod;
using static QuestCraft;
using System.Reflection;

namespace BS.magicshop
{
   
    // Magic shop restocking enhancement
// 魔法商店补货增强
[HarmonyPatch]
public class MagicShop
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Trait), "OnBarter")]
    public static void OnBarter(Trait __instance)
    {
        // Only process magic shops during restocking
        // 仅在魔法商店补货时处理
        if (__instance.ShopType != ShopType.Magic || !__instance.owner.isRestocking)
        {
            return;
        }
        
        // Find or create merchant chest container
        // 查找或创建商人箱子容器
        Thing thing = __instance.owner.things.Find("chest_merchant", -1, -1) ?? ThingGen.Create("chest_merchant", -1, -1);
        __instance.owner.AddThing(thing, true, -1, -1);
        
        // Check if chest has enough space
        // 检查箱子是否有足够空间
        if (thing.things.Count + 2 > thing.things.GridSize)
        {
            return;
        }
        
        // Add random spellbooks based on configured refresh count
        // 根据配置的刷新数量添加随机法术书
        System.Random random = new System.Random();
        for (int i = 0; i < BSmagicshop.Refresh.Value; i++)
        {
            int index = random.Next(ModInfos.MagicList.Count);
            object obj = ModInfos.MagicList[index];
            List<string> list = obj as List<string>;
            string alias;
            
            // Handle both single string and list of strings in magic list
            // 处理魔法列表中的单个字符串和字符串列表
            if (list == null)
            {
                string text = obj as string;
                if (text == null)
                {
                    Debug.LogError("No matching item found in list");
                    // 列表里无匹配项
                    return;
                }
                alias = text;
            }
            else
            {
                alias = list[random.Next(list.Count)];
            }
            
            // Create identified spellbook and add to chest
            // 创建已鉴定的法术书并添加到箱子
            thing.AddThing(ThingGen.CreateSpellbook(alias, 1).Identify(true, IDTSource.Identify), true, -1, -1);
        }
    }
}
    internal static class ModInfos
    {
        internal static readonly List<object> MagicList = new List<object>(5)
        {
            "BS_Fire",
            "BS_Lightning",
            "BS_Darkness"
        };


        internal static class ModInfo
        {
            internal const string Guid = "BSmagic";
            internal const string Name = "黑魂法术";
            internal const string Version = "1.0.0";
        }
    }
    
    [BepInPlugin("BSmagicshop", "黑魂法术", "1.0.0")]
    internal class BSmagicshop : BaseUnityPlugin
    {
        private void Start()
        {
            BSmagicshop.Refresh = base.Config.Bind<int>("Shop", "Refresh_Num", 1, "刷新数量");
            BSmagicshop._harmony.PatchAll();
        }
        
        private static Harmony _harmony = new Harmony("BSmagicshop");
        public static ConfigEntry<int> Refresh;
        public static ConfigEntry<float> MPcost;
        public static ConfigEntry<float> SPower;
        public static ConfigEntry<float> Sdistance;
        public static ConfigEntry<float> SBookValue;
    }
    
}
