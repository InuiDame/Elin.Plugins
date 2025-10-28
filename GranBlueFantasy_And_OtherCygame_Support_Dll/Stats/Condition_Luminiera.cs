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

namespace Condition_Luminiera
{
    public class ConGBFLuminiera1 : Condition
    // 光の刃 / Holy Blade / 光之刃
    {
        public override int GetPhase()
        {
            return 0;  // 固定阶段0 / Fixed phase 0 / 固定フェーズ0
        }

        public override void Tick()
        {
            // 限制最大层数为10 / Limit maximum stacks to 10 / 最大スタック数を10に制限
            if (base.value > 10)
            {
                base.value = 10;
            }

            // 基于STR属性计算效果强度 / Calculate effect strength based on STR attribute / STR属性に基づいて効果強度を計算
            int p = Mathf.Max(EClass.curve(this.owner.STR * 10, 400, 100, 75), 100);

            // 50%概率触发效果 / 50% chance to trigger effect / 50%の確率で効果発動
            if (EClass.rnd(2) == 0)
            {
                // 添加光之刃效果2 / Add Holy Blade effect 2 / 光の刃効果2を追加
                owner.AddCondition<ConGBFLuminiera2>(p, false);
            }

            Mod(-1);  // 减少持续时间 / Reduce duration / 持続時間を減少
        }
    }
    
    public class ConGBFLuminiera2 : Timebuff
    // 光之刃（对敌方自身效果） / Holy Blade (effect on enemy self) / 光の刃（敵自身への効果）
    {
        public override bool UseElements => false;        // 不使用元素系统 / Does not use element system / エレメントシステムを使用しない
        public override bool CanManualRemove => true;     // 允许手动移除 / Can be manually removed / 手動で削除可能

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
