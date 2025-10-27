using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Reflection.Emit;
using UnityEngine;
using Cwl.API.Processors;
using Microsoft.CodeAnalysis;
using HarmonyLib;
using BepInEx;
using BepInEx.Configuration;
using Cwl;
using Cwl.API.Custom;
using Cwl.LangMod;
using Cwl.Helper;
using Newtonsoft.Json;

namespace SpellWrite
{
    internal static class GFAreaSpellEffects
    {
        // 执行区域法术攻击 / Perform area spell attack / エリア魔法攻撃を実行する
        internal static void Atk(Chara cc, Point tp, int power, Element element, List<Point> targets, Act act, string alias, float exDelay = 0f, bool rev = false, string spellType = "ball_")
        {
            int num = Act.powerMod / 100;
            int num2 = ((act != null) ? act.ElementPowerMod : 50) / 50;
            Dice dice = Dice.Create(alias ?? "ball_", power, cc, act);
            if (dice == null)
            {
                alias = ((alias != null) ? alias.Split(new char[]
                {
                '_'
                })[0] : null) + "_";
                dice = Dice.Create(alias, power, cc, act);
            }
            foreach (Point point in targets)
            {
                Effect effect;
                if (!(spellType == "ball_") && !(spellType == "bolt_"))
                {
                    if (!(spellType == "hit_light") && !(spellType == "aura_heaven"))
                    {
                        effect = Effect.Get("Element/ball_Fire");
                    }
                    else
                    {
                        effect = Effect.Get(spellType);
                    }
                }
                else
                {
                    effect = Effect.Get("Element/ball_" + ((element.id == 0) ? "Void" : element.source.alias.Remove(0, 3)));
                }
                Effect effect2 = effect;
                int num3 = Math.Max(tp.Distance(point), 1);
                float num4;
                if (rev)
                {
                    num4 = 0.25f / (float)num3;
                }
                else
                {
                    num4 = 0.04f * (float)num3;
                }
                effect2.SetStartDelay(num4 + exDelay);
                if (spellType == "ball_")
                {
                    effect2.Play(point, 0f, null, null).Flip(point.x > tp.x, false);
                }
                else
                {
                    effect2.Play(point, 0f, null, null);
                }
                int num5 = dice.Roll();
                int num6 = (int)((double)(num * num5) / (0.1 * (double)(9 + tp.Distance(point))));
                
                // 优化：预先获取点上的卡片列表，避免重复调用 / Optimization: Pre-get card list on point to avoid repeated calls / 最適化：ポイント上のカードリストを事前取得、重複呼び出しを回避
                var cardsOnPoint = point.ListCards(false);
                if (cardsOnPoint != null)
                {
                    foreach (Card card2 in cardsOnPoint.Where(card => card.isChara || card.trait.CanBeAttacked))
                    {
                        // 改动：先检查是否存在角色对象 / Change: First check if character object exists / 変更：まずキャラクターオブジェクトが存在するかチェック
                        if (card2.Chara != null)
                        {
                            // 仅对角色类型进行友方判断 / Only perform friendly judgment for character types / キャラクタータイプのみ友好判定を実行
                            if (cc.IsFriendOrAbove(card2.Chara))
                            {
                                int num7 = cc.Evalue(302);
                                if (!cc.IsPC && cc.IsPCFactionOrMinion)
                                {
                                    num7 += EClass.pc.Evalue(302);
                                }
                                if (num7 > 0)
                                {
                                    if (cc.HasElement(1214, 1))
                                    {
                                        num7 *= 2;
                                    }
                                    Debug.Log($"GFSpell: controlLv:{num7 * 10}, dmg:{EClass.rnd(num6 + 1)}");
                                    if (num7 * 10 > EClass.rnd(num6 + 1))
                                    {
                                        // 如果目标是位置中的"第一角色"，只获得经验不扣血 / If target is "first character" in position, only gain experience without HP loss / ターゲットが位置内の「最初のキャラクター」の場合、経験値のみ獲得、HP減少なし
                                        if (card2 == card2.pos.FirstChara)
                                        {
                                            cc.ModExp(302, cc.IsPC ? 10 : 50);
                                            continue;
                                        }
                                        continue;
                                    }
                                    else
                                    {
                                        num6 = EClass.rnd(num6 * 100 / (100 + num7 * 10 + 1));
                                        if (card2 == card2.pos.FirstChara)
                                        {
                                            cc.ModExp(302, cc.IsPC ? 20 : 100);
                                        }
                                    }
                                }
                            }
                            // 如果是角色且非友方，或结束友方逻辑后，继续到伤害处理 / If character and not friendly, or after friendly logic ends, continue to damage processing / キャラクターで友好でない場合、または友好ロジック終了後、ダメージ処理に継続
                        }

                        // 无论是否为角色，都造成伤害 / Cause damage regardless of whether it's a character / キャラクターかどうかに関わらずダメージを与える
                        // 如果是"假人"（card2.Chara == null），仍然执行此处 / If "dummy" (card2.Chara == null), still execute here / 「ダミー」の場合（card2.Chara == null）、ここを実行
                        card2.DamageHP(num6, element.id, power * num2, AttackSource.None, cc, true);

                        // 如果是角色且非玩家派系/仆从，降低敌意 / If character and not player faction/minion, reduce hostility / キャラクターでプレイヤー派閥/ミニオンでない場合、敵意を低下
                        if (card2.isChara && !card2.Chara.IsPCFactionOrMinion)
                        {
                            card2.Chara.hostility -= 2;
                        }
                        cc.Say("spell_bolt_hit", cc, card2, element.Name.ToLower(), null);
                    }
                }

                // 检查并破坏可破坏的对象 / Check and destroy destructible objects / 破壊可能なオブジェクトをチェックして破壊
                if (point.HasObj && point.cell.matObj.hardness <= power / 20)
                {
                    EClass._map.MineObj(point, null, null);
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
                    if (!dictionary.ContainsKey(card2.pos))
                    {
                        dictionary.Add(card2.pos, ++num);
                    }
                }
            }
            return dictionary;
        }
    }
    
