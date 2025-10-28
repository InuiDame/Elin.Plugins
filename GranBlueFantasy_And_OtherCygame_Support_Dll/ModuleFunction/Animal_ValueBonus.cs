using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace GBF.ModuleFunction.Animal_ValueBonus
{ 
    /// 使用示例为野性的觉醒，和机械强化类似，会检查是否是动物类种族，然后给予加成，这个加成不会写在UI中，需要自己适配。
    /// Usage example is Feral Rage, similar to Mechanical Enhancement, it checks if the race is animal type, then grants bonuses. These bonuses are not displayed in the UI and require manual adaptation.
    /// 使用例は野性の覚醒で、機械強化と同様に、動物系種族かどうかをチェックし、ボーナスを付与します。このボーナスはUIに表示されず、自分で適応する必要があります。
    [HarmonyPatch(typeof(ElementContainerCard), "ValueBonus")]
public static class ElementContainerCard_ValueBonus_Patch
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
            bool isAnimal = chara.IsAnimal;  // 检查是否为动物种族 / Check if animal race / 動物種族かチェック
            bool isHellhound = __instance.owner.id == "inui_hellhound";  // 检查是否为地狱犬角色 / Check if hellhound character / ヘルハウンドキャラかチェック

            // 应用野性的觉醒加成 / Apply Feral Rage bonus / 野性の覚醒ボーナスを適用
            if (e.id != 664 && (isAnimal || isHellhound))
            {
                int customElementId = 170029;  // 野性的觉醒元素ID / Feral Rage element ID / 野性の覚醒エレメントID

                // 使用直接的元素值获取，避免递归 / Use direct element value acquisition to avoid recursion / 再帰を避けるために直接エレメント値を取得
                int customElementValue = GetElementValueSafely(__instance.owner, customElementId);

                if (customElementValue > 0)
                {
                    int baseValue = e.ValueWithoutLink + e.vLink;
                    switch (e.id)
                    {
                        case 70:  
                        case 71:  
                            originalBonus += (int)(baseValue * customElementValue * 0.0115f);  // 1.15%加成 / 1.15% bonus / 1.15%ボーナス
                            break;
                        case 79:  
                            originalBonus += baseValue * customElementValue / 100;  // 1%加成 / 1% bonus / 1%ボーナス
                            break;
                        case 916: 
                            originalBonus += (int)(baseValue * customElementValue * 0.013f);  // 1.3%加成 / 1.3% bonus / 1.3%ボーナス
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
