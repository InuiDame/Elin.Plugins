using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using Cwl;
using Cwl.API.Custom;
using Cwl.API.Processors;
using Cwl.Helper;
using Cwl.LangMod;
using SpellWrite;
using HarmonyLib;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using UnityEngine;


namespace Condition_Normal
{
    public class ConGFInvincible : BaseBuff
    //Force shield
    //パワーフィールドシールド
    //力场盾
    {
    }

    public class ConGFMp5mods : BaseBuff
    //Virtualization trench
    //虚化塹壕
    //虚化战壕
    {
    }

    public class ConGFTPScrqbd : Condition
    //Super Popular Rankings
    //超人気ランキング
    //超人气榜单
    {
     public override bool UseElements => true;  // 使用元素系统 / Uses element system / エレメントシステムを使用

     public override bool CanManualRemove => true;  // 允许手动移除 / Can be manually removed / 手動で削除可能

     public override BaseNotification CreateNotification()
     {
        // 创建状态效果通知 / Create status effect notification / 状態効果通知を作成
        return new NotificationBuff
        {
            condition = this
        };
     }

     public override void OnChangePhase(int lastPhase, int newPhase)
     {
        // 阶段变化时设置不同的元素效果 / Set different element effects when phase changes / フェーズ変更時に異なるエレメント効果を設定
        switch (newPhase)
        {
            case 0:
                // 阶段0：设置基础元素效果 / Phase 0: Set basic element effects / フェーズ0：基本エレメント効果を設定
                elements.SetBase(140007, 20);
                elements.SetBase(66, 30);
                elements.SetBase(92, 100);
                break;
            case 1:
                // 阶段1：增加额外元素效果 / Phase 1: Add additional element effects / フェーズ1：追加エレメント効果を追加
                elements.SetBase(140007, 20);
                elements.SetBase(66, 30);
                elements.SetBase(92, 100);
                elements.SetBase(55, 10);
                break;
            case 2:
                // 阶段2：进一步增强元素效果 / Phase 2: Further enhance element effects / フェーズ2：エレメント効果をさらに強化
                elements.SetBase(140007, 20);
                elements.SetBase(66, 30);
                elements.SetBase(92, 100);
                elements.SetBase(55, 50);
                elements.SetBase(56, 40);
                break;
        }
     }

     public override void Tick()
     {
        // 每帧执行的效果 / Effect executed every frame / 毎フレーム実行される効果
        
        // 基于力量计算效果强度 / Calculate effect strength based on STR / STRに基づいて効果強度を計算
        int num = Mathf.Max(EClass.curve(owner.STR * 10, 400, 100), 100);
        
        // 对范围内敌对角色施加负面状态 / Apply negative status to hostile characters in range / 範囲内の敵対キャラにネガティブ状態を付与
        foreach (Chara chara in owner.pos.ListCharasInRadius(
            owner,
            10,  // 10格范围 / 10 tile range / 10タイル範囲
            c => c.IsHostile()  // 只选择敌对目标 / Only select hostile targets / 敵対ターゲットのみ選択
        ))
        {
            if (chara != owner)  // 排除自身 / Exclude self / 自身を除外
            {
                // 施加虚弱状态 / Apply weakness condition / 虚弱状態を付与
                chara.AddCondition<ConWeakness>(num);
                // 施加诅咒状态 / Apply bane condition / 呪い状態を付与
                chara.AddCondition<ConBane>(num);
            }
        }

        // 减少条件持续时间 / Reduce condition duration / 状態持続時間を減少
        Mod(-1);
     }
    }
}
