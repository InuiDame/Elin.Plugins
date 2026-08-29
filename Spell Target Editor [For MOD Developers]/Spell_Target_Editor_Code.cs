using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Spell_Target_Editor.Code
{
    /// 这需要配合Spell类里的Perform中的ActEffect.ProcAt使用，例子为SpellGBF_0533 / This needs to be used with ActEffect.ProcAt in Spell's Perform method, example is SpellGBF_0533 / これはSpellクラスのPerform内のActEffect.ProcAtと連携して使用する必要があり、例はSpellGBF_0533

    [HarmonyPatch(typeof(ActEffect))]
    internal class BuffSelfPatch
    {
        private static readonly HashSet<Card> _selfBuffedCasters = new HashSet<Card>();
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
                bool IsKeyword(string s) => s == "buffself" || s == "bufftarget" ||
                                            s == "buffteam"  || s == "buffall"   ||
                                            s == "bufffriendly";
                Debug.Log($"BuffSelfPatch: 进入补丁, id={id}, n1={actRef.n1}");  // 记录进入补丁 / Log entering patch / パッチ進入を記録
                
                // 检查是否是 buffself 效果 / Check if it's buffself effect / buffself効果か確認
                if (id == EffectId.Buff && actRef.n1 != null && actRef.n1.StartsWith("buffself"))
                {
                    Debug.Log($"BuffSelfPatch: 检测到buffself效果, n1={actRef.n1}");  // 检测到buffself效果 / Detected buffself effect / buffself効果を検出
                    
                    // 解析参数 / Parse parameters / パラメータを解析
                    string[] parameters = actRef.n1.Split(',');
                    var selfBuffNames   = new List<string>();
                    var targetBuffNames = new List<string>();  // 原来的 bufftarget
                    var teamBuffNames   = new List<string>();  // buffteam
                    var allBuffNames    = new List<string>();  // buffall
                    var friendlyBuffNames = new List<string>();// bufffriendly

                    // ---------- 解析参数 ----------
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        string param = parameters[i].Trim();
                        if (param == "buffself")
                        {
                            while (i + 1 < parameters.Length && !IsKeyword(parameters[i + 1].Trim()))
                                selfBuffNames.Add(parameters[++i].Trim());
                        }
                        else if (param == "bufftarget")
                        {
                            while (i + 1 < parameters.Length && !IsKeyword(parameters[i + 1].Trim()))
                                targetBuffNames.Add(parameters[++i].Trim());
                        }
                        else if (param == "buffteam")
                        {
                            while (i + 1 < parameters.Length && !IsKeyword(parameters[i + 1].Trim()))
                                teamBuffNames.Add(parameters[++i].Trim());
                        }
                        else if (param == "buffall")
                        {
                            while (i + 1 < parameters.Length && !IsKeyword(parameters[i + 1].Trim()))
                                allBuffNames.Add(parameters[++i].Trim());
                        }
                        else if (param == "bufffriendly")
                        {
                            while (i + 1 < parameters.Length && !IsKeyword(parameters[i + 1].Trim()))
                                friendlyBuffNames.Add(parameters[++i].Trim());
                        }
                    }

                    // 如果 buffself 没有解析到任何名称，且 aliasEle 存在，则用它作为自身 buff
                    if (selfBuffNames.Count == 0 && !string.IsNullOrEmpty(actRef.aliasEle))
                    {
                        selfBuffNames.Add(actRef.aliasEle);
                    }

                    bool anyAdded = false;

                    // ----- 1. buffself：只对施法者添加一次（群体时只加一次）-----
                    if (cc != null && cc.isChara && selfBuffNames.Count > 0)
                    {
                        if (!_selfBuffedCasters.Contains(cc))
                        {
                            foreach (var name in selfBuffNames)
                            {
                                Condition cond = Condition.Create(name, power);
                                cc.Chara.AddCondition(cond);
                                Debug.Log($"BuffSelfPatch: 给施法者 {cc} 添加自身buff {name}");
                            }
                            _selfBuffedCasters.Add(cc);
                            anyAdded = true;
                        }
                        else
                        {
                            Debug.Log($"BuffSelfPatch: 施法者 {cc} 已添加过自身buff，跳过重复");
                        }
                    }

// ----- 2. bufftarget：原目标（无论敌友）-----
                    if (tc != null && tc.isChara && targetBuffNames.Count > 0)
                    {
                        foreach (var name in targetBuffNames)
                        {
                            Condition cond = Condition.Create(name, power);
                            tc.Chara.AddCondition(cond);
                            Debug.Log($"BuffSelfPatch: 给目标 {tc} 添加buff {name} (bufftarget)");
                        }
                        anyAdded = true;
                    }
                    
                    // ----- 3. buffteam：队友（自身以外的友方）-----
                    if (tc != null && tc.isChara && teamBuffNames.Count > 0)
                    {
                        // 使用游戏内置的 IsFriendOrAbove 判断
                        if (tc != cc && cc.Chara.IsFriendOrAbove(tc.Chara))
                        {
                            foreach (var name in teamBuffNames)
                            {
                                Condition cond = Condition.Create(name, power);
                                tc.Chara.AddCondition(cond);
                                Debug.Log($"BuffSelfPatch: 给队友 {tc} 添加buff {name} (buffteam)");
                            }
                            anyAdded = true;
                        }
                    }
                    
                    // ----- 4. buffall：所有目标（无差别）-----
                    if (tc != null && tc.isChara && allBuffNames.Count > 0)
                    {
                        foreach (var name in allBuffNames)
                        {
                            Condition cond = Condition.Create(name, power);
                            tc.Chara.AddCondition(cond);
                            Debug.Log($"BuffSelfPatch: 给目标 {tc} 添加buff {name} (buffall)");
                        }
                        anyAdded = true;
                    }
                    
                    // ----- 5. bufffriendly：友方（自身以外的友方）-----
                    if (tc != null && tc.isChara && friendlyBuffNames.Count > 0)
                    {
                        // 同样使用 IsFriendOrAbove，并排除自身
                        if (tc != cc && cc.Chara.IsFriendOrAbove(tc.Chara))
                        {
                            foreach (var name in friendlyBuffNames)
                            {
                                Condition cond = Condition.Create(name, power);
                                tc.Chara.AddCondition(cond);
                                Debug.Log($"BuffSelfPatch: 给友方 {tc} 添加buff {name} (bufffriendly)");
                            }
                            anyAdded = true;
                        }
                    }
                    
                    if (anyAdded)
                    {
                        Debug.Log("BuffSelfPatch: 成功处理buff，跳过原方法");
                        return false; // 跳过原方法
                    }
                    else
                    {
                        Debug.LogWarning("BuffSelfPatch: 检测到buffself但没有有效的buff名称或目标");
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
