using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Configuration;
using GBF.Modinfo;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TextCore;
using Object = UnityEngine.Object;

namespace GBF.Patch_DontUse
{
    [HarmonyPatch(typeof(AttackProcess), nameof(AttackProcess.GetRawDamage))]
    static class Patch_GetRawDamage_Katana
    {
        static void Postfix(AttackProcess __instance,
            float dmgMulti,
            bool crit,
            bool maxRoll,
            ref long __result)
        {
            
            var cc = __instance.CC;
            if (cc?.Chara == null)
                return;
            
            Element katana = cc.elements.GetOrCreateElement("weaponKatana");
            if (katana == null || katana.ValueWithoutLink <= 0)
                return;
            
            int lv = cc.LV;
            int mul = Mathf.Min(1 + lv / 5, 2 + lv / 7);
            float fnmul = mul * 0.01f;
            
            __result += (long)Mathf.Round(katana.ValueWithoutLink * fnmul);
        }
    }
}
