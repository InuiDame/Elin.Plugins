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
using Condition_Ailes_Enlacantes;
using Condition_Pupper_Squad_Go;

namespace Patch_Shield_Condition
{
    [HarmonyPatch(typeof(Card))]
    internal class ShieldPatch : EClass
    {
        [HarmonyPatch("DamageHP", new Type[]
        {
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
        internal static bool HandleShieldDamage(Card __instance, ref long dmg, AttackSource attackSource)
        {
            // 检查目标是否为角色 / Check if target is a character / ターゲットがキャラクターか確認
            if (__instance.isChara)
            {
                Chara targetChar = __instance.Chara;
                // 定义可被护盾抵挡的攻击来源 / Define attack sources that can be blocked by shield / シールドでブロック可能な攻撃ソースを定義
                List<AttackSource> damageNegationAttackSources = new List<AttackSource>
                {
                    AttackSource.Melee,        // 近战攻击 / Melee attack / 近接攻撃
                    AttackSource.Range,        // 远程攻击 / Range attack / 遠距離攻撃
                    AttackSource.Throw,        // 投掷攻击 / Throw attack / 投擲攻撃
                    AttackSource.MagicSword,   // 魔法剑攻击 / Magic sword attack / 魔法剣攻撃
                    AttackSource.Shockwave,    // 冲击波攻击 / Shockwave attack / 衝撃波攻撃
                    AttackSource.None          // 无来源攻击 / No source attack / ソースなし攻撃
                };
                
                // 检查攻击来源是否可被护盾抵挡 / Check if attack source can be blocked by shield / 攻撃ソースがシールドでブロック可能か確認
                if (damageNegationAttackSources.Contains(attackSource))
                {
                    // 处理 ConSK2699 护盾（交缠双翼） / Handle ConSK2699 shield (Ailes Enlacantes) / ConSK2699シールド（エール・ド・レトラント）を処理
                    if (targetChar.HasCondition<ConSK2699>())
                    {
                        ConSK2699 shield = targetChar.GetCondition<ConSK2699>();
                        // 护盾值足够完全吸收伤害 / Shield value sufficient to fully absorb damage / シールド値がダメージを完全吸収可能
                        if (shield.value >= dmg)
                        {
                            int num2 = (int)dmg;
                            shield.Mod(-1 * num2);  // 减少护盾值 / Reduce shield value / シールド値を減少
                            return false; // 完全吸收伤害，阻止原方法执行 / Fully absorb damage, prevent original method execution / ダメージを完全吸収、元のメソッド実行を阻止
                        }
                        // 护盾值不足以完全吸收伤害 / Shield value insufficient to fully absorb damage / シールド値がダメージを完全吸収不可能
                        dmg -= shield.value;  // 减少伤害值 / Reduce damage value / ダメージ値を減少
                        shield.Kill();        // 移除护盾效果 / Remove shield effect / シールド効果を削除
                    }
                    
                    // 处理 ConSK1374 护盾 / Handle ConSK1374 shield / ConSK1374シールドを処理
                    if (targetChar.HasCondition<ConSK1374>())
                    {
                        ConSK1374 shield = targetChar.GetCondition<ConSK1374>();
                        // 护盾值足够完全吸收伤害 / Shield value sufficient to fully absorb damage / シールド値がダメージを完全吸収可能
                        if (shield.value >= dmg)
                        {
                            int num2 = (int)dmg;
                            shield.Mod(-1 * num2);  // 减少护盾值 / Reduce shield value / シールド値を減少
                            return false; // 完全吸收伤害，阻止原方法执行 / Fully absorb damage, prevent original method execution / ダメージを完全吸収、元のメソッド実行を阻止
                        }
                        // 护盾值不足以完全吸收伤害 / Shield value insufficient to fully absorb damage / シールド値がダメージを完全吸収不可能
                        dmg -= shield.value;  // 减少伤害值 / Reduce damage value / ダメージ値を減少
                        shield.Kill();        // 移除护盾效果 / Remove shield effect / シールド効果を削除
                    }
                }
            }
            return true;  // 继续执行原伤害方法 / Continue with original damage method / 元のダメージメソッドを継続実行
        }
    }
}
