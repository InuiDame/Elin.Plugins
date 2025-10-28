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

namespace Condition_Attack_Count_Increase
//Attack Count Increase
//攻击回数增加
{
    public class ConGBFStat6156 : Timebuff
    {
        public override bool AllowMultipleInstance => false; // 不允许多个实例 / Multiple instances not allowed / 複数インスタンス不可
        public override bool SyncRide => true;               // 同步骑乘状态 / Sync ride status / 騎乗状態を同期

        public virtual int GetExtraAttacks()
        {
            return 1; // 额外攻击1次 / Extra attack 1 time / 追加攻撃1回
        }
    }
    
     [HarmonyPatch]
    public class DoubleStrikePatches
    {

        // 近战攻击 - 修复无限递归 / Melee Attack - Fix Infinite Recursion / 近接攻撃 - 無限再帰を修正
[HarmonyPatch(typeof(ActMelee), "Attack")]
static class MeleeAttackPatches
{
    private static bool isExecutingExtraAttack = false;  // 防止递归执行的标志 / Flag to prevent recursive execution / 再帰実行を防止するフラグ

    [HarmonyPostfix]
    static void MeleeAttackPostfix(ActMelee __instance, bool __result)
    {
        try
        {
            // 如果正在执行额外攻击，直接返回避免递归 / If executing extra attack, return directly to avoid recursion / 追加攻撃実行中の場合、直接戻って再帰を回避
            if (isExecutingExtraAttack) return;

            if (!__result) return;  // 原始攻击失败则返回 / Return if original attack failed / 元の攻撃が失敗した場合は戻る

            var cc = Act.CC;
            if (cc == null) return;

            var doubleStrike = cc.GetCondition<ConGBFStat6156>();  // 检查连击效果 / Check double strike effect / 連撃効果をチェック
            if (doubleStrike != null)
            {
                int extraAttacks = doubleStrike.GetExtraAttacks();  // 获取额外攻击次数 / Get extra attack count / 追加攻撃回数を取得

                // 设置标志并执行额外攻击 / Set flag and execute extra attacks / フラグを設定して追加攻撃を実行
                isExecutingExtraAttack = true;
                try
                {
                    ExecuteExtraMeleeAttacks(__instance, extraAttacks);
                }
                finally
                {
                    isExecutingExtraAttack = false;  // 确保标志被重置 / Ensure flag is reset / フラグがリセットされることを保証
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"MeleeAttackPostfix 异常: {ex.Message}");  // 记录异常 / Log exception / 例外を記録
            isExecutingExtraAttack = false;
        }
    }

    // 执行额外近战攻击 / Execute extra melee attacks / 追加近接攻撃を実行
    static void ExecuteExtraMeleeAttacks(ActMelee instance, int extraAttacks)
    {
        try
        {
            var originalTC = Act.TC;        // 保存原始目标 / Save original target / 元のターゲットを保存
            var originalTP = Act.TP.Copy(); // 保存原始目标点 / Save original target point / 元のターゲットポイントを保存
            var cc = Act.CC;

            for (int i = 0; i < extraAttacks; i++)
            {
                // 检查施法者和目标是否存活 / Check if caster and target are alive / キャスターとターゲットが生存しているか確認
                if (!cc.IsAliveInCurrentZone || originalTC == null || !originalTC.IsAliveInCurrentZone)
                {
                    break;
                }

                // 恢复原始目标和位置 / Restore original target and position / 元のターゲットと位置を復元
                Act.TC = originalTC;
                Act.TP.Set(originalTP);

                // 直接调用攻击方法 / Directly call attack method / 直接攻撃メソッドを呼び出す
                var attackMethod = AccessTools.Method(typeof(ActMelee), "Attack", new Type[] { typeof(float) });
                attackMethod?.Invoke(instance, new object[] { 1f });
            }

            // 恢复原始状态 / Restore original state / 元の状態を復元
            Act.TC = originalTC;
            Act.TP.Set(originalTP);
        }
        catch (Exception ex)
        {
            Debug.Log($"ExecuteExtraMeleeAttacks 异常: {ex.Message}");  // 记录异常 / Log exception / 例外を記録
        }
    }
}



        // 直接在 Perform 方法中修改攻击次数 / Directly modify attack count in Perform method / Performメソッドで直接攻撃回数を変更
[HarmonyPatch(typeof(AttackProcess))]
static class AttackProcessPerformPatches
{
    [HarmonyPrefix]
    [HarmonyPatch("Perform")]
    static void PerformPrefix(AttackProcess __instance, ref int __state, int count)
    {
        try
        {
            if (__instance.CC == null) return;

            var doubleStrike = __instance.CC.GetCondition<ConGBFStat6156>();  // 检查连击效果 / Check double strike effect / 連撃効果をチェック
            if (doubleStrike != null && __instance.IsRanged)  // 仅对远程攻击生效 / Only works for ranged attacks / 遠距離攻撃のみ有効
            {
                int extraAttacks = doubleStrike.GetExtraAttacks();  // 获取额外攻击次数 / Get extra attack count / 追加攻撃回数を取得
                __state = extraAttacks + 1; // 存储倍数到 __state / Store multiplier to __state / 倍数を__stateに保存

                // 如果是远程攻击且是第一次攻击，修改numFire / If ranged attack and first attack, modify numFire / 遠距離攻撃で最初の攻撃の場合、numFireを変更
                if (count == 0)
                {
                    var numFireField = AccessTools.Field(typeof(AttackProcess), "numFire");  // 获取攻击次数字段 / Get attack count field / 攻撃回数字段を取得
                    var numFireWithoutDamageLossField = AccessTools.Field(typeof(AttackProcess), "numFireWithoutDamageLoss");  // 获取无伤害衰减攻击次数字段 / Get no damage loss attack count field / ダメージ減衰なし攻撃回数字段を取得

                    if (numFireField != null && numFireWithoutDamageLossField != null)
                    {
                        int currentNumFire = (int)numFireField.GetValue(__instance);  // 当前攻击次数 / Current attack count / 現在の攻撃回数
                        int currentNumFireWithoutDamageLoss = (int)numFireWithoutDamageLossField.GetValue(__instance);  // 当前无伤害衰减攻击次数 / Current no damage loss attack count / 現在のダメージ減衰なし攻撃回数

                        // 乘以攻击倍数 / Multiply by attack multiplier / 攻撃倍数を乗算
                        numFireField.SetValue(__instance, currentNumFire * __state);
                        numFireWithoutDamageLossField.SetValue(__instance, currentNumFireWithoutDamageLoss * __state);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"AttackProcess PerformPrefix 异常: {ex.Message}");  // 记录异常 / Log exception / 例外を記録
            __state = 1;  // 出错时恢复默认值 / Restore default value on error / エラー時にデフォルト値を復元
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch("Perform")]
    static void PerformPostfix(AttackProcess __instance, int __state, int count, bool __result)
    {
        try
        {
            // 如果有攻击倍数且是第一次攻击且攻击成功 / If has attack multiplier and first attack and attack successful / 攻撃倍数があり、最初の攻撃で攻撃成功の場合
            if (__state > 1 && count == 0 && __result)
            {
                // 恢复原始值（如果需要） / Restore original values (if needed) / 元の値を復元（必要なら）
                // 注意：由于AttackProcess是单例，可能不需要恢复，因为每次Prepare都会重置 / Note: Since AttackProcess is singleton, may not need to restore as each Prepare will reset / 注意：AttackProcessはシングルトンのため、各Prepareでリセットされるので復元不要かも
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"AttackProcess PerformPostfix 异常: {ex.Message}");  // 记录异常 / Log exception / 例外を記録
        }
    }
}

        // 法术攻击 - 修改咏唱次数 / Spell Attack - Modify Cast Count / 魔法攻撃 - 詠唱回数を変更
[HarmonyPatch(typeof(Chara), "UseAbility", new Type[]
{
    typeof(Act),
    typeof(Card),
    typeof(Point),
    typeof(bool)
})]
[HarmonyTranspiler]
static IEnumerable<CodeInstruction> UseAbilityTranspiler(IEnumerable<CodeInstruction> instructions)
{
    var codes = new List<CodeInstruction>(instructions);
    bool foundNum3Assignment = false;  // 标记是否找到num3赋值 / Flag if num3 assignment found / num3代入が見つかったかマーク
    bool foundNum3Usage = false;       // 标记是否找到num3使用 / Flag if num3 usage found / num3使用が見つかったかマーク

    for (int i = 0; i < codes.Count; i++)
    {
        // 查找 num3 = 1 的赋值位置 / Find assignment location of num3 = 1 / num3 = 1 の代入位置を検索
        if (!foundNum3Assignment &&
            codes[i].opcode == OpCodes.Ldc_I4_1 &&
            i + 1 < codes.Count &&
            codes[i + 1].opcode == OpCodes.Stloc_S)
        {
            // 在 num3 = 1 之后插入我们的乘法逻辑 / Insert our multiplication logic after num3 = 1 / num3 = 1 の後に乗算ロジックを挿入
            yield return codes[i]; // Ldc_I4_1
            yield return codes[i + 1]; // Stloc_S (存储到num3) / Stloc_S (store to num3) / Stloc_S (num3に保存)

            // 插入：num3 = num3 * (extraAttacks + 1) / Insert: num3 = num3 * (extraAttacks + 1) / 挿入：num3 = num3 * (extraAttacks + 1)
            yield return new CodeInstruction(OpCodes.Ldloc_S, codes[i + 1].operand); // 加载num3 / Load num3 / num3をロード
            yield return new CodeInstruction(OpCodes.Ldarg_0); // 加载this (Chara实例) / Load this (Chara instance) / thisをロード (Charaインスタンス)
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DoubleStrikePatches), "GetSpellMultiplier"));
            yield return new CodeInstruction(OpCodes.Mul);
            yield return new CodeInstruction(OpCodes.Stloc_S, codes[i + 1].operand); // 存回num3 / Store back to num3 / num3に戻して保存

            i++; // 跳过下一条指令，因为我们已经处理了 / Skip next instruction as we've processed it / 次の命令をスキップ（既に処理済み）
            foundNum3Assignment = true;
            continue;
        }

        // 查找快速射击逻辑的位置 (if (a.CanRapidFire && HasElement(1648))) / Find rapid fire logic location (if (a.CanRapidFire && HasElement(1648))) / ラピッド射撃ロジックの位置を検索 (if (a.CanRapidFire && HasElement(1648)))
        if (!foundNum3Usage &&
            codes[i].opcode == OpCodes.Call &&
            codes[i].operand.ToString().Contains("HasElement") &&
            i > 2 && i + 2 < codes.Count)
        {
            // 检查是否是快速射击的逻辑 / Check if it's rapid fire logic / ラピッド射撃のロジックか確認
            var prevInstruction = codes[i - 1];
            if (prevInstruction.opcode == OpCodes.Ldc_I4 && prevInstruction.operand.ToString() == "1648")
            {
                // 在快速射击逻辑之后插入我们的双重打击逻辑 / Insert our double strike logic after rapid fire logic / ラピッド射撃ロジックの後に二重打撃ロジックを挿入
                for (int j = i; j < Math.Min(i + 10, codes.Count); j++)
                {
                    yield return codes[j];

                    // 找到存储回num3的位置 / Find location storing back to num3 / num3に戻して保存する位置を見つける
                    if (codes[j].opcode == OpCodes.Stloc_S && codes[j].operand.ToString().Contains("num3"))
                    {
                        // 插入双重打击乘法逻辑 / Insert double strike multiplication logic / 二重打撃乗算ロジックを挿入
                        yield return new CodeInstruction(OpCodes.Ldloc_S, codes[j].operand); // 加载num3 / Load num3 / num3をロード
                        yield return new CodeInstruction(OpCodes.Ldarg_0); // 加载this (Chara实例) / Load this (Chara instance) / thisをロード (Charaインスタンス)
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DoubleStrikePatches), "GetSpellMultiplier"));
                        yield return new CodeInstruction(OpCodes.Mul);
                        yield return new CodeInstruction(OpCodes.Stloc_S, codes[j].operand); // 存回num3 / Store back to num3 / num3に戻して保存

                        foundNum3Usage = true;
                    }
                }
                i = Math.Min(i + 10, codes.Count - 1);
                continue;
            }
        }

        yield return codes[i];
    }
}

// 辅助方法 - 获取法术多重施法倍数 / Helper method - Get spell multi-cast multiplier / 補助メソッド - 魔法多重詠唱倍数を取得
public static int GetSpellMultiplier(Chara cc)
{
    try
    {
        var doubleStrike = cc?.GetCondition<ConGBFStat6156>();  // 检查连击效果 / Check double strike effect / 連撃効果をチェック
        if (doubleStrike != null)
        {
            int multiplier = doubleStrike.GetExtraAttacks() + 1;  // 计算倍数 / Calculate multiplier / 倍数を計算
            return multiplier;
        }
        return 1;  // 默认倍数1 / Default multiplier 1 / デフォルト倍数1
    }
    catch (Exception ex)
    {
        Debug.Log($"GetSpellMultiplier 异常: {ex.Message}");  // 记录异常 / Log exception / 例外を記録
        return 1;
    }
}

    }
}
