using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using BepInEx;
using Cwl.Helper.Unity;
using HarmonyLib;
using UnityEngine;
using Debug = UnityEngine.Debug;



namespace CoolDownsFix
{    

    [HarmonyPatch(typeof(Chara))]
    [HarmonyPatch("TickCooldown")]
    public class TickCooldownPatch
    {
        static bool Prefix(Chara __instance)
        {
            if (__instance._cooldowns == null || __instance._cooldowns.Count == 0)
            {
                return false; // 跳过原始方法
            }

            for (int num = __instance._cooldowns.Count - 1; num >= 0; num--)
            {
                int cooldownValue = __instance._cooldowns[num];
                int remainingTurns = cooldownValue % 1000;

                // 修复逻辑：<= 1 就移除（包括负数）
                if (remainingTurns <= 1)
                {
                    __instance._cooldowns.RemoveAt(num);
                }
                else
                {
                    __instance._cooldowns[num]--;
                }
            }

            if (__instance._cooldowns.Count == 0)
            {
                __instance._cooldowns = null;
            }

            return false; // 完全跳过原始方法
        }
    }

    [HarmonyPatch(typeof(Chara))]
    [HarmonyPatch("GetCooldown")]
    public class GetCooldownPatch
    {
        static void Postfix(Chara __instance, int idEle, ref int __result)
        {
            // 确保不会返回负数冷却
            if (__result < 0)
            {
                __result = 0;
            }
        }
    }

}

namespace CoolDownsFix.info
{


    [BepInPlugin("CoolDownsFix", "冷却修复", "1.0.0")]
    internal class CoolDownsFix : BaseUnityPlugin
    {
        private void Start() {
            Harmony harmony = new Harmony("inui.CoolDownsFix");
            harmony.PatchAll();
        }
    }
}