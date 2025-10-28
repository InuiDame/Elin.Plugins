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
using Condition_Invincible;

namespace Condition_Phantasmal_Legacy
//Phantasmal Legacy
//ファンタズマル・レガシー
//幻之遗产
{
    public class ConSK2615 : Timebuff
    {
        public ConSK2615() : base() { }  // 默认构造函数 / Default constructor / デフォルトコンストラクタ

        // 带参构造函数 / Parameterized constructor / パラメータ付きコンストラクタ
        public ConSK2615(int power, int duration) : this()
        {
            this.power = power;    // 加成强度（百分比） / Bonus strength (percentage) / ボーナス強度（パーセンテージ）
            this.value = duration; // 剩余回合（Condition 用 value 跟踪时长） / Remaining turns (Condition uses value to track duration) / 残りターン数（Conditionはvalueで持続時間を追跡）
        }

        public override bool CanManualRemove => true;  // 允许手动移除 / Allow manual removal / 手動削除可能
        public override int GetPhase() => 0;           // 固定阶段0 / Fixed phase 0 / 固定フェーズ0

        public override BaseNotification CreateNotification()
        {
            // 创建状态效果通知 / Create status effect notification / 状態効果通知を作成
            return new NotificationBuff { condition = this };
        }

        public override void OnStart()
        {
            // 状态开始时添加重生效果 / Add rebirth effect when condition starts / 状態開始時にリバース効果を追加
            owner.AddCondition<ConRebirth>(1);
            // 添加10回合无敌效果 / Add 10 turns invincible effect / 10ターン無敵効果を追加
            owner.AddCondition<ConGBFInvincible>(10);
        }

        public override void Tick()
        {
            // 每回合持续，power 秒后自动结束 / Duration each turn, automatically ends after power seconds / 毎ターン持続、power秒後に自動終了
            Mod(-1);
        }

        public override void OnWriteNote(List<string> list)
        {
            // 计算实际倍率（已经在 BonusMultiplier 里 clamp 到 2 倍） / Calculate actual multiplier (already clamped to 2x in BonusMultiplier) / 実際の倍率を計算（BonusMultiplierですでに2倍にクランプ済み）
            float cappedMult = Mathf.Min(BonusMultiplier, 2f);
            // 显示为百分比：2 倍 → 200%，1.5 倍 → 150% / Display as percentage: 2x → 200%, 1.5x → 150% / パーセンテージで表示：2倍 → 200%、1.5倍 → 150%
            int displayPct = Mathf.FloorToInt(cappedMult * 100f);
            list.Add($"<color=red>暴击伤害 +{displayPct}%</color>");  // 暴击伤害加成 / Critical damage bonus / クリティカルダメージボーナス
        }

        // 供 Harmony 补丁读取 / For Harmony patch to read / Harmonyパッチが読み取る用
        public float BonusMultiplier => Mathf.Min(1f + power / 100f, 2f);  // 计算加成倍率 / Calculate bonus multiplier / ボーナス倍率を計算
    }

    // 用 Harmony 在 GetRawDamage 运算后插入暴击加成 / Use Harmony to insert critical bonus after GetRawDamage calculation / Harmonyを使用してGetRawDamage計算後にクリティカルボーナスを挿入
    [HarmonyPatch(typeof(AttackProcess), nameof(AttackProcess.GetRawDamage))]
    static class Patch_AttackProcess_GetRawDamage
    {
        static void Postfix(bool crit, AttackProcess __instance, ref long __result)
        {
            if (crit)  // 如果是暴击攻击 / If it's a critical attack / クリティカル攻撃の場合
            {
                var cc = __instance.CC;
                // 检查身上是否有这个 Buff / Check if this Buff is present / このバフが存在するか確認
                var cond = cc.GetCondition<ConSK2615>();
                if (cond != null)
                {
                    // 应用暴击伤害加成 / Apply critical damage bonus / クリティカルダメージボーナスを適用
                    float bonus = Mathf.Min(cond.BonusMultiplier, 2f);
                    __result = Mathf.FloorToInt(__result * bonus);
                }
            }
        }
    }
}
