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

namespace Condition_Jinx_of_the_Tiger
//Jinx of the Tiger / 悪運散虎 / 恶运散虎
{
    public class ConSK1640 : Timebuff
    {
        public override bool UseElements => false;    // 不使用元素系统 / Does not use element system / エレメントシステムを使用しない
        public override bool CanManualRemove => true; // 允许手动移除 / Can be manually removed / 手動で削除可能

        public override BaseNotification CreateNotification()
        {
            // 创建状态效果通知 / Create status effect notification / 状態効果通知を作成
            return new NotificationBuff
            {
                condition = this
            };
        }

        // 驱散增益效果 / Dispels buff effects / バフ効果を解除
        private void DispelBuff(Chara target)
        {
            var toRemove = new List<Condition>();
            // 收集所有增益类型状态 / Collect all buff type conditions / 全てのバフタイプ状態を収集
            foreach (var cond in target.conditions)
                if (cond.Type == ConditionType.Buff)
                    toRemove.Add(cond);
            // 移除所有增益效果 / Remove all buff effects / 全てのバフ効果を削除
            foreach (var cond in toRemove)
                cond.Kill();
        }

        public override void Tick()
        {
            // 每帧驱散增益效果 / Dispels buffs every frame / 毎フレームバフを解除
            DispelBuff(owner);

            Mod(-1);  // 减少持续时间 / Reduce duration / 持続時間を減少
        }
        public override int GetPhase() => 0;  // 固定阶段0 / Fixed phase 0 / 固定フェーズ0
    }
}
