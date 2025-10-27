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
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Condition_Normal;

namespace Condition_Patch_HealHPBoost
{
    [HarmonyPatch(typeof(Card), nameof(Card.HealHPHost))]
    static class Patch_HealHPHost_InjectBoost
    {
        // Prefix在原始方法之前运行；如果有buff则调整'amount' / Prefix runs before original method; adjust 'amount' if buff present / Prefixは元のメソッドの前に実行；バフがある場合は'amount'を調整
        static void Prefix(Chara __instance, int a, HealSource origin)
        {
            // 查询是否有我们的自定义buff / Check if our custom buff is present / カスタムバフが存在するか確認
            var buff = __instance.GetCondition<Connatsu_sk1>();
            if (buff != null)
            {
                // 增加额外的治疗量 / Add extra healing amount / 追加治療量を加算
                // 假设 BonusAmount 是基础治疗量，再乘上 DEX 带来的增益 / Assuming BonusAmount is base heal, multiplied by DEX bonus / BonusAmountが基本治療量で、DEXによるボーナスを乗算
                float factor = MagBoost(__instance.MAG);
                a = Mathf.RoundToInt(a * (1 + factor));
            }
        }
        
        // 基于MAG属性的治疗加成计算 / Healing bonus calculation based on MAG attribute / MAG属性に基づく治療ボーナス計算
        public static float MagBoost(int mag)
        {
            // 先算出线性部分：每 400 MAG => +0.02 / First calculate linear part: every 400 MAG => +0.02 / まず線形部分を計算：400 MAGごとに+0.02
            float boost = (mag / 400f) * 0.02f;

            // 强制下限 0.05 (5% 最小加成) / Force minimum 0.05 (5% minimum bonus) / 最小値0.05を強制 (5% 最小ボーナス)
            boost = Mathf.Max(boost, 0.05f);

            // 强制上限 2 (200% 最大加成) / Force maximum 2 (200% maximum bonus) / 最大値2を強制 (200% 最大ボーナス)
            boost = Mathf.Min(boost, 2f);

            return boost;
        }
    }
}
