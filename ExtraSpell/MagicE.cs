using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ExtraSpell;

internal class StarSpell : Spell //魔法: 星光
{
    public override bool Perform()
    {
        var power = CC.elements.GetElement(source.aliasParent)?.Value ?? 1;
        ProcAt_Internal(power, source, act);
        var spellExp = CC.Chara.elements.GetSpellExp(CC.Chara, act);
        CC.Chara.ModExp(source.alias, spellExp);
        return true;
    }

    private static void ProcAt_Internal(int power, SourceElement.Row source, Act act)
    {
        var mainElement = Create(source.aliasRef, power / 10);
        var ro = CC.pos.x == TP.x || CC.pos.z == TP.z;
        var points1 = _map.ListPointsInCircle(TP, source.radius, false, false);
        var points2 = SpellTools.ListPointsInStar(TP, source.radius * 2, _map, ro);
        if (points1.Count == 0)
            points1.Add(TP.Copy());
        if (points2.Count == 0)
            points2.Add(TP.Copy());

        CC.Chara.Say("spell_ball", CC.Chara, mainElement.Name.ToLower());
        Wait(0.8f, CC.Chara);
        ActEffect.TryDelay(() => CC.Chara.PlaySound("spell_ball"));
        SpellEffects.Atk(CC, TP, power / 10, mainElement, points1, act, source.alias, 0f, true);
        SpellEffects.Atk(CC, TP, power, mainElement, points2, act, source.alias, 0.4f);
    }
}

internal class LightningSpell : Spell //魔法: 雷击
{
    public override bool Perform()
    {
        var power = CC.elements.GetElement(source.aliasParent)?.Value ?? 1;
        ProcAt_Internal(power, source, act);
        var spellExp = CC.Chara.elements.GetSpellExp(CC.Chara, act);
        CC.Chara.ModExp(source.alias, spellExp);
        return true;
    }

    private static void ProcAt_Internal(int power, SourceElement.Row source, Act act)
    {
        var mainElement = Create(source.aliasRef, power / 10);
        var points1 = _map.ListPointsInLine(CC.pos, TP, 99);
        points1.Remove(CC.pos);
        CC.Chara.Say("spell_bolt", CC.Chara, mainElement.Name.ToLower());
        Wait(0.8f, CC.Chara);
        ActEffect.TryDelay(() => CC.Chara.PlaySound("spell_bolt"));
        var lineTargets = SpellEffects.Search(points1);
        SpellEffects.Atk(CC, TP, power, mainElement, points1, act, source.alias, spellType: "bolt_");
        var queue = new Queue<KeyValuePair<Point, int>>(lineTargets);
        while (queue.Count > 0)
        {
            var firstT = queue.Dequeue();
            var targets = _map.ListPointsInCircle(firstT.Key, source.radius, false);
            foreach (var nextT in SpellEffects.Search(targets, firstT.Value))
            {
                if (!lineTargets.ContainsKey(nextT.Key) && !Equals(nextT.Key, CC.pos))
                {
                    lineTargets.Add(nextT.Key, nextT.Value);
                    queue.Enqueue(nextT);
                    var lines = _map.ListPointsInLine(firstT.Key, nextT.Key, (int)source.radius + 1);
                    SpellEffects.Atk(CC, firstT.Key, power / Math.Max(nextT.Value / 2, 1), mainElement, lines, act,
                        source.alias, nextT.Value * 0.2f + 0.2f, spellType: "bolt_");
                }
            }
        }
    }
}

internal class BlowbackSpell : Spell //魔法: 吹飞
{
    public override bool Perform()
    {
        var power = CC.elements.GetElement(source.aliasParent)?.Value ?? 1;
        ProcAt_Internal(power, source, act);
        var spellExp = CC.Chara.elements.GetSpellExp(CC.Chara, act);
        CC.Chara.ModExp(source.alias, spellExp);
        return true;
    }

    private static void ProcAt_Internal(int power, SourceElement.Row source, Act act)
    {
        var radius = (int)(source.radius + 0.01 * power);
        var mainElement = Create(source.aliasRef, power / 10);
        var points1 = _map.ListPointsInArc(CC.pos, TP, radius, 50);
        points1.Remove(CC.pos);

        CC.Chara.Say("spell_ball", CC.Chara, mainElement.Name.ToLower());
        Wait(0.8f, CC.Chara);
        ActEffect.TryDelay(() => CC.Chara.PlaySound("spell_ball"));
        SpellEffects.Atk(CC, CC.pos, power / 10, mainElement, points1, act, source.alias);

        SpellEffects.Push(CC, points1, power / 20);
    }
}