    internal class SpellGF : Spell
{
    // 基础GF法术类 / Basic GF spell class / 基本GFスペルクラス
}

internal class SpellGF2 : Spell
{
    // GF法术2：基础法术实现 / GF Spell 2: Basic spell implementation / GFスペル2：基本スペル実装
    public override bool Perform()
    {
        Element element = Act.CC.elements.GetElement(base.source.aliasParent);
        int spellExp = Act.CC.Chara.elements.GetSpellExp(Act.CC.Chara, base.act, 100);
        Act.CC.Chara.ModExp(base.source.alias, spellExp);
        return true;
    }

    public override bool CanAutofire => true;        // 允许自动发射 / Allow auto fire / オート発射可能
    public override bool CanPressRepeat => true;     // 允许按住重复 / Allow press repeat / 押し続け繰り返し可能
    public override bool CanRapidFire => true;       // 允许快速射击 / Allow rapid fire / ラピッド射撃可能
    public override float RapidDelay => 0.3f;        // 快速射击延迟 / Rapid fire delay / ラピッド射撃遅延
}

internal class SpellGF_Grande : Spell
{
    // GF Grande法术：高级范围法术 / GF Grande Spell: Advanced area spell / GFグランデスペル：高度な範囲スペル
    public override bool CanAutofire => true;        // 允许自动发射 / Allow auto fire / オート発射可能
    public override bool CanPressRepeat => true;     // 允许按住重复 / Allow press repeat / 押し続け繰り返し可能
    public override bool CanRapidFire => true;       // 允许快速射击 / Allow rapid fire / ラピッド射撃可能

