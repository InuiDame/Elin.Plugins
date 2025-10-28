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

namespace Condition_Invincible
{
    public class ConGBFInvincible : Timebuff
    {
        public ConGBFInvincible() : base() { }  // 默认构造函数 / Default constructor / デフォルトコンストラクタ

        // 带参构造，方便手动 AddCondition(duration) / Parameterized constructor for manual AddCondition(duration) / 手動AddCondition(duration)用のパラメータ付きコンストラクタ
        public ConGBFInvincible(int duration) : this()
        {
            this.value = duration; // value是剩余回合数 / value is remaining turns / valueは残りターン数
            this.power = power;    // 设置效果强度 / Set effect strength / 効果強度を設定
        }

        // 指定合法相位，避免基类越界 / Specify legal phase to avoid base class out of bounds / 基本クラスの範囲外を避けるため合法フェーズを指定
        public override int GetPhase() => 0;

        // 不允许手动移除 / Not allowed to manually remove / 手動削除不可
        public override bool CanManualRemove => false;

        // 图标和提示 / Icon and prompt / アイコンとプロンプト
        public override BaseNotification CreateNotification()
            => new NotificationBuff { condition = this };

        // 开始时/或每回合逻辑，这里简单倒计时 / Start/turn logic, simple countdown here / 開始時/ターン毎のロジック、ここでは単純なカウントダウン
        public override void OnStart() { }
        public override void Tick() => Mod(-1);  // 每回合减少持续时间 / Reduce duration each turn / 毎ターン持続時間を減少

        // 状态栏文字 / Status bar text / ステータスバーテキスト
        public override void OnWriteNote(List<string> list)
        {
            // 显示"无敌（剩X回合）" / Display "Invincible (X turns left)" / 「無敵（残りXターン）」を表示
            list.Add($"<color=cyan>无敌（剩{value}回合）</color>");
        }
    }
}
