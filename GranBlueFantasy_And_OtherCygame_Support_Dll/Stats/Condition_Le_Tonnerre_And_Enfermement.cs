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

namespace Condition_Le_Tonnerre_And_Enfermement
//Le Tonnerre & Enfermement
//アンフェルモン & ル・トネール
//封禁 和 闪雷
//这俩没啥突出的放一起了 / These two have nothing outstanding, put them together / これら2つは特に目立たないので一緒にしました
{
    public class ConSK2700 : Timebuff
    {
        public override bool UseElements => false;        // 不使用元素系统 / Does not use element system / エレメントシステムを使用しない
        public override bool CanManualRemove => true;     // 允许手动移除 / Allow manual removal / 手動削除可能

        public override BaseNotification CreateNotification()
        {
            // 创建状态效果通知 / Create status effect notification / 状態効果通知を作成
            return new NotificationBuff
            {
                condition = this
            };
        }

        public override void Tick()
        {
            // 基于END属性计算效果强度 / Calculate effect strength based on END attribute / END属性に基づいて効果強度を計算
            int num = Mathf.Max(EClass.curve(owner.END * 10, 400, 100), 100);
            // 为范围内友军添加效果 / Add effects to allies in range / 範囲内の味方に効果を追加
            foreach (Chara chara in owner.pos.ListCharasInRadius(
                owner,
                10,  // 10格范围 / 10 tile range / 10タイル範囲
                c => !owner.IsHostile(c)  // 选择非敌对目标（友军） / Select non-hostile targets (allies) / 非敵対ターゲット（味方）を選択
            ))
            {
                if (chara != owner)  // 排除自身 / Exclude self / 自身を除外
                {
                    // 添加封禁效果2 / Add Enfermement effect 2 / アンフェルモン効果2を追加
                    chara.AddCondition<ConSK2700_2>(num);
                }
            }

            Mod(-1);  // 减少持续时间 / Reduce duration / 持続時間を減少
        }
        public override int GetPhase() => 0;  // 固定阶段0 / Fixed phase 0 / 固定フェーズ0
    }

    public class ConSK2700_2 : Timebuff
    {
        public override int GetPhase() => 0;  // 固定阶段0 / Fixed phase 0 / 固定フェーズ0
    }

    public class ConSK2701 : Timebuff
    {
        public override bool UseElements => false;        // 不使用元素系统 / Does not use element system / エレメントシステムを使用しない
        public override bool CanManualRemove => true;     // 允许手动移除 / Allow manual removal / 手動削除可能

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
