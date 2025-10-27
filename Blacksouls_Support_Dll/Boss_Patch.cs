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
using BS.magicshop;
using Cwl.LangMod;
using static QuestCraft;
using System.Reflection;

namespace Boss_Patch
{
    [HarmonyPatch(typeof(SourceManager), "Init")]
    internal static class Init_Patch
    //Write text information to LangGame (obsolete syntax)
    {
        [HarmonyPostfix]
        public static void Postfix(SourceManager __instance)
        {
            if (!Init_Patch.initialized)
            {
                LangGame.Row row = new LangGame.Row
                {
                    id = "sign_BSBOSS_1",
                    filter = "",
                    group = "",
                    color = "sign",
                    logColor = "",
                    sound = "",
                    effect = "",
                    text_JP = "頭蓋狩りの獣が現れた！",
                    text = "The skull hunting beast has appeared!"
                };
                string lang = EClass.core.config.lang;
                if (lang != "CN")
                {
                    if (lang == "ZHTW")
                    {
                        EClass.sources.charas.map["BSBOSS_1_1"].name_L = "獵顱之獸";
                        EClass.sources.charas.map["BSBOSS_1_2"].name_L = "狩獵邪龍的沃帕爾";
                        EClass.sources.charas.map["BSBOSS_1_3"].name_L = "狩獵邪龍的沃帕爾 狂暴";
                        row.text_L = "獵顱之獸出現了！";
                    }
                }
                else
                {
                    EClass.sources.charas.map["BSBOSS_1_1"].name_L = "猎颅之兽";
                    EClass.sources.charas.map["BSBOSS_1_2"].name_L = "狩猎邪龙的沃帕尔";
                    EClass.sources.charas.map["BSBOSS_1_3"].name_L = "狩猎邪龙的沃帕尔 狂暴";
                    row.text_L = "猎颅之兽出现了！";
                }
                __instance.langGame.rows.Add(row);
                __instance.langGame.SetRow(row);
                Init_Patch.initialized = true;
            }
        }
        private static bool initialized;
    }
    
    [HarmonyPatch(typeof(Zone), "OnGenerateMap")]
    internal static class OnGenerateMap_Patch
        // Randomly spawns Head-Hunting Beast with 1% chance when any map is generated, and displays spawn message
// 当地图生成时，有1%的概率随机生成兔老师，并显示生成消息
    {
        [HarmonyPostfix]
        public static void Postfix(Zone __instance)
        {
            if (0.01000000298023224 > (double)EClass.rndf(1f))
            {
                EClass._zone.AddCard(CharaGen.Create("BSBOSS_1_1", -1), __instance.GetSpawnPos(SpawnPosition.Random, 10000));
                Msg.Say("sign_BSBOSS_1");
            }
        }
    }
    
    // Boss loot drop transformation handler
// BOSS掉落物转换处理器
    [HarmonyPatch(typeof(Chara), "TryDropBossLoot")]
    internal static class TryDropBossLoot_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(Chara __instance)
        {
            // Skip processing for player faction characters
            // 跳过玩家阵营角色的处理
            if (__instance.IsPCFaction || __instance.IsPCFactionMinion)
                return true;

            // Find nearest valid spawn point or use current position
            // 寻找最近的有效生成点或使用当前位置
            Point spawnPoint = __instance.pos.GetNearestPoint(true, false, false, true) ?? __instance.pos;
        
            // Transform BSBOSS_1_1 into BSBOSS_1_2 upon death
            // 当BSBOSS_1_1死亡时转换为BSBOSS_1_2
            if (__instance.id == "BSBOSS_1_1")
            {
                EMono._zone.AddCard(CharaGen.Create("BSBOSS_1_2", -1), spawnPoint);
            }
        
            return true; // Continue with original method execution // 继续执行原方法
        }
    }

// Second stage boss transformation handler
// 第二阶段BOSS转换处理器
    [HarmonyPatch(typeof(Chara), "TryDropBossLoot")]
    internal static class TryDropBossLoot_Patch2
    {
        [HarmonyPrefix]
        public static bool Prefix(Chara __instance)
        {
            // Skip processing for player faction characters
            // 跳过玩家阵营角色的处理
            if (__instance.IsPCFaction || __instance.IsPCFactionMinion)
                return true;

            // Find nearest valid spawn point or use current position
            // 寻找最近的有效生成点或使用当前位置
            Point spawnPoint = __instance.pos.GetNearestPoint(true, false, false, true) ?? __instance.pos;
        
            // Transform BSBOSS_1_2 into BSBOSS_1_3 upon death
            // 当BSBOSS_1_2死亡时转换为BSBOSS_1_3
            if (__instance.id == "BSBOSS_1_2")
            {
                EMono._zone.AddCard(CharaGen.Create("BSBOSS_1_3", -1), spawnPoint);
            }
        
            return true; // Continue with original method execution // 继续执行原方法
        }
    }
}
