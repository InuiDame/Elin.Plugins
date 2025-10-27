using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Configuration;
using Cwl.API.Custom;
using Cwl.Helper.Unity;
using Cwl.LangMod;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace Condition_Normal
{

    public class Connatsu_sk2 : Timebuff
// Eat this and cheer up / これ食べて元気出してー / 吃点这个打起精神来
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
    
    public override int GetPhase() => 0;  // 固定为阶段0 / Fixed at phase 0 / フェーズ0で固定
}

public class Connatsu_sk1 : Timebuff
// It's the best thing to do in times like these / こういう時のとっておき / 为了此刻的珍藏
{
    private Dice healDice;           // 一次性治疗骰子 / One-time heal dice / 一回性治療ダイス
    private Dice hotDice;            // 持续治疗骰子 / Heal over time dice / 持続治療ダイス
    private bool diceInitialized;    // 骰子初始化标志 / Dice initialization flag / ダイス初期化フラグ

    // 幂等初始化骰子与首次治疗 / Idempotent dice initialization and first heal / 冪等なダイス初期化と初回治療
    private void InitDice()
    {
        if (diceInitialized) return;
        
        // 1) 计算一次性疗伤骰 / Calculate one-time heal dice / 一回性治療ダイスを計算
        int num = Mathf.Max(EClass.curve(owner.DEX * 10, 400, 100), 100);
        healDice = Dice.Create("SpHealEris", num);
        
        // 2) HOT持续恢复骰 / HOT continuous recovery dice / HOT持続回復ダイス
        hotDice = Dice.Create("SpHOT", power);
        
        // 3) 首次立刻治疗 / First immediate heal / 初回即時治療
        owner.HealHPHost(healDice.Roll(), HealSource.Magic);

        diceInitialized = true;
    }

    public override void OnStart()
    {
        InitDice();              // 初始化骰子 / Initialize dice / ダイスを初期化
        DispelNegative(owner);   // 驱散负面状态 / Dispels negative status / ネガティブ状態を解除
    }

    public override void Tick()
    {
        // 确保骰子已初始化 / Ensure dice are initialized / ダイスが初期化済みか確認
        InitDice();

        // HOT持续恢复 / HOT continuous recovery / HOT持続回復
        if (hotDice != null)
        {
            owner.HealHPHost(hotDice.Roll(), HealSource.HOT);
        }

        Mod(-1);  // 减少持续时间 / Reduce duration / 持続時間を減少
    }

    public override void OnWriteNote(List<string> list)
    {
        // 在状态说明中添加治疗信息 / Add heal info to status description / 状態説明に治療情報を追加
        if (hotDice != null)
        {
            list.Add($"<color=green>Hot +{hotDice}</color>");        // 持续治疗量 / Heal over time amount / 持続治療量
        }
        if (healDice != null)
        {
            list.Add($"<color=green>Heal +{healDice}</color>");      // 单次治疗量 / Single heal amount / 単回治療量
        }
    }
    
    public override int GetPhase() => 0;  // 固定为阶段0 / Fixed at phase 0 / フェーズ0で固定

    // 驱散负面状态效果 / Dispels negative status effects / ネガティブ状態効果を解除
    private void DispelNegative(Chara target)
    {
        var toRemove = new List<Condition>();
        // 收集所有负面状态 / Collect all negative statuses / 全てのネガティブ状態を収集
        foreach (var cond in target.conditions)
            if (cond.Type == ConditionType.Bad || cond.Type == ConditionType.Debuff)
                toRemove.Add(cond);
        
        // 移除所有负面状态 / Remove all negative statuses / 全てのネガティブ状態を削除
        foreach (var cond in toRemove)
            cond.Kill();
    }
}
}
