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
        
        var points1 = _map.ListPointsInLine(CC.pos, TP, 99,false);
        points1 = FilterValidPoints(points1); // 过滤无效点
        
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
            
            // 安全检查：确保 firstT.Key 是有效的
            if (!IsValidPoint(firstT.Key)) continue;
            
            var targets = _map.ListPointsInCircle(firstT.Key, source.radius, false);
            foreach (var nextT in SpellEffects.Search(targets, firstT.Value))
            {
                if (!lineTargets.ContainsKey(nextT.Key) && !Equals(nextT.Key, CC.pos))
                {
                    // 确保 nextT.Key 是有效的
                    if (!IsValidPoint(nextT.Key)) continue;
                    
                    lineTargets.Add(nextT.Key, nextT.Value);
                    queue.Enqueue(nextT);
                    var lines = _map.ListPointsInLine(firstT.Key, nextT.Key, (int)source.radius + 1);
                    
                    lines = FilterValidPoints(lines); // 再次过滤线段点
                    
                    SpellEffects.Atk(CC, firstT.Key, power / Math.Max(nextT.Value / 2, 1), mainElement, lines, act,
                        source.alias, nextT.Value * 0.2f + 0.2f, spellType: "bolt_");
                }
            }
        }
    }
    // 辅助方法：过滤有效点
    private static List<Point> FilterValidPoints(List<Point> points)
    {
        return points.Where(p => IsValidPoint(p)).ToList();
    }

// 辅助方法：检查点是否有效
    private static bool IsValidPoint(Point p)
    {
        return p != null && p.IsValid && p.IsInBounds && p.cell != null;
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

internal class GatherSpell : Spell //魔法: 聚集 / 吸聚
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

        // 聚集范围：以 TP 为中心，半径 2（5×5 范围）
        var radius = 2;
        var targetsArea = _map.ListPointsInCircle(TP, radius, false, false);
        if (targetsArea.Count == 0)
            targetsArea.Add(TP.Copy());

        // 施法表现
        CC.Chara.Say("spell_ball", CC.Chara, mainElement.Name.ToLower());
        Wait(0.8f, CC.Chara);
        ActEffect.TryDelay(() => CC.Chara.PlaySound("spell_ball"));

        var hits = SpellEffects.Search(targetsArea);

        int pulledCount = 0;

        foreach (var targetPair in hits)
        {
            var targetPoint = targetPair.Key;
            var card = targetPoint.FirstChara;

            if (card == null || !card.isChara || card.Chara == null)
                continue;

            var chara = card.Chara;

            // 不影响自己
            if (chara == CC.Chara)
                continue;

            // 302 Control Magic 保护（友军 + 中立NPC 有概率抵抗）
            if (CC.Chara.IsFriendOrAbove(chara) || (chara.hostility == 0 && !chara.IsPCFactionOrMinion))
            {
                var controlLv = CC.Evalue(302);
                if (!CC.IsPC && CC.IsPCFactionOrMinion)
                    controlLv += EClass.pc.Evalue(302);

                if (controlLv > 0)
                {
                    if (CC.HasElement(1214)) controlLv *= 2;

                    if (controlLv * 8 > EClass.rnd(100))   // 抵抗概率，可调整
                    {
                        CC.ModExp(302, CC.IsPC ? 10 : 50);
                        continue;
                    }
                }
            }

            // ================== 核心：把目标拉向 TP ==================
            int pullStrength = Math.Max(power / 12, 3);   // 强度，可微调

            for (int i = 0; i < pullStrength; i++)
            {
                // 正确调用方式：让目标尝试从 TP（中心点）移动 → 实际效果是往 TP 靠拢
                chara.TryMoveFrom(TP);
            }

            pulledCount++;

            // 可选：被拉动时在目标位置显示小特效
            // Effect.Get("Element/ball_Ether").Play(chara.pos);
        }

        // 中心点显示聚集特效
        Effect.Get("Element/ball_Ether").Play(TP);

        if (pulledCount > 0)
        {
            // 如果游戏有对应文本，可以改成合适的；没有就删掉这行
            // CC.Chara.Say("spell_gather_success", CC.Chara);
        }
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

                bool isProtected = false;   // 新增：是否需要走302保护

                if (card.Chara != null)
                {
                    // 原有：明确友军（pets、party 等）
                    if (cc.Chara.IsFriendOrAbove(card.Chara))
                    {
                        isProtected = true;
                    }
                    // 新增：中立 NPC（hostility == 0 且不是敌对）
                    else if (card.Chara.hostility == 0 && !card.Chara.IsPCFactionOrMinion)
                    {
                        isProtected = true;
                    }
                }

                // 如果是受保护目标（友军 或 中立NPC），执行302 Control Magic 逻辑
                if (isProtected)
                {
                    var controlLv = cc.Evalue(302);
                    if (!cc.IsPC && cc.IsPCFactionOrMinion) controlLv += EClass.pc.Evalue(302);
                
                    if (controlLv > 0)
                    {
                        if (cc.HasElement(1214)) controlLv *= 2;   // 可能的部分强化

                        // 概率完全免伤
                        if (controlLv * 10 > EClass.rnd(dmg + 1))
                        {
                            if (card == card.pos.FirstChara)
                                cc.ModExp(302, cc.IsPC ? 10 : 50);
                            continue;   // 跳过伤害
                        }

                        // 否则按比例降低伤害
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
            // 添加安全检查：跳过无效点
            if (attackPoint == null || !attackPoint.IsValid || !attackPoint.IsInBounds || attackPoint.cell == null)
                continue;
            
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