using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace GBF.ModuleFunction.Eleventh_Treat
{ 
    /// <summary>
    /// Copy with Animal_ValueBonus
    /// </summary>
    [HarmonyPatch(typeof(ElementContainerCard), "ValueBonus")]
public static class Eleventh_Treat_ValueBonus_Patch
{
    private static bool isProcessing = false; // 防止递归的标志 / Flag to prevent recursion / 再帰を防止するフラグ

    public static void Postfix(ElementContainerCard __instance, Element e, ref int __result)
    {
        // 防止递归调用 / Prevent recursive calls / 再帰呼び出しを防止
        if (isProcessing) return;

        try
        {
            isProcessing = true;

            // 基础null检查 / Basic null checks / 基本的なnullチェック
            if (__instance?.owner?.Chara == null || e == null)
                return;

            var chara = __instance.owner.Chara;

            // 简单的状态检查 / Simple status check / 簡単な状態チェック
            if (chara.isDead)
                return;

            int originalBonus = __result;

            // 使用安全的条件判断 / Use safe condition judgment / 安全な条件判断を使用
            bool isVajra = __instance.owner.id == "Vajra";  
            
            if (isVajra)
            {
                int customElementId = 170049;  

                // 使用直接的元素值获取，避免递归 / Use direct element value acquisition to avoid recursion / 再帰を避けるために直接エレメント値を取得
                int customElementValue = GetElementValueSafely(__instance.owner, customElementId);

                if (customElementValue > 0)
                {
                    // 固定值加成 / Fixed value bonus / 固定値ボーナス
                    int fixedBonus = customElementValue; // 或者根据需要调整固定值的计算方式
            
                    switch (e.id)
                    {
                        case 70:                              
                        case 71:                              
                            originalBonus += fixedBonus * 10; // 每点元素值增加10点71
                            break;
                    }
                }
            }

            __result = originalBonus;  // 更新最终结果 / Update final result / 最終結果を更新
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Error in ValueBonus patch: {ex}");  // 错误日志 / Error log / エラーログ
        }
        finally
        {
            isProcessing = false;  // 确保标志被重置 / Ensure flag is reset / フラグがリセットされることを保証
        }
    }

    // 安全的元素值获取方法，避免递归 / Safe element value acquisition method to avoid recursion / 再帰を避ける安全なエレメント値取得メソッド
    private static int GetElementValueSafely(Card owner, int elementId)
    {
        if (owner?.elements == null) return 0;

        // 直接访问基础值，避免调用Value属性（会触发ValueBonus） / Directly access base value, avoid calling Value property (would trigger ValueBonus) / 基本値に直接アクセス、Valueプロパティの呼び出しを回避（ValueBonusをトリガーするため）
        var element = owner.elements.GetElement(elementId);
        if (element == null) return 0;

        return element.ValueWithoutLink + element.vLink;  // 返回基础值加链接值 / Return base value plus link value / 基本値＋リンク値を返す
    }
}
}
