using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using UnityEngine;
using Cwl.API.Processors;
using Microsoft.CodeAnalysis;
using HarmonyLib;
using BepInEx;
using BepInEx.Configuration;
using Cwl;
using Cwl.API.Custom;
using BS.magicshop;
using Cwl.LangMod;
using static QuestCraft;
using System.Reflection;

namespace Spell_Rewrite
{
    internal static class BSAreaSpellEffects
    {
        /// <summary>
        /// 执行区域法术攻击效果
        /// Perform area spell attack effects
        /// </summary>
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
                // 先把整个查询改写成 LINQ + ToList()，好调试也好维护
                // Convert the entire query to LINQ + ToList() for better debugging and maintenance
                var hits = point
                    .ListCards(false)
                    .Where(card => card.isChara || card.trait.CanBeAttacked)
                    .ToList();

                foreach (Card card2 in hits)
                {
                    // 1) 先缓存下来，方便多次重用
                    // 1) Cache first for multiple reuse
                    Chara target = card2.Chara;

                    // 2) 只有在 target != null 且是友军/以上时，才走"友军"那套流程
                    // 2) Only when target != null and is friend/above, go through the "friendly" process
                    if (target != null && cc.Chara.IsFriendOrAbove(target))
                    {
                        int num7 = cc.Evalue(302);
                        if (!cc.IsPC && cc.IsPCFactionOrMinion)
                            num7 += EClass.pc.Evalue(302);

                        if (num7 > 0)
                        {
                            if (cc.HasElement(1214, 1))
                                num7 *= 2;

                            Debug.Log($"BSSpell: controlLv:{num7 * 10}, dmg:{EClass.rnd(num6 + 1)}");

                            if (num7 * 10 > EClass.rnd(num6 + 1))
                            {
                                if (card2 == card2.pos.FirstChara)
                                    cc.ModExp(302, cc.IsPC ? 10 : 50);
                                continue;   // 友军免伤逻辑：跳过后面的 DamageHP
                                            // Friendly damage immunity logic: skip subsequent DamageHP
                            }
                            else
                            {
                                num6 = EClass.rnd(num6 * 100 / (100 + num7 * 10 + 1));
                                if (card2 == card2.pos.FirstChara)
                                    cc.ModExp(302, cc.IsPC ? 20 : 100);
                            }
                        }
                    }

                    // 3) 不管是不是 Chara，都可以调用 DamageHP
                    // 3) Whether it's Chara or not, DamageHP can be called
                    card2.DamageHP(num6, element.id, power * num2, AttackSource.None, cc, true);

                    // 4) 只有真实的 Chara 且不是玩家派系/仆从时，才调整 hostility
                    // 4) Only adjust hostility for real Chara that are not player faction/minions
                    if (card2.isChara && target != null && !target.IsPCFactionOrMinion)
                    {
                        target.hostility -= 2;
                    }

                    cc.Say("spell_bolt_hit", cc, card2, element.Name.ToLower(), null);
                }
                if (point.HasObj && point.cell.matObj.hardness <= power / 20)
                {
                    EClass._map.MineObj(point, null, null);
                }
            }
        }

        /// <summary>
        /// 搜索目标点
        /// Search target points
        /// </summary>
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

    internal static class DarknessSpellEffects
    {
        /// <summary>
        /// 执行黑暗法术攻击效果
        /// Perform darkness spell attack effects
        /// </summary>
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
                        effect = Effect.Get("Element/eleDarkness");
                    }
                    else
                    {
                        effect = Effect.Get(spellType);
                    }
                }
                else
                {
                    effect = Effect.Get("Element/eleDarkness");
                }
                Effect effect2 = effect;
                int num3 = Math.Max(tp.Distance(point), 1);
                float num4;
                if (rev)
                {
                    num4 = 0.15f / (float)num3;
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
                // 先把整个查询改写成 LINQ + ToList()，好调试也好维护
                // Convert the entire query to LINQ + ToList() for better debugging and maintenance
                var hits = point
                    .ListCards(false)
                    .Where(card => card.isChara || card.trait.CanBeAttacked)
                    .ToList();

                foreach (Card card2 in hits)
                {
                    // 1) 先缓存下来，方便多次重用
                    // 1) Cache first for multiple reuse
                    Chara target = card2.Chara;

                    // 2) 只有在 target != null 且是友军/以上时，才走"友军"那套流程
                    // 2) Only when target != null and is friend/above, go through the "friendly" process
                    if (target != null && cc.Chara.IsFriendOrAbove(target))
                    {
                        int num7 = cc.Evalue(302);
                        if (!cc.IsPC && cc.IsPCFactionOrMinion)
                            num7 += EClass.pc.Evalue(302);

                        if (num7 > 0)
                        {
                            if (cc.HasElement(1214, 1))
                                num7 *= 2;

                            Debug.Log($"BSSpell: controlLv:{num7 * 10}, dmg:{EClass.rnd(num6 + 1)}");

                            if (num7 * 10 > EClass.rnd(num6 + 1))
                            {
                                if (card2 == card2.pos.FirstChara)
                                    cc.ModExp(302, cc.IsPC ? 10 : 50);
                                continue;   // 友军免伤逻辑：跳过后面的 DamageHP
                                            // Friendly damage immunity logic: skip subsequent DamageHP
                            }
                            else
                            {
                                num6 = EClass.rnd(num6 * 100 / (100 + num7 * 10 + 1));
                                if (card2 == card2.pos.FirstChara)
                                    cc.ModExp(302, cc.IsPC ? 20 : 100);
                            }
                        }
                    }

                    // 3) 不管是不是 Chara，都可以调用 DamageHP
                    // 3) Whether it's Chara or not, DamageHP can be called
                    card2.DamageHP(num6, element.id, power * num2, AttackSource.None, cc, true);

                    // 4) 只有真实的 Chara 且不是玩家派系/仆从时，才调整 hostility
                    // 4) Only adjust hostility for real Chara that are not player faction/minions
                    if (card2.isChara && target != null && !target.IsPCFactionOrMinion)
                    {
                        target.hostility -= 2;
                    }

                    cc.Say("spell_bolt_hit", cc, card2, element.Name.ToLower(), null);
                }
                if (point.HasObj && point.cell.matObj.hardness <= power / 20)
                {
                    EClass._map.MineObj(point, null, null);
                }
            }
        }

        /// <summary>
        /// 搜索目标点
        /// Search target points
        /// </summary>
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

    internal static class CutSpellEffects
    {
        /// <summary>
        /// 执行切割法术攻击效果
        /// Perform cut spell attack effects
        /// </summary>
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
                        effect = Effect.Get("Element/eleCut");
                    }
                    else
                    {
                        effect = Effect.Get(spellType);
                    }
                }
                else
                {
                    effect = Effect.Get("Element/eleCut");
                }
                Effect effect2 = effect;
                int num3 = Math.Max(tp.Distance(point), 1);
                float num4;
                if (rev)
                {
                    num4 = 0.15f / (float)num3;
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
                // 先把整个查询改写成 LINQ + ToList()，好调试也好维护
                // Convert the entire query to LINQ + ToList() for better debugging and maintenance
                var hits = point
                    .ListCards(false)
                    .Where(card => card.isChara || card.trait.CanBeAttacked)
                    .ToList();

                foreach (Card card2 in hits)
                {
                    // 1) 先缓存下来，方便多次重用
                    // 1) Cache first for multiple reuse
                    Chara target = card2.Chara;

                    // 2) 只有在 target != null 且是友军/以上时，才走"友军"那套流程
                    // 2) Only when target != null and is friend/above, go through the "friendly" process
                    if (target != null && cc.Chara.IsFriendOrAbove(target))
                    {
                        int num7 = cc.Evalue(302);
                        if (!cc.IsPC && cc.IsPCFactionOrMinion)
                            num7 += EClass.pc.Evalue(302);

                        if (num7 > 0)
                        {
                            if (cc.HasElement(1214, 1))
                                num7 *= 2;

                            Debug.Log($"BSSpell: controlLv:{num7 * 10}, dmg:{EClass.rnd(num6 + 1)}");

                            if (num7 * 10 > EClass.rnd(num6 + 1))
                            {
                                if (card2 == card2.pos.FirstChara)
                                    cc.ModExp(302, cc.IsPC ? 10 : 50);
                                continue;   // 友军免伤逻辑：跳过后面的 DamageHP
                                            // Friendly damage immunity logic: skip subsequent DamageHP
                            }
                            else
                            {
                                num6 = EClass.rnd(num6 * 100 / (100 + num7 * 10 + 1));
                                if (card2 == card2.pos.FirstChara)
                                    cc.ModExp(302, cc.IsPC ? 20 : 100);
                            }
                        }
                    }

                    // 3) 不管是不是 Chara，都可以调用 DamageHP
                    // 3) Whether it's Chara or not, DamageHP can be called
                    card2.DamageHP(num6, element.id, power * num2, AttackSource.None, cc, true);

                    // 4) 只有真实的 Chara 且不是玩家派系/仆从时，才调整 hostility
                    // 4) Only adjust hostility for real Chara that are not player faction/minions
                    if (card2.isChara && target != null && !target.IsPCFactionOrMinion)
                    {
                        target.hostility -= 2;
                    }

                    cc.Say("spell_bolt_hit", cc, card2, element.Name.ToLower(), null);
                }
                if (point.HasObj && point.cell.matObj.hardness <= power / 20)
                {
                    EClass._map.MineObj(point, null, null);
                }
            }
        }

        /// <summary>
        /// 搜索目标点
        /// Search target points
        /// </summary>
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

    public static class BSSpellFixTools
    {
        /// <summary>
        /// 修复魔法设置
        /// Fix magic settings
        /// </summary>
        public static void FixMagic()
        {
            SourceManager sources = Core.Instance.sources;
            foreach (string magic in ModInfos.MagicList)
            {
                SourceElement.Row row = sources.elements.rows.FirstOrDefault((SourceElement.Row r) => r.alias == magic);
                if (row == null)
                {
                    Debug.LogError("ES_Fix_Nus".Loc(magic));
                    continue;
                }

                row.value *= (int)BSmagicshop.SBookValue.Value;
                row.cost = new int[1] { (int)((float)row.cost[0] * BSmagicshop.MPcost.Value) };
                row.radius *= (int)BSmagicshop.Sdistance.Value;
                Debug.Log("ES_Fix_Done".Loc(row.alias, row.radius, row.cost[0], row.value));
            }

            sources.elements.Reset();
            sources.elements.Init();
            Debug.Log("ES_Fix_AllDone".Loc());
        }

        /// <summary>
        /// 列出指定半径内的点
        /// List points within specified radius
        /// </summary>
        internal static List<Point> ListPoints(Point center, float radius, Map map, bool ro = false)
        {
            radius = (int)radius;
            HashSet<Point> hashSet = new HashSet<Point> { center.Copy() };
            for (int i = 1; (float)i <= radius; i++)
            {
                Point[] array;
                if (ro)
                {
                    int num = (int)Math.Round((double)i * 0.101);
                    array = new Point[4]
                    {
                    new Point(center.x - num, center.z - num),
                    new Point(center.x + num, center.z - num),
                    new Point(center.x - num, center.z + num),
                    new Point(center.x + num, center.z + num)
                    };
                    foreach (Point point2 in array)
                    {
                        if (IsPointValid(point2))
                        {
                            hashSet.Add(point2);
                        }
                    }

                    continue;
                }

                array = new Point[4]
                {
                new Point(center.x, center.z - i),
                new Point(center.x, center.z + i),
                new Point(center.x - i, center.z),
                new Point(center.x + i, center.z)
                };
                foreach (Point point3 in array)
                {
                    if (IsPointValid(point3))
                    {
                        hashSet.Add(point3);
                    }
                }
            }

            HashSet<Point> hashSet2 = hashSet;
            List<Point> list = new List<Point>(hashSet2.Count);
            list.AddRange(hashSet2);
            return list;
            bool IsPointValid(Point point)
            {
                if (point.x >= 0 && point.x < map.Size && point.z >= 0)
                {
                    return point.z < map.Size;
                }

                return false;
            }
        }
    }

    /// <summary>
    /// BS法术2
    /// BS Spell 2
    /// </summary>
    internal class SpellBS2 : Spell
    {
        public override bool Perform()
        {
            Element element = Act.CC.elements.GetElement(base.source.aliasParent);
            SpellBS2.ProcAt_Internal((element != null) ? element.Value : 1, base.source, base.act);
            int spellExp = Act.CC.Chara.elements.GetSpellExp(Act.CC.Chara, base.act, 100);
            Act.CC.Chara.ModExp(base.source.alias, spellExp);
            return true;
        }

        /// <summary>
        /// 内部处理攻击
        /// Internal attack processing
        /// </summary>
        private static void ProcAt_Internal(int power, SourceElement.Row source, Act act)
        {
            int radius = (int)((double)source.radius * 2 + 0.01 * (double)power);
            Element element = Element.Create(source.aliasRef, power / 10);
            List<Point> list = EClass._map.ListPointsInCircle(Act.TP, radius, true, true);
            list.Remove(Act.CC.pos);
            Act.CC.Chara.Say("spell_ball", Act.CC.Chara, element.Name.ToLower(), null);
            EClass.Wait(0.8f, Act.CC.Chara);
            ActEffect.TryDelay(delegate
            {
                Act.CC.Chara.PlaySound("spell_ball", 1f, true);
            });
            BSAreaSpellEffects.Atk(Act.CC, Act.CC.pos, power / 10, element, list, act, source.alias, 0f, false, "ball_");
        }
    }

    /// <summary>
    /// BS法术3
    /// BS Spell 3
    /// </summary>
    internal class SpellBS3 : Spell
    {
        public override bool Perform()
        {
            Element element = Act.CC.elements.GetElement(base.source.aliasParent);
            SpellBS3.ProcAt_Internal((element != null) ? element.Value : 1, base.source, base.act);
            int spellExp = Act.CC.Chara.elements.GetSpellExp(Act.CC.Chara, base.act, 100);
            Act.CC.Chara.ModExp(base.source.alias, spellExp);
            return true;
        }

        /// <summary>
        /// 内部处理攻击
        /// Internal attack processing
        /// </summary>
        private static void ProcAt_Internal(int power, SourceElement.Row source, Act act)
        {
            
            Element element = Element.Create(source.aliasRef, power / 10);
            int radius = (int)((double)source.radius + 0.01 * (double)power);
            List<Point> list = EClass._map.ListPointsInLine(Act.CC.pos, Act.TP, 1);
            List<Point> list2 = EClass._map.ListPointsInArc(Act.CC.pos, Act.TP, radius, 1f);
            List<Point> list3 = EClass._map.ListPointsInArc(Act.CC.pos, Act.TP, radius, 1f);
            List<Point> list4 = EClass._map.ListPointsInArc(Act.CC.pos, Act.TP, radius, 1f);
            if (list.Count == 0)
            {
                list.Add(Act.TP.Copy());
            }
            if (list2.Count == 0)
            {
                list2.Add(Act.TP.Copy());
            }
            if (list3.Count == 0)
            {
                list3.Add(Act.TP.Copy());
            }
            if (list4.Count == 0)
            {
                list4.Add(Act.TP.Copy());
            }
            list.Remove(Act.CC.pos);
            list2.Remove(Act.CC.pos);
            list3.Remove(Act.CC.pos);
            list4.Remove(Act.CC.pos);
            Act.CC.Chara.Say("spell_ball", Act.CC.Chara, element.Name.ToLower(), null);
            EClass.Wait(0.8f, Act.CC.Chara);
            ActEffect.TryDelay(delegate
            {
                Act.CC.Chara.PlaySound("spell_ball", 1f, true);
            });
            DarknessSpellEffects.Atk(Act.CC, Act.CC.pos, power / 5, element, list, act, source.alias, 0f, true, "hand_");
            DarknessSpellEffects.Atk(Act.CC, Act.CC.pos, power / 5, element, list2, act, source.alias, 0.1f, false, "hand_");
            DarknessSpellEffects.Atk(Act.CC, Act.CC.pos, power / 5, element, list3, act, source.alias, 0f, false, "hand_");
            DarknessSpellEffects.Atk(Act.CC, Act.CC.pos, power / 5, element, list4, act, source.alias, 0.1f, false, "hand_");
        }
    }

    /// <summary>
    /// 卡片信息
    /// Card Information
    /// </summary>
    internal class CardInfo
    {
        public Card card;
        public Point Point;
    }

    /// <summary>
    /// 攻击仇恨处理
    /// Attack Enmity Processing
    /// </summary>
    internal class AtkEnmityProcess
    {       
        /// <summary>
        /// 执行攻击
        /// Perform attack
        /// </summary>
        internal static void Atk(Chara cc, Point tp, int power, Element element, List<Point> targets, Act act, string alias, float exDelay = 0f, bool rev = false, string spellType = "ball_")
        {
            
            int lostHPRatio = (cc.MaxHP - cc.hp) / (int)cc.MaxHP;
            int steps = lostHPRatio * 20; // 20 = 100% / 5%
            int EnmityDMG = (int)(1 + (steps * 0.05f) * 2);
            int num = (Act.powerMod / 100) * EnmityDMG;
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
                        effect = Effect.Get("Element/eleSound");
                    }
                    else
                    {
                        effect = Effect.Get(spellType);
                    }
                }
                else
                {
                    effect = Effect.Get("Element/eleSound");
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
                foreach (Card card2 in from card in point.ListCards(false)
                                       where card.isChara || card.trait.CanBeAttacked
                                       select card)
                {
                    if (cc.Chara.IsFriendOrAbove(card2.Chara))
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
                            Debug.Log(string.Format("BSSpell5: controlLv:{0}, dmg:{1}", num7 * 10, EClass.rnd(num6 + 1)));
                            if (num7 * 10 > EClass.rnd(num6 + 1))
                            {
                                if (card2 == card2.pos.FirstChara)
                                {
                                    cc.ModExp(302, cc.IsPC ? 10 : 50);
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
                    card2.DamageHP(num6, element.id, power * num2, AttackSource.None, cc, true);
                    if (card2.isChara && !card2.Chara.IsPCFactionOrMinion)
                    {
                        card2.Chara.hostility -= 2;
                    }
                    cc.Say("spell_bolt_hit", cc, card2, element.Name.ToLower(), null);
                }
                if (point.HasObj && point.cell.matObj.hardness <= power / 20)
                {
                    EClass._map.MineObj(point, null, null);
                }
            }
        }
    }

    /// <summary>
    /// 法术工具类
    /// Spell Tools Class
    /// </summary>
    public static class SpellTools
    {
        /// <summary>
        /// 列出八星形范围内的点
        /// List points in eight-star shape range
        /// </summary>
        internal static List<Point> ListPointsInEightStar(Point center, float radius, Map map, bool ro = false)
        {
            radius = (int)radius;
            HashSet<Point> hashSet = new HashSet<Point> { center.Copy() };

            for (int i = 1; (float)i <= radius; i++)
            {
                int diagonalOffset = (int)Math.Round(i * 0.707); // 45度偏移量 // 45 degree offset
                Point[] points = new Point[8]
                {
            // 四向正交
            // Four-way orthogonal
            new Point(center.x, center.z - i),    // 上 // Up
            new Point(center.x, center.z + i),    // 下 // Down
            new Point(center.x - i, center.z),    // 左 // Left
            new Point(center.x + i, center.z),    // 右 // Right
            
            // 四向对角
            // Four-way diagonal
            new Point(center.x - diagonalOffset, center.z - diagonalOffset), // 左上 // Top-left
            new Point(center.x + diagonalOffset, center.z - diagonalOffset), // 右上 // Top-right
            new Point(center.x - diagonalOffset, center.z + diagonalOffset), // 左下 // Bottom-left
            new Point(center.x + diagonalOffset, center.z + diagonalOffset)  // 右下 // Bottom-right
                };

                foreach (Point point in points)
                {
                    if (IsPointValid(point))
                    {
                        hashSet.Add(point);
                    }
                }
            }

            return new List<Point>(hashSet);

            bool IsPointValid(Point point)
            {
                return point.x >= 0 && point.x < map.Size &&
                       point.z >= 0 && point.z < map.Size;
            }
        }
    }

    /// <summary>
    /// BS法术5
    /// BS Spell 5
    /// </summary>
    internal class SpellBS5 : Spell
    {
        public override bool Perform()
        {
            Element element = Act.CC.elements.GetElement(base.source.aliasParent);
            SpellBS5.ProcAt_Enmity((element != null) ? element.Value : 1, base.source, base.act);
            int spellExp = Act.CC.Chara.elements.GetSpellExp(Act.CC.Chara, base.act, 100);
            Act.CC.Chara.ModExp(base.source.alias, spellExp);
            return true;
        }

        /// <summary>
        /// 处理仇恨攻击
        /// Process enmity attack
        /// </summary>
        private static void ProcAt_Enmity(int power, SourceElement.Row source, Act act)
        {
            int radius = (int)source.radius;
            Element element = Element.Create(source.aliasRef, power / 10);
            List<Point> list = EClass._map.ListPointsInArc(Act.CC.pos, Act.TP, radius, 1f);
            list.Remove(Act.CC.pos);
            Act.CC.Chara.Say("spell_ball", Act.CC.Chara, element.Name.ToLower(), null);
            EClass.Wait(0.8f, Act.CC.Chara);
            ActEffect.TryDelay(delegate
            {
                Act.CC.Chara.PlaySound("spell_ball", 1f, true);
            });
            AtkEnmityProcess.Atk(Act.CC, Act.CC.pos, power / 10, element, list, act, source.alias, 0f, false, "ball_");
        }
    }

    /// <summary>
    /// BS法术6
    /// BS Spell 6
    /// </summary>
    internal class SpellBS6 : Spell
    {
        public override bool Perform()
        {
            Element element = Act.CC.elements.GetElement(base.source.aliasParent);
            SpellBS6.ProcAt_1((element != null) ? element.Value : 1, base.source, base.act);
            int spellExp = Act.CC.Chara.elements.GetSpellExp(Act.CC.Chara, base.act, 100);
            Act.CC.Chara.ModExp(base.source.alias, spellExp);
            return true;
        }

        /// <summary>
        /// 处理攻击1
        /// Process attack 1
        /// </summary>
        private static void ProcAt_1(int power, SourceElement.Row source, Act act)
        {

            Element element = Element.Create(source.aliasRef, power);
            int radius = (int)source.radius;
            List<Point> list = EClass._map.ListPointsInArc(Act.CC.pos, Act.TP, radius, 1f);
            List<Point> list2 = EClass._map.ListPointsInArc(Act.CC.pos, Act.TP, radius, 1f);
            List<Point> list3 = EClass._map.ListPointsInArc(Act.CC.pos, Act.TP, radius, 1f);
            List<Point> list4 = EClass._map.ListPointsInArc(Act.CC.pos, Act.TP, radius, 1f);
            if (list.Count == 0)
            {
                list.Add(Act.TP.Copy());
            }
            if (list2.Count == 0)
            {
                list2.Add(Act.TP.Copy());
            }
            if (list3.Count == 0)
            {
                list3.Add(Act.TP.Copy());
            }
            if (list4.Count == 0)
            {
                list4.Add(Act.TP.Copy());
            }
            list.Remove(Act.CC.pos);
            list2.Remove(Act.CC.pos);
            list3.Remove(Act.CC.pos);
            list4.Remove(Act.CC.pos);

            Act.CC.Chara.Say("spell_ball", Act.CC.Chara, element.Name.ToLower(), null);
            EClass.Wait(0.8f, Act.CC.Chara);
            ActEffect.TryDelay(delegate
            {
                Act.CC.Chara.PlaySound("spell_breathe", 1f, true);
            });
            BSAreaSpellEffects.Atk(Act.CC, Act.CC.pos, power / 5, element, list, act, source.alias, 0f, true, "ball_");
            BSAreaSpellEffects.Atk(Act.CC, Act.CC.pos, power / 5, element, list2, act, source.alias, 0.5f, true, "ball_");
            BSAreaSpellEffects.Atk(Act.CC, Act.CC.pos, power / 5, element, list3, act, source.alias, 1f, true, "ball_");
            BSAreaSpellEffects.Atk(Act.CC, Act.CC.pos, power / 5, element, list4, act, source.alias, 1.5f, true, "ball_");

        }
    }
}
