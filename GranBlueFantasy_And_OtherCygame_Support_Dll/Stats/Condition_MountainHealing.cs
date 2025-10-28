using System;
using System.Collections;
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
using static NoticeManager;
using static UnityEngine.UI.GridLayoutGroup;

namespace Condition_MountainHealing
{
    public class ConGBFEarth4 : Timebuff
    //Mountain's Healing / 大地の治癒 / 大地的治愈
    {
        public override int GetPhase() => 0;  // 固定阶段0 / Fixed phase 0 / 固定フェーズ0
    }
    
    [HarmonyPatch(typeof(Card), nameof(Card.HealHPHost))]
    static class Patch_HealHPHost_InjectBoost2
    {
        static void Prefix(Chara __instance, int a, HealSource origin)
        {
            // 检查是否存在大地治愈效果 / Check if Earth Healing effect exists / 大地の治癒効果が存在するか確認
            var buff = __instance.GetCondition<ConGBFEarth4>();
            if (buff != null)
            {
                // 基于END属性计算治疗加成 / Calculate healing bonus based on END attribute / END属性に基づいて治療ボーナスを計算
                float factor = EndBoost(__instance.END);
                a = Mathf.RoundToInt(a * (1 + factor));
            }
        }
        // END属性治疗加成计算 / END attribute healing bonus calculation / END属性治療ボーナス計算
        public static float EndBoost(int end)
        {
            // 基础加成计算：每400 END提供2%加成 / Base bonus calculation: 2% per 400 END / 基本ボーナス計算：400 ENDごとに2%ボーナス
            float boost = (end / 400f) * 0.02f;

            boost = Mathf.Max(boost, 0.05f);  // 最小加成5% / Minimum bonus 5% / 最小ボーナス5%

            boost = Mathf.Min(boost, 2f);     // 最大加成200% / Maximum bonus 200% / 最大ボーナス200%

            return boost;
        }
    }
}
