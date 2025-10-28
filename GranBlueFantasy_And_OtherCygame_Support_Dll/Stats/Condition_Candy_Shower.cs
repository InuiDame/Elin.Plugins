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
using Condition_Attack_Count_Increase;

namespace Condition_Candy_Shower
//Candy Shower / 戌躍甘撒 / 戌跃甘撒
{
    public class ConSK2745 : Timebuff
    {
        public Chara caster;  // 施法者角色 / Caster character / キャスターキャラクター

        public override void OnStart()
        {
            base.OnStart();
            // 确定目标角色（优先施法者，否则所有者） / Determine target character (prefer caster, otherwise owner) / ターゲットキャラを決定（キャスター優先、それ以外は所有者）
            Chara target = caster ?? owner;
            // 清除技能170030的冷却 / Clear cooldown for skill 170030 / スキル170030のクールダウンをクリア
            ClearCooldown(target, 170030);
            // 清除技能170031的冷却 / Clear cooldown for skill 170031 / スキル170031のクールダウンをクリア
            ClearCooldown(target, 170031);
            // 添加属性加成效果 / Add attribute bonus effect / 属性ボーナス効果を追加
            target.AddCondition<ConGBFStat6156>(1, false);
        }

        // 清除指定技能的冷却 / Clear cooldown for specified skill / 指定スキルのクールダウンをクリア
        private void ClearCooldown(Chara who, int id)
        {
            // 检查角色是否有效 / Check if character is valid / キャラが有効か確認
            if (who == null)
            {
                return;
            }

            // 检查冷却列表是否存在 / Check if cooldown list exists / クールダウンリストが存在するか確認
            if (who._cooldowns == null)
            {
                return;
            }

            // 检查冷却列表是否为空 / Check if cooldown list is empty / クールダウンリストが空か確認
            if (who._cooldowns.Count == 0)
            {
                return;
            }
            
            // 遍历冷却项（调试用循环） / Iterate through cooldown items (debug loop) / クールダウン項目を巡回（デバッグ用ループ）
            for (int i = 0; i < who._cooldowns.Count; i++)
            {
                int cooldownId = who._cooldowns[i] / 1000;  // 提取技能ID / Extract skill ID / スキルIDを抽出
            }
            
            // 移除指定技能的冷却项 / Remove cooldown items for specified skill / 指定スキルのクールダウン項目を削除
            int removedCount = 0;
            for (int i = who._cooldowns.Count - 1; i >= 0; i--)
            {
                if (who._cooldowns[i] / 1000 == id)  // 匹配技能ID / Match skill ID / スキルIDをマッチ
                {
                    who._cooldowns.RemoveAt(i);  // 移除冷却项 / Remove cooldown entry / クールダウン項目を削除
                    removedCount++;
                }
            }
            
            // 如果冷却列表为空则设为null / Set to null if cooldown list is empty / クールダウンリストが空の場合nullに設定
            if (who._cooldowns.Count == 0)
            {
                who._cooldowns = null;
            }
        }
    }
}
