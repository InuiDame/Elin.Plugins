using System;
using System.Collections;
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
using static NoticeManager;
using static UnityEngine.UI.GridLayoutGroup;


namespace Condition_Aura
{
    public class ConDispelAura : Timebuff
    {
        public override bool UseElements => false;        // 不使用元素系统 / Does not use element system / エレメントシステムを使用しない
        public override bool CanManualRemove => true;     // 允许手动移除 / Allow manual removal / 手動削除可能

        public override BaseNotification CreateNotification()
        {
            return new NotificationBuff
            {
                condition = this
            };
        }

        public override void Tick()
        {
            // 为自身驱散负面效果 / Dispels negative effects for self / 自身のネガティブ効果を解除
            DispelNegative(owner);

            // 为范围内友军驱散负面效果 / Dispels negative effects for allies in range / 範囲内の味方のネガティブ効果を解除
            foreach (var ally in owner.pos.ListCharasInRadius(owner, 10, c => !c.IsHostile()))
                if (ally != owner)
                    DispelNegative(ally);

            // 减少持续时间 / Reduce duration / 持続時間を減少
            if (value > 0)
                Mod(-1);
            else
                Kill();
        }

        public override int GetPhase() => 0;

        private void DispelNegative(Chara target)
        {
            var toRemove = new List<Condition>();
            foreach (var cond in target.conditions)
                if (cond.Type == ConditionType.Bad || cond.Type == ConditionType.Debuff)
                    toRemove.Add(cond);
            foreach (var cond in toRemove)
                cond.Kill();
        }
    }

    public class ConDispelAura2 : Timebuff
    {
        public override bool UseElements => false;        // 不使用元素系统 / Does not use element system / エレメントシステムを使用しない
        public override bool CanManualRemove => true;     // 允许手动移除 / Allow manual removal / 手動削除可能

        public override BaseNotification CreateNotification()
        {
            return new NotificationBuff
            {
                condition = this
            };
        }

        public override void Tick()
        {
            // 为范围内敌对目标驱散负面效果 / Dispels negative effects for hostile targets in range / 範囲内の敵対ターゲットのネガティブ効果を解除
            foreach (Chara target in owner.pos.ListCharasInRadius(
                owner,
                10,
                c => c.IsHostile()
            ))
            {
                DispelNegative(target);
            }

            // 减少持续时间 / Reduce duration / 持続時間を減少
            if (value > 0)
                Mod(-1);
            else
                Kill();
        }

        public override int GetPhase() => 0;

        private void DispelNegative(Chara target)
        {
            var toRemove = new List<Condition>();
            foreach (var cond in target.conditions)
                if (cond.Type == ConditionType.Bad || cond.Type == ConditionType.Debuff)
                    toRemove.Add(cond);
            foreach (var cond in toRemove)
                cond.Kill();
        }
    }
}

