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

namespace Condition_Princess_Assault
{
    public class ConPCRPrincess_Assault : Timebuff
{
    /// <summary>
    /// バフ開始時の処理 / Buff start processing / Buff开始时的处理
    /// </summary>
    public override void OnStart()
    {
        // 現在のHP比率を計算 / Calculate current HP ratio / 计算当前生命值比例
        float HPRatio = owner.hp / (float)owner.MaxHP;
        // 5%ごとの段階を計算（20 = 100% / 5%） / Calculate steps per 5% (20 = 100% / 5%) / 计算每5%的阶梯数（20 = 100% / 5%）
        int steps = Mathf.FloorToInt(HPRatio * 20);
        // ダメージ倍率を計算（基本1 + 段階数×0.05） / Calculate damage multiplier (base 1 + steps × 0.05) / 计算伤害倍率（基础1 + 阶梯数×0.05）
        float damageMultiplier = 1 + steps * 0.05f;
        
        // 67: ダメージを設定（基本値50 × 倍率） / Set damage (base 50 × multiplier) / 设置伤害（基础值50 × 倍率）
        owner.elements.ModBase(67, (int)(damageMultiplier * 50));
        // 75: 意志を設定（基本値50 × 倍率） / Set will (base 50 × multiplier) / 设置意志（基础值50 × 倍率）
        owner.elements.ModBase(75, (int)(damageMultiplier * 50));
    }
    
    /// <summary>
    /// バフ持続中の各ティック処理 / Each tick processing during buff duration / Buff持续期间的每个tick处理
    /// </summary>
    public override void Tick()
    {
        // 現在のHP比率を計算 / Calculate current HP ratio / 计算当前生命值比例
        float HPRatio = owner.hp / (float)owner.MaxHP;
        // 5%ごとの段階を計算（20 = 100% / 5%） / Calculate steps per 5% (20 = 100% / 5%) / 计算每5%的阶梯数（20 = 100% / 5%）
        int steps = Mathf.FloorToInt(HPRatio * 20);
        // ダメージ倍率を計算（基本1 + 段階数×0.05） / Calculate damage multiplier (base 1 + steps × 0.05) / 计算伤害倍率（基础1 + 阶梯数×0.05）
        float damageMultiplier = 1 + steps * 0.05f;
        
        // 67: ダメージを設定（基本値50 × 倍率） / Set damage (base 50 × multiplier) / 设置伤害（基础值50 × 倍率）
        owner.elements.ModBase(67, (int)(damageMultiplier * 50));
        // 75: 意志を設定（基本値50 × 倍率） / Set will (base 50 × multiplier) / 设置意志（基础值50 × 倍率）
        owner.elements.ModBase(75, (int)(damageMultiplier * 50));
        
        // バフ持続時間を1減少 / Reduce buff duration by 1 / 减少buff持续时间1
        Mod(-1);
    }

    /// <summary>
    /// バフのフェーズを返す / Return buff phase / 返回buff阶段
    /// </summary>
    /// <returns>フェーズ0 / Phase 0 / 阶段0</returns>
    public override int GetPhase() => 0;
}
}