internal static class SpellEffects //效果
{
    internal static void Atk(Chara cc, Point tp, int power, Element element,
        List<Point> targets, Act act, string alias,
        float exDelay = 0f, bool rev = false, string spellType = "ball_")
    {
        // 默认威力修正
        var powerMod = Act.powerMod / 100f;
        // eleP修正，不可用为 50
        var elementPowerMod = (act?.ElementPowerMod ?? 50) / 50;

        // 骰子名
        var diceAlias = alias ?? "ball_";
        var dice = Dice.Create(diceAlias, power, cc, act);
        if (dice == null && alias != null)
        {
            var idx = alias.IndexOf('_');
            diceAlias = (idx > 0 ? alias.Substring(0, idx) : alias) + "_";
            dice = Dice.Create(diceAlias, power, cc, act);
        }

        // Effect路径
        string effectPath;
        switch (spellType)
        {
            case "ball_":
            case "bolt_":
                effectPath = "Element/ball_" + (element.id == 0 ? "Void" : element.source.alias.Remove(0, 3));
                break;
            case "hit_light":
            case "aura_heaven":
                effectPath = spellType;
                break;
            default:
                effectPath = "Element/ball_Ether";
                break;
        }

        foreach (var attackPoint in targets)
        {
            var dis = Math.Max(tp.Distance(attackPoint), 1);
            var effect = Effect.Get(effectPath);
            var delay = rev ? 0.25f / dis : 0.04f * dis;
            effect.SetStartDelay(delay + exDelay);
            if (spellType == "ball_")
                effect.Play(attackPoint).Flip(attackPoint.x > tp.x);
            else
                effect.Play(attackPoint);

            var randomDamage = dice.Roll();
            var dmg = (int)(powerMod * randomDamage / (0.1f * (9 + dis)));

            var hits = attackPoint
                .ListCards(false)
                .Where(card => card.isChara || card.trait.CanBeAttacked)
                .ToList();
            
            foreach (var card in hits)
            {

                if (card.Chara != null && cc.Chara.IsFriendOrAbove(card.Chara))
                {
                    var controlLv = cc.Evalue(302);
                    if (!cc.IsPC && cc.IsPCFactionOrMinion)
                        controlLv += EClass.pc.Evalue(302);
                    if (controlLv > 0)
                    {
                        if (cc.HasElement(1214)) controlLv *= 2;
                        if (controlLv * 10 > EClass.rnd(dmg + 1))
                        {
                            if (card == card.pos.FirstChara)
                                cc.ModExp(302, cc.IsPC ? 10 : 50);
                            continue;
                        }
                        dmg = EClass.rnd(dmg * 100 / (100 + controlLv * 10 + 1));
                        if (card == card.pos.FirstChara)
                            cc.ModExp(302, cc.IsPC ? 20 : 100);
                    }
                }

                card.DamageHP(dmg, element.id, power * elementPowerMod, origin: cc);
                if (card.isChara && card.Chara != null && !card.Chara.IsPCFactionOrMinion)
                {
                    card.Chara.hostility -= 2;
                }

                cc.Say("spell_bolt_hit", cc, card, element.Name.ToLower());
            }

            if (attackPoint.HasObj && attackPoint.cell.matObj.hardness <= power / 20)
            {
                EClass._map.MineObj(attackPoint);
            }
        }
    }

    internal static void Push(Chara cc, List<Point> targets, int er)
    {
        var minPush = Math.Max(er, 2);
        foreach (var attackPoint in targets)
        {
            var hits = attackPoint
                .ListCards(false)
                .Where(card => card.isChara)
                .ToList();
            
            foreach (var card in hits)
            {
                if (!card.isChara) continue;
                for (var i = 0; i < minPush; i++)
                {
                    card.Chara.TryMoveFrom(cc.pos);
                }
            }
        }
    }

    internal static Dictionary<Point, int> Search(List<Point> targets, int num = 0)
    {
        // 容量
        var refPoints = new Dictionary<Point, int>(targets.Count * 2);
        foreach (var attackPoint in targets)
        {
            var validCards = attackPoint
                .ListCards(false)
                .Where(card => card.isChara || card.trait.CanBeAttacked);
            
            foreach (var card in validCards)
            {
                if (!refPoints.ContainsKey(card.pos))
                {
                    refPoints.Add(card.pos, ++num);
                }
            }
        }
        return refPoints;
    }
}