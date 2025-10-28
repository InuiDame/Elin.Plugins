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
using Condition_Aura;

namespace Condition_Total_Eclipse
//Total Eclipse / トータル・エクリプス / 全蚀
{
    public class ConSK2612_2 : Timebuff
    {
        public override int GetPhase() => 0;  // 固定阶段0 / Fixed phase 0 / 固定フェーズ0
    }

    public class ConSK2612 : Timebuff
    {
        public override bool UseElements => false;    // 不使用元素系统 / Does not use element system / エレメントシステムを使用しない
        public override bool CanManualRemove => true; // 允许手动移除 / Allow manual removal / 手動削除可能

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
            // 基于DEX属性计算效果强度 / Calculate effect strength based on DEX attribute / DEX属性に基づいて効果強度を計算
            int num = Mathf.Max(EClass.curve(owner.DEX * 10, 400, 100), 100);
            
            // 为范围内友军添加效果 / Add effects to allies in range / 範囲内の味方に効果を追加
            foreach (Chara chara in owner.pos.ListCharasInRadius(
                owner,
                10,                      // 10格范围 / 10 tile range / 10タイル範囲
                c => !owner.IsHostile(c) // 选择非敌对目标（友军） / Select non-hostile targets (allies) / 非敵対ターゲット（味方）を選択
            ))
            {
                if (chara != owner)  // 排除自身 / Exclude self / 自身を除外
                {
                    // 添加全蚀效果2 / Add Total Eclipse effect 2 / トータル・エクリプス効果2を追加
                    chara.AddCondition<ConSK2612_2>(num);
                    // 添加驱散光环效果 / Add dispel aura effect / ディスペルオーラ効果を追加
                    chara.AddCondition<ConDispelAura2>(num);
                }
            }

            Mod(-1);  // 减少持续时间 / Reduce duration / 持続時間を減少
        }
        public override int GetPhase() => 0;  // 固定阶段0 / Fixed phase 0 / 固定フェーズ0
    }
}