    public override bool Perform()
    {
        // 执行法术主要逻辑 / Execute main spell logic / スペルのメインロジックを実行
        ProcAt_Internal(Act.CC.elements.GetElement(base.source.aliasParent)?.Value ?? 1, base.source, base.act);
        int spellExp = Act.CC.Chara.elements.GetSpellExp(Act.CC.Chara, base.act);
        Act.CC.Chara.ModExp(base.source.alias, spellExp);
        
        // 处理法术附加效果 / Process spell additional effects / スペル追加効果を処理
        if (base.source.proc.Length > 0)
        {
            string text = base.source.aliasRef.IsEmpty(Act.CC.MainElement.source.alias);
            string text2 = base.source.proc[0];
            
            // 处理特殊效果类型 / Process special effect types / 特殊効果タイプを処理
            if ((text2 == "LulwyTrick" || text2 == "BuffStats") && base.source.proc.Length >= 2)
            {
                text = base.source.proc[1];
            }
            else if (text == "mold")
            {
                text = Act.CC.MainElement.source.alias;
            }
            
            // 设置目标和位置 / Set target and position / ターゲットと位置を設定
            if (TargetType.Range == TargetRange.Self && !Act.forcePt)
            {
                Act.TC = Act.CC;
                Act.TP.Set(Act.CC.pos);
            }
            
            // 计算威力并执行效果 / Calculate power and execute effect / 威力を計算して効果を実行
            int power = Act.CC.elements.GetOrCreateElement(base.source.id).GetPower(Act.CC);
            ActEffect.ProcAt(base.source.proc[0].ToEnum<EffectId>(), power, BlessedState.Normal, Act.CC, Act.TC, Act.TP, base.source.tag.Contains("neg"), new ActRef
            {
                n1 = base.source.proc.TryGet(1, returnNull: true),
                aliasEle = text,
                act = this
            });
        }
        return true;
    }

    // 内部攻击处理 / Internal attack processing / 内部攻撃処理
    private static void ProcAt_Internal(int power, SourceElement.Row source, Act act)
    {
        Element element = Element.Create(source.aliasRef, power / 10);
        Point tP = Act.TP;
        Map map = EClass._map;
        
        // 计算作用半径 / Calculate effect radius / 効果半径を計算
        int radius = (int)((double)source.radius + 0.01 * (double)power);
        
        // 创建十字形目标区域 / Create cross-shaped target area / 十字形のターゲットエリアを作成
        List<Point> list = new List<Point>();
        list.AddRange(map.ListPointsInLine(tP, new Point(tP.x + 1, tP.z), 2));  // 右方向 / Right direction / 右方向
        list.AddRange(map.ListPointsInLine(tP, new Point(tP.x - 1, tP.z), 2));  // 左方向 / Left direction / 左方向
        list.AddRange(map.ListPointsInLine(tP, new Point(tP.x, tP.z + 1), 2));  // 下方向 / Down direction / 下方向
        list.AddRange(map.ListPointsInLine(tP, new Point(tP.x, tP.z - 1), 2));  // 上方向 / Up direction / 上方向
        
        // 去除重复点 / Remove duplicate points / 重複ポイントを除去
        List<Point> targets = (from p in list
                               group p by new { p.x, p.z } into g
                               select g.First()).ToList();
        
        // 播放施法音效和动画 / Play casting sound and animation / 詠唱音声とアニメーションを再生
        Act.CC.Chara.Say("spell_ball", Act.CC.Chara, element.Name.ToLower());
        EClass.Wait(0.8f, Act.CC.Chara);
        ActEffect.TryDelay(delegate
        {
            Act.CC.Chara.PlaySound("spell_ball");
        });
        
        // 计算实际威力（基础威力 + 等级加成） / Calculate actual power (base power + level bonus) / 実際の威力を計算（基本威力 + レベルボーナス）
        int LVpower = Act.CC.LV;
        int RealPower = power/10 + LVpower*5;
        
        // 执行区域法术攻击 / Execute area spell attack / エリアスペル攻撃を実行
        GFAreaSpellEffects.Atk(Act.CC, tP, RealPower, element, targets, act, source.alias);
    }
}
}
