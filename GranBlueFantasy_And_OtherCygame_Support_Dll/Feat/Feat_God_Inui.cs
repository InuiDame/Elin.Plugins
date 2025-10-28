using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace GBF.feat.Feat_God_Inui
{
    internal class featGod_inui : Feat
    {
        private bool hint = true;  // 提示显示标志 / Hint display flag / ヒント表示フラグ
        
        internal void _OnApply(int add, ElementContainer eleOwner, bool hint)
        {
            this.hint = hint;
            GodHint();  // 调用神祇提示 / Call god hint / 神のヒントを呼び出す
            // 修改属性，设置潜力，应用效果等 / Modify attributes, set potential, apply effects, etc. / 属性を修正、潜在力を設定、効果を適用など
        }
        
        public override Sprite GetIcon(string suffix = "")
        {
            return SpriteSheet.Get(source.alias);  // 获取专长图标 / Get feat icon / 特技アイコンを取得
        }

        private void GodHint()
        {
            if (!hint)
            {
                return;
            }

            // 遍历信仰元素并显示提示 / Iterate through faith elements and display hints / 信仰エレメントを巡回してヒントを表示
            foreach (Element value2 in owner.Card.Chara.faithElements.dict.Values)
            {
                if (value2.source.id != id)
                {
                    NoteElement(value2.id, value2.Value);  // 记录元素信息 / Note element information / エレメント情報を記録
                }
            }
        }

        private void Note(string s)
        {
            if (hint)
            {
                hints.Add(s);  // 添加提示信息 / Add hint message / ヒントメッセージを追加
            }
        }

        private void NoteElement(int ele, int a)
        {
            SourceElement.Row row = EClass.sources.elements.map[ele];
            if (row.category == "ability")
            {
                Note("hintLearnAbility".lang(row.GetName().ToTitleCase()));  // 学习能力提示 / Learn ability hint / 能力習得ヒント
            }
            else if (row.tag.Contains("flag"))
            {
                Note(row.GetName());  // 标记名称提示 / Flag name hint / フラグ名ヒント
            }
            else
            {
                string @ref = ((a < 0) ? "" : "+") + a;
                if (row.category == "resist")
                {
                    int num3 = 0;
                    @ref = ((a > 0) ? "+" : "-").Repeat(Mathf.Clamp(Mathf.Abs(a) / 5 + num3, 1, 5));
                    Note("modValueRes".lang(row.GetName(), @ref));  // 抗性修改提示 / Resistance modification hint / 抵抗修正ヒント
                }
                else
                {
                    Note("modValue".lang(row.GetName(), @ref));  // 数值修改提示 / Value modification hint / 数値修正ヒント
                }
            }
        }
    }
    // 这下面的补丁生效了多少我暂时不清楚，我只测试过伤害提升的效果，如果无法使用的话，请通过issue反馈给我。
// I'm not sure how many of the patches below are actually working, I've only tested the damage boost effect. If they don't work, please report to me via issues.
// 以下のパッチがどの程度有効かは暫時不明です。ダメージ向上効果のみテストしました。使用できない場合は、Issueで報告してください。
    public static class HellFeatHelper
    {
        public const int HELL_SPELL_MASTERY_FEAT_ID = 170028;  // 地狱法术精通专长ID / Hell spell mastery feat ID / 地獄呪文習熟特技ID

        // 检查是否拥有地狱法术精通 / Check if has hell spell mastery / 地獄呪文習熟を持っているかチェック
        public static bool HasHellSpellMastery(Chara character)
        {
            return character != null && character.HasElement(HELL_SPELL_MASTERY_FEAT_ID);
        }

        // 检查是否为地狱法术 / Check if is hell spell / 地獄呪文かチェック
        public static bool IsHellSpell(Act act)
        {
            if (act == null) return false;

            if (act.source.aliasRef == "eleNether")  // 以太元素判断 / Ether element judgment / エーテルエレメント判定
                return true;

            return false;
        }

        // 获取地狱精通加成值 / Get hell mastery bonus value / 地獄習熟ボーナス値を取得
        public static int GetHellMasteryBonus(Chara caster)
        {
            if (caster == null || !HasHellSpellMastery(caster)) return 0;

            int hellMastery = caster.Evalue(916);  // 获取元素916的值 / Get value of element 916 / エレメント916の値を取得
            return hellMastery;
        }
    }

    // 在GetPower方法中强化地狱魔法 / Enhance hell magic in GetPower method / GetPowerメソッドで地獄魔法を強化
    [HarmonyPatch(typeof(Ability))]
    [HarmonyPatch("GetPower")]
    public static class HellSpellPowerBoostPatch
    {
        static void Postfix(Ability __instance, Card c, ref int __result)
        {
            if (c == null || !(c is Chara chara)) return;

            // 检查是否是地狱魔法且拥有Feat / Check if hell magic and has feat / 地獄魔法かつ特技を持っているかチェック
            if (HellFeatHelper.IsHellSpell(__instance) && HellFeatHelper.HasHellSpellMastery(chara))
            {
                int hellBonus = HellFeatHelper.GetHellMasteryBonus(chara);

                // 应用地狱精通加成 / Apply hell mastery bonus / 地獄習熟ボーナスを適用
                __result = __result * (100 + hellBonus) / 100;

                // 额外15%基础加成 / Additional 15% base bonus / 追加15%基本ボーナス
                __result = __result * 115 / 100; 
            }
        }
    }

    // 地狱魔法附加特效 / Hell magic additional effects / 地獄魔法追加効果
    [HarmonyPatch(typeof(AttackProcess))]
    [HarmonyPatch("Perform")]
    public static class HellSpellEffectPatch
    {
        static void Postfix(AttackProcess __instance, bool __result)
        {
            if (!__result || !__instance.hit || __instance.CC == null || __instance.TC == null)
                return;

            // 检查是否拥有Feat / Check if has feat / 特技を持っているかチェック
            if (IsHellSpellAttack(__instance) && HellFeatHelper.HasHellSpellMastery(__instance.CC))
            {
                int hellMastery = HellFeatHelper.GetHellMasteryBonus(__instance.CC);

                // 30%基础概率+地狱精通/2的燃烧效果 / 30% base probability + hell mastery/2 burning effect / 30%基本確率+地獄習熟/2の燃焼効果
                if (__instance.TC.isChara && EClass.rnd(100) < 30 + hellMastery / 2)
                {
                    __instance.TC.Chara.AddCondition<ConBurning>(hellMastery);  // 添加燃烧状态 / Add burning condition / 燃焼状態を追加
                }

                // 20%概率恢复MP / 20% chance to restore MP / 20%の確率でMP回復
                if (EClass.rnd(100) < 20)
                {
                    int mpGain = hellMastery / 50 + 1;  // MP恢复量计算 / MP recovery amount calculation / MP回復量計算
                    __instance.CC.mana.Mod(mpGain);  // 修改MP值 / Modify MP value / MP値を修正
                }
            }
        }

        // 检查是否为地狱法术攻击 / Check if hell spell attack / 地獄呪文攻撃かチェック
        private static bool IsHellSpellAttack(AttackProcess attack)
        {
            return (attack.weapon == null || attack.IsCane) &&  // 无武器或法杖 / No weapon or cane / 武器なしまたは杖
                   attack.CC != null &&
                   attack.CC.MainElement?.id == 916;  // 主元素为916 / Main element is 916 / 主エレメントが916
        }
    }

    // 魔法施放时的地狱强化 / Hell enhancement during spell casting / 呪文詠唱時の地獄強化
    [HarmonyPatch(typeof(Chara))]
    [HarmonyPatch("UseAbility", new Type[] { typeof(Act), typeof(Card), typeof(Point), typeof(bool) })]
    public static class HellSpellCastingPatch
    {
        static void Postfix(Chara __instance, Act a, bool __result)
        {
            if (!__result || a == null) return;

            // 检查是否拥有Feat / Check if has feat / 特技を持っているかチェック
            if (HellFeatHelper.IsHellSpell(a) && HellFeatHelper.HasHellSpellMastery(__instance))
            {
                int hellMastery = HellFeatHelper.GetHellMasteryBonus(__instance);

                if (a is Spell && hellMastery > 0)
                {
                    int mpReduction = hellMastery / 100; // 每10点地狱精通减少1%MP消耗 / Every 10 hell mastery reduces 1% MP cost / 地獄習熟10ごとにMP消費1%減少
                    // 这里需要在MP计算阶段处理，可能需要Transpiler / Need to handle in MP calculation phase, may need transpiler / MP計算段階で処理する必要あり、トランスパイラが必要かも
                }
            }
        }
    }
}



