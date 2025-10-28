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

namespace GBF.spell.Spell_Effects.Spell_Effect_Custom_Effect_DiceChange
{
    /// 通过读取texture文件夹下的{alias}_effect.png来自动为使用这个Spell类型的法术修改特效，你也可以改成别的。
    /// By reading {alias}_effect.png in the texture folder, it automatically modifies effects for spells using this Spell type. You can also change it to something else.
    /// テクスチャフォルダ内の{alias}_effect.pngを読み取ることで、このSpellタイプを使用する呪文のエフェクトを自動的に変更します。他のものに変更することもできます。
    ///这个类型将骰子从ball修改为了sword
    /// This type changes the dice from ball to sword
    ///このタイプはダイスをballからswordに変更します
    /// 在命中目的地播放一个大的特效，而不是在每个命中点位播放一个特效
    /// playing one large effect at the hit destination rather than playing an effect at each hit point
    /// 各ヒットポイントでエフェクトを再生するのではなく、ヒット先で1つの大きなエフェクトを再生します
     internal static class GBFSpellEffects_0533
{
    // 执行0533类型法术攻击效果 / Perform 0533 type spell attack effects / 0533タイプ魔法攻撃効果を実行
    internal static void Atk(Chara cc, Point tp, int power, Element element, List<Point> targets, Act act, string alias, float exDelay = 0f, bool rev = false, string spellType = "ball_")
    {
        // 添加参数检查 / Add parameter checks / パラメータチェックを追加
        if (cc == null) throw new ArgumentNullException(nameof(cc));
        if (element == null) throw new ArgumentNullException(nameof(element));
        if (targets == null) throw new ArgumentNullException(nameof(targets));
        
        int num = Act.powerMod / 100;  // 威力修正计算 / Power modification calculation / 威力修正計算
        int num2 = ((act != null) ? act.ElementPowerMod : 50) / 50;  // 元素威力修正 / Element power modification / エレメント威力修正
        
        // 修复 Dice 创建问题 / Fix Dice creation issue / ダイス作成問題を修正
        // 这个类型将骰子从ball修改为了sword / This type changes the dice from ball to sword / このタイプはダイスをballからswordに変更します
        string diceAlias = !string.IsNullOrEmpty(alias) ? alias : "sword_";
        Dice dice = Dice.Create(diceAlias, power, cc, act);

        if (dice == null)
        {
            // 尝试使用默认的骰子类型 / Try using default dice type / デフォルトダイスタイプを使用してみる
            diceAlias = "sword_";
            dice = Dice.Create(diceAlias, power, cc, act);

            if (dice == null)
            {
                // 如果还是失败，创建一个最简单的骰子 / If still fails, create the simplest dice / それでも失敗した場合、最もシンプルなダイスを作成
                dice = new Dice(power, power); // 最小值和最大值相同 / Min and max values are the same / 最小値と最大値が同じ
                Debug.LogWarning($"Using fallback dice for alias: {alias}");  // 备用骰子警告 / Fallback dice warning / 代替ダイス警告
            }
        }

        foreach (Point point in targets)
        {
            int num3 = Math.Max(tp.Distance(point), 1);  // 计算目标点距离 / Calculate target point distance / ターゲットポイント距離を計算
            float num4;
            if (rev)
            {
                num4 = 0.25f / (float)num3;  // 反向延迟计算 / Reverse delay calculation / 逆方向遅延計算
            }
            else
            {
                num4 = 0.04f * (float)num3;  // 正向延迟计算 / Forward delay calculation / 順方向遅延計算
            }

            int num5 = dice.Roll();  // 骰子掷点 / Dice roll / ダイスロール
            int num6 = (int)((double)(num * num5) / (0.1 * (double)(9 + tp.Distance(point))));  // 伤害计算 / Damage calculation / ダメージ計算

            // 获取目标点上的可攻击对象 / Get attackable objects on target point / ターゲットポイント上の攻撃可能オブジェクトを取得
            var hits = point
                .ListCards(false)
                .Where(card => card.isChara || card.trait.CanBeAttacked)
                .ToList();

            foreach (Card card2 in hits)
            {
                Chara target = card2.Chara;  // 缓存目标角色 / Cache target character / ターゲットキャラをキャッシュ

                // 友军伤害免疫逻辑 / Friendly damage immunity logic / 味方ダメージ免除ロジック
                if (target != null && cc.Chara.IsFriendOrAbove(target))
                {
                    int num7 = cc.Evalue(302);  // 获取控制等级 / Get control level / 制御レベルを取得
                    if (!cc.IsPC && cc.IsPCFactionOrMinion)
                        num7 += EClass.pc.Evalue(302);

                    if (num7 > 0)
                    {
                        if (cc.HasElement(1214, 1))
                            num7 *= 2;  // 元素1214加倍控制等级 / Element 1214 doubles control level / エレメント1214で制御レベル倍増

                        Debug.Log($"GBFSpell0533: controlLv:{num7 * 10}, dmg:{EClass.rnd(num6 + 1)}");  // 控制等级和伤害日志 / Control level and damage log / 制御レベルとダメージログ

                        if (num7 * 10 > EClass.rnd(num6 + 1))  // 控制等级检查 / Control level check / 制御レベルチェック
                        {
                            if (card2 == card2.pos.FirstChara)
                                cc.ModExp(302, cc.IsPC ? 10 : 50);  // 修改经验值 / Modify experience / 経験値を修正
                            continue;   // 跳过友军伤害 / Skip friendly damage / 味方ダメージをスキップ
                        }
                        else
                        {
                            num6 = EClass.rnd(num6 * 100 / (100 + num7 * 10 + 1));  // 根据控制等级减少伤害 / Reduce damage based on control level / 制御レベルに基づいてダメージを減少
                            if (card2 == card2.pos.FirstChara)
                                cc.ModExp(302, cc.IsPC ? 20 : 100);  // 修改经验值 / Modify experience / 経験値を修正
                        }
                    }
                }

                // 应用伤害 - 使用魔法剑攻击来源 / Apply damage - using magic sword attack source / ダメージを適用 - 魔法剣攻撃ソースを使用
                card2.DamageHP(num6, element.id, power * num2, AttackSource.MagicSword, cc, true);

                // 调整敌意 / Adjust hostility / 敵意を調整
                if (card2.isChara && target != null && !target.IsPCFactionOrMinion)
                {
                    target.hostility -= 2;  // 降低敌意 / Reduce hostility / 敵意を低下
                }

                cc.Say("Spell_GBF_0533_hit", cc, card2, element.Name.ToLower(), null);  // 播放命中语音 / Play hit voice / 命中音声を再生
            }
        }
    }
}
}
