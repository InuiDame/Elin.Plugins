using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Configuration;
using Cwl.API.Custom;
using Cwl.Helper.Unity;
using Cwl.LangMod;
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
        // 注意：参数签名必须和原方法匹配，最后一个 ref int __result 用来改返回值
        static void Postfix(AttackProcess __instance,
            float dmgMulti,
            bool crit,
            bool maxRoll,
            ref long __result)
        {

            // 1) 目标必须存在且是角色
            var tc = __instance.TC;
            if (tc?.Chara == null)
                return;

            // 2) 拿到武士刀元素
            Element katana = tc.elements.GetOrCreateElement("weaponKatana");
            if (katana == null || katana.ValueWithoutLink <= 0)
                return;

            // 3) 计算倍率：Min(1 + LV/5, 2 + LV/7)
            int lv = tc.LV;
            int mul = Mathf.Min(1 + lv / 5, 2 + lv / 7);

            // 4) 把加成叠到 __result 上
            __result += katana.ValueWithoutLink * mul;
        }
    }
}
