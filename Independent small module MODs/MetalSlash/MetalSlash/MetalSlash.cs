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



namespace MetalSlash.spell
{

    [HarmonyPatch(typeof(Card))]
    public class CardDamageCalculationPatch
    {
        [HarmonyPatch("DamageHP", new Type[] {
    typeof(long),
    typeof(int),
    typeof(int),
    typeof(AttackSource),
    typeof(Card),
    typeof(bool),
    typeof(Thing),
    typeof(Chara)
})]
        [HarmonyPrefix]
        public static bool OverrideDamageCalculation(Card __instance,
            ref long dmg,
            int ele,
            int eleP,
            AttackSource attackSource,
            Card origin,
            bool showEffect,
            Thing weapon,
            Chara originalTarget)
        {
            try
            {
                // 检查无视1218条件
                if (origin != null && origin.HasElement(999902) && __instance.HasElement(1218))
                {
                    UnityEngine.Debug.Log($"[MOD] 使用完全覆盖模式处理伤害");

                    // 完全重写伤害计算流程
                    long finalDamage = CalculateFinalDamage(__instance, dmg, ele, eleP, attackSource, origin, weapon, originalTarget);

                    // 调用基础伤害应用方法（不包含1218减伤）
                    ApplyDamageWithout1218(__instance, finalDamage, ele, eleP, attackSource, origin, showEffect, weapon, originalTarget);

                    return false; // 跳过原始方法
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[MOD] 完全覆盖错误: {ex}");
            }

            return true; // 正常执行原始方法
        }

        private static long CalculateFinalDamage(Card target, long baseDamage, int ele, int eleP,
            AttackSource attackSource, Card origin, Thing weapon, Chara originalTarget)
        {
            long damage = baseDamage;

            // 应用除了1218之外的所有伤害计算逻辑

            // 1. 应用攻击者加成
            if (origin != null && origin.isChara && origin.Chara.HasCondition<ConBerserk>())
            {
                damage = damage * 3 / 2;
            }

            // 2. 应用元素抗性（除了1218）
            if (ele != 0 && ele != 926)
            {
                Element e = Element.Create(ele);
                if (!e.source.aliasRef.IsEmpty() && attackSource != AttackSource.ManaBackfire)
                {
                    int resistBonus = origin?.Evalue(1238) ?? 0;
                    if (attackSource == AttackSource.MagicSword)
                    {
                        resistBonus += 2;
                        if (origin.HasElement(1247))
                        {
                            resistBonus++;
                        }
                    }
                    // 跳过1218检查，直接应用其他抗性
                    damage = Element.GetResistDamage((int)damage, target.Evalue(e.source.aliasRef), resistBonus);
                }
            }

            // 3. 跳过1218减伤部分
            // 原始代码: damage = damage * (1000 - Mathf.Min(target.Evalue(1218), 1000)) / 1000;
            // 我们直接跳过这一行

            UnityEngine.Debug.Log($"[MOD] 最终伤害计算: {baseDamage} -> {damage}");
            return damage;
        }

        private static void ApplyDamageWithout1218(Card target, long damage, int ele, int eleP,
            AttackSource attackSource, Card origin, bool showEffect, Thing weapon, Chara originalTarget)
        {
            try
            {
                // 直接调用基础的HP减少逻辑
                int originalHP = target.hp;
                target.hp -= (int)damage;

                UnityEngine.Debug.Log($"[MOD] 应用伤害: {damage}, HP: {originalHP} -> {target.hp}");

                // 触发伤害效果
                if (target.isSynced && damage != 0L)
                {
                    float ratio = (float)damage / (float)target.MaxHP;
                    Card effectTarget = (target.parent is Chara) ? (target.parent as Chara) : target;

                    // 播放伤害效果
                    if (showEffect && attackSource != AttackSource.Condition)
                    {
                        effectTarget.PlayEffect("blood").SetParticleColor(EClass.Colors.matColors[target.material.alias].main).Emit(20 + (int)(30f * ratio));
                    }

                    // 显示伤害数字
                    if (EClass.core.config.test.showNumbers || target.isThing)
                    {
                        Element e = (ele == 0 || ele == 926) ? Element.Void : Element.Create(ele);
                        EClass.scene.damageTextRenderer.Add(target, effectTarget, (int)damage, e);
                    }
                }

                // 处理死亡逻辑
                if (target.hp < 0 && target.hp != originalHP)
                {
                    target.Die(Element.Create(ele), origin, attackSource, originalTarget);
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[MOD] 应用伤害错误: {ex}");
            }
        }
    }


}

namespace MetalSlash.info
{


    [BepInPlugin("MetalSlash", "金属破裂者模块", "1.0.0")]
    internal class MetalSlash : BaseUnityPlugin
    {
        private void Start() {
            Harmony harmony = new Harmony("inui.MetalSlash");
            harmony.PatchAll();
            //Logger.LogInfo("金属破裂者模块已加载");
        }
    }
}