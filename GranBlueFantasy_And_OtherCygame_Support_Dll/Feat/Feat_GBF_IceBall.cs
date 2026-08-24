using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace GBF.feat.Feat_GBF_IceBall;

// 专长对应的标记元素 ID（请替换为你实际分配且不冲突的 ID）
public static class IceBallElements
{
    public const int IceCastleBall = 170059;    // 冰城舞会的标记元素
    public const int StayByYourSide = 170060;   // 惟愿常伴您身旁的标记元素
}

// ==================== 状态定义 ====================

/// <summary>
/// 冰姬的舞踏：每层永久存在，用 value 记录层数，达到4时转化为欧罗巴城。
/// 重写 Tick 阻止自动减少，因此 value 永不下降。
/// </summary>
public class ConIcePrincessDance : Timebuff
{
    public override void Tick()
    {
        // 保持 value 不变，永不过期
    }

    public override void OnStart()
    {
        base.OnStart();
        // 每次获得新“层”时检查是否达到4
        if (value >= 4)
        {
            owner.RemoveCondition<ConIcePrincessDance>();
            owner.AddCondition<ConEuropa>(3, true);
        }
    }
}

/// <summary>
/// 欧罗巴城：持续3回合，获得时清除所有技能冷却，存在期间正面buff不减少。
/// </summary>
public class ConEuropa : Timebuff
{
    public override void OnStart()
    {
        base.OnStart();
        ClearAllCooldowns(owner);
    }

    private void ClearAllCooldowns(Chara who)
    {
        if (who?._cooldowns == null || who._cooldowns.Count == 0)
            return;
        who._cooldowns.Clear();
        who._cooldowns = null;
    }
}

// ==================== 专长定义 ====================

internal class FeatIceCastleBall : Feat
{
    internal void _OnApply(int add, ElementContainer eleOwner, bool hint)
    {
        // 这里可以按需修改属性
    }

    public override Sprite GetIcon(string suffix = "")
    {
        return SpriteSheet.Get(source.alias);
    }
}

internal class FeatStayByYourSide : Feat
{
    internal void _OnApply(int add, ElementContainer eleOwner, bool hint)
    {
        // 属性加成
    }

    public override Sprite GetIcon(string suffix = "")
    {
        return SpriteSheet.Get(source.alias);
    }
}

// ==================== 每5回合叠一层冰姬舞踏 ====================

[HarmonyPatch(typeof(Chara), "Tick")]
public static class CharaTick_IceDance
{
    private static readonly Dictionary<Chara, int> counters = new Dictionary<Chara, int>();

    public static void Postfix(Chara __instance)
    {
        try
        {
            if (__instance == null || !__instance.HasElement(IceBallElements.IceCastleBall))
                return;
            if (__instance.HasCondition<ConEuropa>())
                return;

            if (!counters.ContainsKey(__instance))
                counters[__instance] = 0;
            counters[__instance]++;

            if (counters[__instance] >= 5)
            {
                counters[__instance] = 0;
                __instance.AddCondition<ConIcePrincessDance>(1, true);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[IceBall] CharaTick error: {ex}");
        }
    }
}

// ==================== 欧罗巴城期间正面buff冻结 ====================

[HarmonyPatch(typeof(Condition), "Tick", new Type[] { })]
public static class TimebuffTick_Freeze
{
    public static bool Prefix(Condition __instance)
    {
        try
        {
            Chara owner = __instance.owner;
            if (owner == null)
                return true;
            // 拥有欧罗巴城且当前状态是 Buff 类型，则跳过 Tick，冻结持续时间
            if (owner.HasCondition<ConEuropa>() && __instance.Type == ConditionType.Buff)
                return false;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[IceBall] TimebuffTick error: {ex}");
        }
        return true;
    }
}

// ==================== 惟愿常伴您身旁：追击 ====================

[HarmonyPatch(typeof(Card), "DamageHP", new Type[] { typeof(long), typeof(int), typeof(int), typeof(AttackSource), typeof(Card), typeof(bool), typeof(Thing), typeof(Chara), typeof(int) })]
public static class CardDamageHP_Pursuit
{
    private static bool pursuitLock = false;

    public static void Postfix(Card __instance, Card origin, Chara originalTarget, int ele)
    {
        try
        {
            if (pursuitLock) return;
            if (origin == null || origin.Chara != EClass.pc) return;

            Chara target = originalTarget ?? __instance.Chara;
            if (target == null) return;
            if (EClass.pc.party?.members == null) return;

            foreach (Chara member in EClass.pc.party.members)
            {
                if (member == EClass.pc || !member.HasElement(IceBallElements.StayByYourSide)) continue;
                if (!member.HasCondition<ConEuropa>()) continue;

                // ?!追击?!
                long pursuitDmg = member.Evalue(10) * 2;

                pursuitLock = true;
                try
                {
                    target.DamageHP(pursuitDmg, 0, 0, AttackSource.Condition, member, false, null, target);
                }
                finally
                {
                    pursuitLock = false;
                }
                break;
            }
        }
        catch (Exception ex)
        {
            pursuitLock = false;
            Debug.LogError($"[IceBall] Pursuit error: {ex}");
        }
    }
}