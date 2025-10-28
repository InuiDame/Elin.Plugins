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

namespace GBF.Patch_ActEffect_Proc_Modify_the_usage_method_of_Buff_to_select_multiple_targets
{
    /// 这需要配合Spell类里的Perform中的ActEffect.ProcAt使用，例子为SpellGBF_0533 / This needs to be used with ActEffect.ProcAt in Spell's Perform method, example is SpellGBF_0533 / これはSpellクラスのPerform内のActEffect.ProcAtと連携して使用する必要があり、例はSpellGBF_0533

    [HarmonyPatch(typeof(ActEffect))]
    internal class BuffSelfPatch
    {
        [HarmonyPatch(nameof(ActEffect.Proc), new Type[] { 
            typeof(EffectId), typeof(int), typeof(BlessedState), typeof(Card), typeof(Card), typeof(ActRef) 
        })]
        [HarmonyPrefix]
        internal static bool HandleBuffSelf(
            EffectId id, 
            int power, 
            BlessedState state, 
            Card cc, 
            Card tc, 
            ActRef actRef)
        {
            try
            {
                Debug.Log($"BuffSelfPatch: 进入补丁, id={id}, n1={actRef.n1}");  // 记录进入补丁 / Log entering patch / パッチ進入を記録
                
                // 检查是否是 buffself 效果 / Check if it's buffself effect / buffself効果か確認
                if (id == EffectId.Buff && actRef.n1 != null && actRef.n1.StartsWith("buffself"))
                {
                    Debug.Log($"BuffSelfPatch: 检测到buffself效果, n1={actRef.n1}");  // 检测到buffself效果 / Detected buffself effect / buffself効果を検出
                    
                    // 解析参数 / Parse parameters / パラメータを解析
                    string[] parameters = actRef.n1.Split(',');
                    string selfBuffName = null;    // 自身buff名称 / Self buff name / 自身バフ名
                    string targetBuffName = null;  // 目标buff名称 / Target buff name / ターゲットバフ名

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        string param = parameters[i].Trim();
                        if (param == "buffself" && i + 1 < parameters.Length)
                        {
                            selfBuffName = parameters[i + 1].Trim();
                            // 检查是否是有效的buff名称（不是bufftarget等关键字） / Check if valid buff name (not keywords like bufftarget) / 有効なバフ名か確認（bufftargetなどのキーワードでない）
                            if (!string.IsNullOrEmpty(selfBuffName) && !selfBuffName.StartsWith("buff"))
                            {
                                i++;
                            }
                            else
                            {
                                selfBuffName = null;
                            }
                        }
                        else if (param == "bufftarget" && i + 1 < parameters.Length)
                        {
                            targetBuffName = parameters[i + 1].Trim();
                            if (!string.IsNullOrEmpty(targetBuffName) && !targetBuffName.StartsWith("buff"))
                            {
                                i++;
                            }
                            else
                            {
                                targetBuffName = null;
                            }
                        }
                    }

                    // 如果没有解析到buff名称，尝试使用 aliasEle 作为备选 / If no buff name parsed, try using aliasEle as alternative / バフ名が解析されない場合、aliasEleを代替として使用
                    if (string.IsNullOrEmpty(selfBuffName) && !string.IsNullOrEmpty(actRef.aliasEle))
                    {
                        selfBuffName = actRef.aliasEle;
                        Debug.Log($"BuffSelfPatch: 使用aliasEle作为buff名称: {selfBuffName}");  // 使用备选名称 / Using alternative name / 代替名を使用
                    }

                    bool handled = false;

                    // 给施法者自己添加buff / Add buff to caster self / キャスター自身にバフを追加
                    if (cc != null && cc.isChara && !string.IsNullOrEmpty(selfBuffName))
                    {
                        Debug.Log($"BuffSelfPatch: 给施法者添加buff: {selfBuffName}");
                        Condition selfCondition = Condition.Create(selfBuffName, power);  // 创建自身buff / Create self buff / 自身バフを作成
                        cc.Chara.AddCondition(selfCondition);
                        handled = true;
                    }

                    // 给目标添加buff / Add buff to target / ターゲットにバフを追加
                    if (tc != null && tc.isChara && !string.IsNullOrEmpty(targetBuffName))
                    {
                        Debug.Log($"BuffSelfPatch: 给目标添加buff: {targetBuffName}");
                        Condition targetCondition = Condition.Create(targetBuffName, power);  // 创建目标buff / Create target buff / ターゲットバフを作成
                        tc.Chara.AddCondition(targetCondition);
                        handled = true;
                    }

                    if (handled)
                    {
                        Debug.Log("BuffSelfPatch: 成功处理buff，跳过原方法");  // 成功处理，跳过原方法 / Successfully handled, skip original method / 成功処理、元のメソッドをスキップ
                        return false; // 跳过原方法 / Skip original method / 元のメソッドをスキップ
                    }
                    else
                    {
                        Debug.LogWarning("BuffSelfPatch: 检测到buffself但没有找到有效的buff名称");  // 警告：未找到有效名称 / Warning: No valid name found / 警告：有効な名前が見つからない
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"BuffSelfPatch 错误: {ex}");  // 记录错误 / Log error / エラーを記録
            }

            return true;  // 继续执行原方法 / Continue with original method / 元のメソッドを継続実行
        }
    }
}
