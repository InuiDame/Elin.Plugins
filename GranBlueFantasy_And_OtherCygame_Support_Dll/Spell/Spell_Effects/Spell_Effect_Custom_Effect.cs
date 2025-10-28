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

namespace GBF.spell.Spell_Effects.Spell_Effect_Custom_Effect
{
    /// 通过读取texture文件夹下的{alias}_effect.png来自动为使用这个Spell类型的法术修改特效，你也可以改成别的。
    /// By reading {alias}_effect.png in the texture folder, it automatically modifies effects for spells using this Spell type. You can also change it to something else.
    /// テクスチャフォルダ内の{alias}_effect.pngを読み取ることで、このSpellタイプを使用する呪文のエフェクトを自動的に変更します。他のものに変更することもできます。
    internal static class GBFSpellEffects_2700
{
    // 执行2700类型法术攻击效果 / Perform 2700 type spell attack effects / 2700タイプ魔法攻撃効果を実行
    internal static void Atk(Chara cc, Point tp, int power, Element element, List<Point> targets, Act act, string alias, float exDelay = 0f, bool rev = false, string spellType = "ball_")
    {
        // 添加参数检查 / Add parameter checks / パラメータチェックを追加
        if (cc == null) throw new ArgumentNullException(nameof(cc));
        if (element == null) throw new ArgumentNullException(nameof(element));
        if (targets == null) throw new ArgumentNullException(nameof(targets));
        
        // 全面的 null 检查 / Comprehensive null checks / 包括的なnullチェック
        if (cc == null)
        {
            Debug.LogError("cc is null in GBFSpellEffects_2700.Atk");  // 施法者为空错误 / Caster is null error / キャスターがnullのエラー
            return;
        }

        if (element == null)
        {
            Debug.LogError("element is null in GBFSpellEffects_2700.Atk");  // 元素为空错误 / Element is null error / エレメントがnullのエラー
            return;
        }

        if (targets == null)
        {
            Debug.LogError("targets is null in GBFSpellEffects_2700.Atk");  // 目标列表为空错误 / Targets list is null error / ターゲットリストがnullのエラー
            return;
        }
        
        int num = Act.powerMod / 100;  // 威力修正计算 / Power modification calculation / 威力修正計算
        int num2 = ((act != null) ? act.ElementPowerMod : 50) / 50;  // 元素威力修正 / Element power modification / エレメント威力修正
        
        // 修复 Dice 创建问题 / Fix Dice creation issue / ダイス作成問題を修正
        string diceAlias = !string.IsNullOrEmpty(alias) ? alias : "ball_";
        Dice dice = Dice.Create(diceAlias, power, cc, act);

        if (dice == null)
        {
            // 尝试使用默认的骰子类型 / Try using default dice type / デフォルトダイスタイプを使用してみる
            diceAlias = "ball_";
            dice = Dice.Create(diceAlias, power, cc, act);

            if (dice == null)
            {
                // 如果还是失败，创建一个最简单的骰子 / If still fails, create the simplest dice / それでも失敗した場合、最もシンプルなダイスを作成
                dice = new Dice(power, power); // 最小值和最大值相同 / Min and max values are the same / 最小値と最大値が同じ
                Debug.LogWarning($"Using fallback dice for alias: {alias}");  // 备用骰子警告 / Fallback dice warning / 代替ダイス警告
            }
        }
        
        // 动态生成特效名称 / Dynamically generate effect name / 動的にエフェクト名を生成
        string effectName = $"{alias}_effect";
        Debug.Log($"DynamicSpellEffects: 技能alias={alias}, 生成效果名称={effectName}");  // 特效名称日志 / Effect name log / エフェクト名ログ

        foreach (Point point in targets)
        {
            // 简化：无论什么技能类型都使用同一种特效 / Simplified: Use the same effect regardless of skill type / 簡素化：スキルタイプに関わらず同じエフェクトを使用
            Effect effect = Effect.Get(effectName);

            Effect effect2 = effect;
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
            effect2.SetStartDelay(num4 + exDelay);  // 设置特效启动延迟 / Set effect start delay / エフェクト開始遅延を設定

            // 简化：移除条件判断，统一播放特效 / Simplified: Remove conditional judgment, uniformly play effects / 簡素化：条件判断を削除、統一してエフェクトを再生
            effect2.Play(point, 0f, null, null);  // 播放特效 / Play effect / エフェクトを再生

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

                        Debug.Log($"GBFSpell2700: controlLv:{num7 * 10}, dmg:{EClass.rnd(num6 + 1)}");  // 控制等级和伤害日志 / Control level and damage log / 制御レベルとダメージログ

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

                // 应用伤害 / Apply damage / ダメージを適用
                card2.DamageHP(num6, element.id, power * num2, AttackSource.None, cc, true);

                // 调整敌意 / Adjust hostility / 敵意を調整
                if (card2.isChara && target != null && !target.IsPCFactionOrMinion)
                {
                    target.hostility -= 2;  // 降低敌意 / Reduce hostility / 敵意を低下
                }

                cc.Say("Spell_GBF_2700_hit", cc, card2, element.Name.ToLower(), null);  // 播放命中语音 / Play hit voice / 命中音声を再生
            }
        }
    }
}
}
