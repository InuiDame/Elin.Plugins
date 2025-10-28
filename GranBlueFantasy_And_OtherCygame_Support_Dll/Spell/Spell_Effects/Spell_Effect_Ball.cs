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

namespace GBF.spell.Spell_Effects.Spell_Effect_Ball
{
    internal static class GBFAreaSpellEffects
    {
        // 执行区域法术攻击效果 / Perform area spell attack effects / エリア魔法攻撃効果を実行
        internal static void Atk(Chara cc, Point tp, int power, Element element, List<Point> targets, Act act, string alias, float exDelay = 0f, bool rev = false, string spellType = "ball_")
        {
            int num = Act.powerMod / 100;  // 威力修正计算 / Power modification calculation / 威力修正計算
            int num2 = ((act != null) ? act.ElementPowerMod : 50) / 50;  // 元素威力修正 / Element power modification / エレメント威力修正
            Dice dice = Dice.Create(alias ?? "ball_", power, cc, act);  // 创建伤害骰子 / Create damage dice / ダメージダイスを作成
            if (dice == null)
            {
                alias = ((alias != null) ? alias.Split(new char[]
                {
                    '_'
                })[0] : null) + "_";
                dice = Dice.Create(alias, power, cc, act);  // 重新创建骰子 / Recreate dice / ダイスを再作成
            }
            foreach (Point point in targets)
            {
                Effect effect;
                // 根据法术类型选择特效 / Select effect based on spell type / スペルタイプに基づいてエフェクトを選択
                if (!(spellType == "ball_") && !(spellType == "bolt_"))
                {
                    if (!(spellType == "hit_light") && !(spellType == "aura_heaven"))
                    {
                        effect = Effect.Get("Element/ball_Fire");  // 默认火球特效 / Default fireball effect / デフォルト火球エフェクト
                    }
                    else
                    {
                        effect = Effect.Get(spellType);
                    }
                }
                else
                {
                    effect = Effect.Get("Element/ball_" + ((element.id == 0) ? "Void" : element.source.alias.Remove(0, 3)));  // 元素对应特效 / Element corresponding effect / エレメント対応エフェクト
                }
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
                if (spellType == "ball_")
                {
                    effect2.Play(point, 0f, null, null).Flip(point.x > tp.x, false);  // 播放球状特效并翻转 / Play ball effect and flip / 球状エフェクトを再生して反転
                }
                else
                {
                    effect2.Play(point, 0f, null, null);  // 播放其他类型特效 / Play other type effects / 他のタイプエフェクトを再生
                }
                int num5 = dice.Roll();  // 骰子掷点 / Dice roll / ダイスロール
                int num6 = (int)((double)(num * num5) / (0.1 * (double)(9 + tp.Distance(point))));  // 伤害计算 / Damage calculation / ダメージ計算
                
                // 先把整个查询改写成 LINQ + ToList()，好调试也好维护 / Convert entire query to LINQ + ToList() for better debugging and maintenance / デバッグと保守性向上のため全体クエリをLINQ + ToList()に変換
                var hits = point
                    .ListCards(false)
                    .Where(card => card.isChara || card.trait.CanBeAttacked)
                    .ToList();

                foreach (Card card2 in hits)
                {
                    // 1) 先缓存下来，方便多次重用 / 1) Cache first for multiple reuse / 1) まずキャッシュ、複数回再利用用
                    Chara target = card2.Chara;

                    // 2) 只有在 target != null 且是友军/以上时，才走"友军"那套流程 / 2) Only when target != null and is friend/above, go through the "friendly" process / 2) target != null かつ味方/以上の場合のみ「味方」プロセスを実行
                    if (target != null && cc.Chara.IsFriendOrAbove(target))
                    {
                        int num7 = cc.Evalue(302);  // 获取控制等级 / Get control level / 制御レベルを取得
                        if (!cc.IsPC && cc.IsPCFactionOrMinion)
                            num7 += EClass.pc.Evalue(302);

                        if (num7 > 0)
                        {
                            if (cc.HasElement(1214, 1))
                                num7 *= 2;  // 元素1214加倍控制等级 / Element 1214 doubles control level / エレメント1214で制御レベル倍増

                            Debug.Log($"GBFSpell: controlLv:{num7 * 10}, dmg:{EClass.rnd(num6 + 1)}");  // 控制等级和伤害日志 / Control level and damage log / 制御レベルとダメージログ

                            if (num7 * 10 > EClass.rnd(num6 + 1))  // 控制等级检查 / Control level check / 制御レベルチェック
                            {
                                if (card2 == card2.pos.FirstChara)
                                    cc.ModExp(302, cc.IsPC ? 10 : 50);  // 修改经验值 / Modify experience / 経験値を修正
                                continue;   // 友军免伤逻辑：跳过后面的 DamageHP / Friendly damage immunity logic: skip subsequent DamageHP / 味方ダメージ免除ロジック：後続のDamageHPをスキップ
                            }
                            else
                            {
                                num6 = EClass.rnd(num6 * 100 / (100 + num7 * 10 + 1));  // 根据控制等级减少伤害 / Reduce damage based on control level / 制御レベルに基づいてダメージを減少
                                if (card2 == card2.pos.FirstChara)
                                    cc.ModExp(302, cc.IsPC ? 20 : 100);  // 修改经验值 / Modify experience / 経験値を修正
                            }
                        }
                    }

                    // 3) 不管是不是 Chara，都可以调用 DamageHP / 3) Whether it's Chara or not, DamageHP can be called / 3) Charaかどうかに関わらず、DamageHPを呼び出し可能
                    card2.DamageHP(num6, element.id, power * num2, AttackSource.None, cc, true);

                    // 4) 只有真实的 Chara 且不是玩家派系/仆从时，才调整 hostility / 4) Only adjust hostility for real Chara that are not player faction/minions / 4) 実際のCharaでプレイヤー派閥/ミニオンでない場合のみ敵意を調整
                    if (card2.isChara && target != null && !target.IsPCFactionOrMinion)
                    {
                        target.hostility -= 2;  // 降低敌意 / Reduce hostility / 敵意を低下
                    }

                    cc.Say("spell_bolt_hit", cc, card2, element.Name.ToLower(), null);  // 播放命中语音 / Play hit voice / 命中音声を再生
                }
            }
        }
        
        // 搜索目标点并编号 / Search target points and number them / ターゲットポイントを検索して番号付け
        internal static Dictionary<Point, int> Search(List<Point> targets, int num = 0)
        {
            Dictionary<Point, int> dictionary = new Dictionary<Point, int>();
            foreach (Point point in targets)
            {
                foreach (Card card2 in from card in point.ListCards(false)
                                       where card.isChara || card.trait.CanBeAttacked
                                       select card)
                {
                    if (!dictionary.ContainsKey(card2.pos))  // 避免重复点 / Avoid duplicate points / 重複ポイントを回避
                    {
                        dictionary.Add(card2.pos, ++num);  // 添加点到字典并编号 / Add point to dictionary and number / ポイントを辞書に追加して番号付け
                    }
                }
            }
            return dictionary;
        }
    }
}
