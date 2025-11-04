using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Configuration;
using Condition_IceDragonSeal;
using Condition_Noraml;
using Cwl.API.Custom;
using Cwl.Helper.Unity;
using Cwl.LangMod;
using GBF.Modinfo;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using static NoticeManager;
using static UnityEngine.UI.GridLayoutGroup;

namespace GBF.Patch_PCR_Frozen_Fang_Assault
{
    [HarmonyPatch(typeof(Chara), "UseAbility", new Type[]
    {
        typeof(Act),
        typeof(Card),
        typeof(Point),
        typeof(bool)
    })]
    public class Chara_UseAbility_Patch
    {
        [HarmonyPostfix]
        public static void UseAbility_Postfix(Chara __instance, Act a, bool __result)
        {
            // 检查是否使用了目标技能且技能执行成功 / Check if target skill is used and executed successfully / 対象スキルが使用され、実行成功したかチェック
            if (a?.source?.id == 1700006 && __result)
            {
                // 获取角色实例 / Get character instance / キャラクターインスタンスを取得
                Chara targetChar = __instance;
            
                // 检查是否存在冰龙封印BUFF / Check if Ice Dragon Seal buff exists / 氷龍封印BUFFが存在するかチェック
                if (targetChar.HasCondition<ConIceDragonSeal>())
                {
                    // 获取冰龙封印状态实例 / Get Ice Dragon Seal condition instance / 氷龍封印状態のインスタンスを取得
                    ConIceDragonSeal iceSeal = targetChar.GetCondition<ConIceDragonSeal>();
                
                    if (iceSeal != null)
                    {
                        iceSeal.Mod(+6);
                    
                        // 添加另一个BUFF（持续7回合） / Add another buff (duration 7 turns) / 別のBUFFを追加（持続時間7ターン）
                        //targetChar.AddCondition<ConIceDragonSeal>(iceSeal.value+7);
                    
                        // 或者添加属性加成 / Or add attribute bonus / または属性ボーナスを追加
                        // targetChar.elements.SetBase(属性ID, 加成值);
                    }
                }
            }
            if (a?.source?.id == 1700007 && __result)
            {
                Chara targetChar = __instance;
                if (targetChar.HasCondition<ConIceDragonSeal>())
                {
                    ConIceDragonSeal iceSeal = targetChar.GetCondition<ConIceDragonSeal>();
                
                    if (iceSeal != null)
                    {
                        iceSeal.Mod(-1);
                        targetChar.AddCondition<ConSnow_Boots>(iceSeal.value);

                    }
                }
            }
            if (a?.source?.id == 1700008 && __result)
            {
                Chara targetChar = __instance;
                if (targetChar.HasCondition<ConIceDragonSeal>())
                {
                    ConIceDragonSeal iceSeal = targetChar.GetCondition<ConIceDragonSeal>();
                
                    if (iceSeal != null)
                    {
                        iceSeal.Mod(-1);
                    }
                }
            }
        }
    }
}
