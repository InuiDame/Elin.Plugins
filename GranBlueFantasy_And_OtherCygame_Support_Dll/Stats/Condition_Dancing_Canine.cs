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

namespace Condition_Dancing_Canine
//犬神战舞 / 犬神戦舞 / Dancing Canine
{
    public class ConSK2744 : Timebuff
    {
        public Chara caster;  // 手动保存施法者 / Manually save the caster / 手動でキャスターを保存

        public override void OnStart()
        {
            base.OnStart();

            // 记录buff开始，使用ToString()来显示角色信息 / Log buff start, use ToString() to display character info / バフ開始を記録、ToString()でキャラ情報を表示

            // 确定要清除冷却的目标角色 / Determine target character to clear cooldown / クールダウンをクリアするターゲットキャラを決定
            Chara target = caster ?? owner;  // 优先使用施法者，否则使用所有者 / Prefer caster, otherwise use owner / キャスターを優先、それ以外は所有者を使用

            ClearCooldown(target, 170031);  // 清除技能170031的冷却 / Clear cooldown for skill 170031 / スキル170031のクールダウンをクリア
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

            if (who._cooldowns.Count == 0)
            {
                return;
            }
            

            // 记录当前所有的冷却项（用于调试） / Log all current cooldown entries (for debugging) / 現在の全てのクールダウン項目を記録（デバッグ用）
            for (int i = 0; i < who._cooldowns.Count; i++)
            {
                int cooldownId = who._cooldowns[i] / 1000;  // 提取技能ID / Extract skill ID / スキルIDを抽出

            }

            // 清除指定技能的冷却 / Clear cooldown for specified skill / 指定スキルのクールダウンをクリア
            int removedCount = 0;
            for (int i = who._cooldowns.Count - 1; i >= 0; i--)
            {
                if (who._cooldowns[i] / 1000 == id)  // 匹配技能ID / Match skill ID / スキルIDをマッチ
                {
                    who._cooldowns.RemoveAt(i);  // 移除冷却项 / Remove cooldown entry / クールダウン項目を削除
                    removedCount++;
                }
            }
            

            // 如果冷却列表为空，设置为null / If cooldown list is empty, set to null / クールダウンリストが空の場合、nullに設定
            if (who._cooldowns.Count == 0)
            {
                who._cooldowns = null;
            }
            else
            {
                UnityEngine.Debug.Log($"[ConSK2744] After clearing - character has {who._cooldowns.Count} cooldown entries remaining");
            }
        }
    }
}
