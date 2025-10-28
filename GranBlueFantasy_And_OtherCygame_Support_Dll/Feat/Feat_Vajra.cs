using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace GBF.feat.Feat_Vajra
{
    internal class featVajra : Feat
    //Queen of Canines / 犬神宮の主 / 犬神宫之主
    {
        internal void _OnApply(int add, ElementContainer eleOwner, bool hint)
        {
            owner.ModBase(466, add * 15);  // 修改元素466基础值 / Modify base value of element 466 / エレメント466の基本値を修正
            // 修改属性，设置潜力，应用效果等 / Modify attributes, set potential, apply effects, etc. / 属性を修正、潜在力を設定、効果を適用など
        }
        public override Sprite GetIcon(string suffix = "")
        {
            return SpriteSheet.Get(source.alias);  // 获取专长图标 / Get feat icon / 特技アイコンを取得
        }
    }

    internal class featVajra2 : Feat
    //狗狗与糖果 / ドッグ・アンド・トリート / Scritches and Treats
    {
        internal void _OnApply(int add, ElementContainer eleOwner, bool hint)
        {
            owner.ModBase(651, 100);    // 修改元素651基础值 / Modify base value of element 651 / エレメント651の基本値を修正
            owner.ModBase(67, add * 30); // 修改元素67基础值 / Modify base value of element 67 / エレメント67の基本値を修正
            // 修改属性，设置潜力，应用效果等 / Modify attributes, set potential, apply effects, etc. / 属性を修正、潜在力を設定、効果を適用など
        }
        public override Sprite GetIcon(string suffix = "")
        {
            return SpriteSheet.Get(source.alias);  // 获取专长图标 / Get feat icon / 特技アイコンを取得
        }
    }

    //免疫DEbuff / Immune to Debuffs / デバフ免疫
    [HarmonyPatch(typeof(Chara))]
    [HarmonyPatch("AddCondition", typeof(Condition), typeof(bool))]
    public static class CharaAddConditionPatch
    {
        private static readonly HashSet<int> ImmuneElements = new()  // 免疫元素ID集合 / Immune element ID collection / 免疫エレメントIDコレクション
    {
        170031, // 原本的免疫元素 / Original immune element / 元の免疫エレメント
        170038
        // 继续往这里添加新的元素ID即可 / Continue adding new element IDs here / ここに新しいエレメントIDを追加し続ける
    };
        public static bool Prefix(Chara __instance, Condition c, bool force, ref Condition __result)
        {
            // 检查是否是负面状态 / Check if negative status / ネガティブ状態かチェック
            if (c != null && (c.Type == ConditionType.Bad || c.Type == ConditionType.Debuff || c.Type == ConditionType.Disease || c.Type == ConditionType.Wrath))
            {
                foreach (var eleId in ImmuneElements)
                {
                    if (__instance.HasElement(eleId))  // 检查是否拥有免疫元素 / Check if has immune element / 免疫エレメントを持っているかチェック
                    {
                        // 可选：输出日志方便调试 / Optional: output log for debugging / オプション：デバッグ用にログ出力
                        // UnityEngine.Debug.Log($"角色 {__instance.id} 免疫了 {c.id} ({c.Type})，原因: 元素 {eleId}");

                        __result = null; // 阻止添加状态 / Prevent adding status / 状態追加を阻止
                        return false;    // 跳过原方法 / Skip original method / 元のメソッドをスキップ
                    }
                }
            }

            return true; // 继续执行原方法 / Continue with original method / 元のメソッドを継続実行
        }
    }

    //金属特攻 / Metal Special Attack / 金属特攻
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
                // 检查无视1218条件 / Check ignore 1218 condition / 1218無視条件をチェック
                if (origin != null && origin.HasElement(170031) && __instance.HasElement(1218))
                {
                    UnityEngine.Debug.Log($"[MOD] 使用完全覆盖模式处理伤害");  // 完全覆盖模式日志 / Full override mode log / 完全上書きモードログ

                    // 完全重写伤害计算流程 / Completely rewrite damage calculation process / 完全にダメージ計算プロセスを書き換え
                    long finalDamage = CalculateFinalDamage(__instance, dmg, ele, eleP, attackSource, origin, weapon, originalTarget);

                    // 调用基础伤害应用方法（不包含1218减伤） / Call basic damage application method (excluding 1218 damage reduction) / 基本ダメージ適用メソッドを呼び出し（1218ダメージ軽減を含まない）
                    ApplyDamageWithout1218(__instance, finalDamage, ele, eleP, attackSource, origin, showEffect, weapon, originalTarget);

                    return false; // 跳过原始方法 / Skip original method / 元のメソッドをスキップ
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[MOD] 完全覆盖错误: {ex}");  // 完全覆盖错误日志 / Full override error log / 完全上書きエラーログ
            }

            return true; // 正常执行原始方法 / Normally execute original method / 正常に元のメソッドを実行
        }

        // 计算最终伤害 / Calculate final damage / 最終ダメージを計算
        private static long CalculateFinalDamage(Card target, long baseDamage, int ele, int eleP,
            AttackSource attackSource, Card origin, Thing weapon, Chara originalTarget)
        {
            long damage = baseDamage;

            // 应用除了1218之外的所有伤害计算逻辑 / Apply all damage calculation logic except 1218 / 1218以外の全てのダメージ計算ロジックを適用

            // 1. 应用攻击者加成 / Apply attacker bonus / 攻撃者ボーナスを適用
            if (origin != null && origin.isChara && origin.Chara.HasCondition<ConBerserk>())
            {
                damage = damage * 3 / 2;  // 狂暴状态50%加成 / Berserk state 50% bonus / 狂暴状態50%ボーナス
            }

            // 2. 应用元素抗性（除了1218） / Apply element resistance (except 1218) / エレメント抵抗を適用（1218以外）
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
                    // 跳过1218检查，直接应用其他抗性 / Skip 1218 check, directly apply other resistances / 1218チェックをスキップ、直接他の抵抗を適用
                    damage = Element.GetResistDamage((int)damage, target.Evalue(e.source.aliasRef), resistBonus);
                }
            }

            // 3. 跳过1218减伤部分 / Skip 1218 damage reduction part / 1218ダメージ軽減部分をスキップ
            // 原始代码: damage = damage * (1000 - Mathf.Min(target.Evalue(1218), 1000)) / 1000;
            // 我们直接跳过这一行 / We directly skip this line / この行を直接スキップ

            UnityEngine.Debug.Log($"[MOD] 最终伤害计算: {baseDamage} -> {damage}");  // 最终伤害计算日志 / Final damage calculation log / 最終ダメージ計算ログ
            return damage;
        }

        // 应用无1218减伤的伤害 / Apply damage without 1218 reduction / 1218軽減なしのダメージを適用
        private static void ApplyDamageWithout1218(Card target, long damage, int ele, int eleP,
            AttackSource attackSource, Card origin, bool showEffect, Thing weapon, Chara originalTarget)
        {
            try
            {
                // 直接调用基础的HP减少逻辑 / Directly call basic HP reduction logic / 直接基本HP減少ロジックを呼び出す
                int originalHP = target.hp;
                target.hp -= (int)damage;

                UnityEngine.Debug.Log($"[MOD] 应用伤害: {damage}, HP: {originalHP} -> {target.hp}");  // 应用伤害日志 / Apply damage log / ダメージ適用ログ

                // 触发伤害效果 / Trigger damage effects / ダメージ効果をトリガー
                if (target.isSynced && damage != 0L)
                {
                    float ratio = (float)damage / (float)target.MaxHP;
                    Card effectTarget = (target.parent is Chara) ? (target.parent as Chara) : target;

                    // 播放伤害效果 / Play damage effect / ダメージ効果を再生
                    if (showEffect && attackSource != AttackSource.Condition)
                    {
                        effectTarget.PlayEffect("blood").SetParticleColor(EClass.Colors.matColors[target.material.alias].main).Emit(20 + (int)(30f * ratio));
                    }

                    // 显示伤害数字 / Display damage numbers / ダメージ数値を表示
                    if (EClass.core.config.test.showNumbers || target.isThing)
                    {
                        Element e = (ele == 0 || ele == 926) ? Element.Void : Element.Create(ele);
                        EClass.scene.damageTextRenderer.Add(target, effectTarget, (int)damage, e);
                    }
                }

                // 处理死亡逻辑 / Handle death logic / 死亡ロジックを処理
                if (target.hp < 0 && target.hp != originalHP)
                {
                    target.Die(Element.Create(ele), origin, attackSource, originalTarget);  // 触发死亡 / Trigger death / 死亡をトリガー
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[MOD] 应用伤害错误: {ex}");  // 应用伤害错误日志 / Apply damage error log / ダメージ適用エラーログ
            }
        }
    }
}
