using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using BepInEx;
using Cwl.Helper.Unity;
using HarmonyLib;
using UnityEngine;
using Debug = UnityEngine.Debug;



namespace InuiBackAttack.spell
{
    [HarmonyPatch(typeof(AttackProcess))]
    [HarmonyPatch("GetWeaponEnc")]
    public static class BackstabEncPatch
    {
        static void Postfix(Chara CC, Thing w, int ele, bool addSelfEnc, ref int __result)
        {
            // 只在查询背刺元素(999901)时处理
            if (ele == 999901)
            {
                bool hasBackstabElement = false;
            
            // 1. 首先检查武器
            if (w != null)
            {
                var weaponBackstabElement = w.elements.dict.Values.FirstOrDefault(e => e.id == 999901);
                if (weaponBackstabElement != null && weaponBackstabElement.Value > 0)
                {
                    hasBackstabElement = true;
                    //Debug.Log($"武器 {w.id} 有999901元素: 等级{weaponBackstabElement.Value}");
                }
            }

                // 2. 如果武器没有，且 addSelfEnc=true，检查角色
                if (!hasBackstabElement && addSelfEnc)
                {
                    var charaBackstabElement = CC.elements.dict.Values.FirstOrDefault(e => e.id == 999901);
                    if (charaBackstabElement != null && charaBackstabElement.Value > 0)
                    {
                        hasBackstabElement = true;
                        //Debug.Log($"角色 {CC.id} 有999901元素: 等级{charaBackstabElement.Value}");
                    }
                }

                // 3. 如果有背刺元素，进行背面判断
                if (hasBackstabElement)
                {
                    __result = GetBackstabLevel(CC, w);
                    //Debug.Log($"有背刺元素，返回背刺等级: {__result}");
                }
                else
                {
                    __result = 0;
                    //Debug.Log($"武器和角色都没有999901元素，返回0");
                }
                //Debug.Log($"=== 背刺检查结束 ===\n");
            }
        }

        private static int GetBackstabLevel(Chara attacker, Thing weapon)
        {
            try
            {
                // 获取当前攻击的目标
                var currentAttack = AttackProcess.Current;
                if (currentAttack?.TC == null || !currentAttack.TC.isChara)
                    return 0;

                Chara target = currentAttack.TC.Chara;
                if (target == null)
                    return 0;

                // 判断是否为背面攻击
                if (BackstabUtils.IsBackAttack(attacker, target))
                {
                    //Debug.Log($"背刺条件满足! 攻击者: {attacker.id}, 目标: {target.id}");
                    return 100; // 返回背刺等级
                }
            }
            catch (Exception e)
            {
                //Debug.LogError($"背刺等级计算错误: {e}");
            }
            return 0;
        }

        
    }

    [HarmonyPatch(typeof(ActMelee))]
    [HarmonyPatch("Attack", new Type[] { typeof(float) })]
    public static class BackstabDamagePatch
    {
        [HarmonyPrefix]
        static void Prefix(ref float dmgMulti)
        {
            try
            {
                if (Act.CC == null || Act.TC == null || !Act.TC.isChara)
                    return;


                // 检查是否有背刺元素（武器或角色）
                bool hasBackstabElement = CheckBackstabElement(Act.CC);


                // 只有武器有999901元素时才检查背面攻击
                if (hasBackstabElement && BackstabUtils.IsBackAttack(Act.CC, Act.TC.Chara))
                {
                    float backstabMultiplier = 1.5f;
                    dmgMulti *= backstabMultiplier;

                    //Debug.Log($"背刺触发! 倍率: {backstabMultiplier}, 最终伤害倍率: {dmgMulti}");

                    // 背刺特效
                    Act.CC.Say("Spell_BeiCi", Act.CC, Act.TC);
                    Act.TC.PlaySound("critical");
                }
                else if (!hasBackstabElement)
                {
                    //Debug.Log("武器和角色都没有999901元素，不触发背刺");
                }
            }
            catch (Exception e)
            {
                //Debug.LogError($"背刺伤害补丁错误: {e}");
            }
        }
        private static bool CheckBackstabElement(Chara chara)
        {
            try
            {
                //Debug.Log($"=== 检查背刺元素 ===");

                // 检查角色元素 - 使用正确的访问方式
                if (chara.elements != null && chara.elements.dict != null)
                {
                    //Debug.Log($"角色元素数量: {chara.elements.dict.Count}");
                    foreach (var element in chara.elements.dict.Values)
                    {
                        //Debug.Log($"角色元素: ID={element.id}, 值={element.Value}");
                        if (element.id == 999901 && element.Value > 0)
                        {
                            //Debug.Log($"角色 {chara.id} 有999901元素: 等级{element.Value}");
                            return true;
                        }
                    }
                }
                else
                {
                    //Debug.Log("角色元素字典为null");
                }

                // 检查当前武器元素
                Thing weapon = GetCurrentWeapon();
                if (weapon != null)
                {
                    if (weapon.elements != null && weapon.elements.dict != null)
                    {
                        foreach (var element in weapon.elements.dict.Values)
                        {
                            //Debug.Log($"武器元素: ID={element.id}, 值={element.Value}");
                            if (element.id == 999901 && element.Value > 0)
                            {
                                //Debug.Log($"武器 {weapon.id} 有999901元素: 等级{element.Value}");
                                return true;
                            }
                        }
                    }
                }

                //Debug.Log("没有找到999901元素");
                return false;
            }
            catch (Exception e)
            {
                //Debug.LogError($"检查背刺元素错误: {e}");
                return false;
            }
        }
        // 获取当前攻击使用的武器
        private static Thing GetCurrentWeapon()
        {
            try
            {
                //Debug.Log("=== 开始获取当前武器 ===");

                // 优先尝试从 AttackProcess.Current 获取武器
                var currentAttack = AttackProcess.Current;
                if (currentAttack?.weapon != null)
                {
                    //Debug.Log($"从 AttackProcess 获取到武器: {currentAttack.weapon.id} ");
                    return currentAttack.weapon;
                }
                else
                {
                    //Debug.Log("AttackProcess.Current 中未找到武器或为 null");
                }

                // 如果 AttackProcess 中没有，则尝试从角色装备中获取
                if (Act.CC?.body != null)
                {
                    //Debug.Log($"开始扫描角色 {Act.CC.id} 的装备槽位...");
                    int slotCount = 0;
                    int weaponSlotCount = 0;

                    foreach (var slot in Act.CC.body.slots)
                    {
                        slotCount++;
                        if (slot?.thing != null && slot.elementId == 35 && slot.thing.source.offense.Length >= 2)
                        {
                            weaponSlotCount++;
                            //Debug.Log($"找到武器槽位 {weaponSlotCount}: {slot.thing.id} (位置: {slot.elementId}, )");
                            return slot.thing;
                        }
                    }

                    //Debug.Log($"扫描完成。共检查 {slotCount} 个槽位，其中 {weaponSlotCount} 个是有效武器槽位");
                }
                else
                {
                    //Debug.Log("角色或角色身体为 null，无法检查装备");
                }

                //Debug.Log("未找到任何武器，返回 null");
                return null;
            }
            catch (Exception e)
            {
                //Debug.LogError($"获取武器过程中发生错误: {e}");
                return null;
            }
        }
    }
    [HarmonyPatch(typeof(AttackProcess))]
    [HarmonyPatch("GetRawDamage")]
    public static class SimpleDamageLogPatch
    {
        static float lastDmgMulti = 1f;
        static long lastBaseDamage = 0;

        [HarmonyPrefix]
        static void GetRawDamagePrefix(AttackProcess __instance, float dmgMulti)
        {
            lastDmgMulti = dmgMulti;
        }

        [HarmonyPostfix]
        static void GetRawDamagePostfix(ref long __result, float dmgMulti)
        {
            // 如果是背刺攻击
            if (dmgMulti >= 1.49f)
            {
                long actualDamage = __result;
                // 估算无背刺伤害
                long estimatedWithoutBackstab = (long)(actualDamage / dmgMulti);

                //Debug.Log($"🎯 背刺伤害分析:");
                //Debug.Log($"   伤害倍率: {dmgMulti}");
                //Debug.Log($"   实际伤害: {actualDamage}");
                //Debug.Log($"   估算无背刺伤害: {estimatedWithoutBackstab}");
                //Debug.Log($"   背刺增益: +{actualDamage - estimatedWithoutBackstab} 伤害");

                if (estimatedWithoutBackstab > 0)
                {
                    //Debug.Log($"   提升比例: +{((actualDamage - estimatedWithoutBackstab) / (float)estimatedWithoutBackstab * 100):F1}%");
                }

                //Debug.Log($"---");
            }
        }
    }

    public static class BackstabUtils
    {
        public static bool IsBackAttack(Chara attacker, Chara target)
        {
            if (attacker == null || target == null)
                return false;

            Point attackerPos = attacker.pos;
            Point targetPos = target.pos;

            int dx = targetPos.x - attackerPos.x;
            int dz = targetPos.z - attackerPos.z;

            //Debug.Log($"背面判断: 攻击者({attackerPos.x},{attackerPos.z}) -> 目标({targetPos.x},{targetPos.z}), dx={dx}, dz={dz}");

            if (dx == 0 && dz == 0)
            {
                //Debug.Log("同一位置，不算背面");
                return false;
            }

            int targetDirection = target.dir; // 四方向: 0=左, 1=下, 2=右, 3=上
            int attackDirection = CalculateFourDirectionFromEight(dx, dz);

            //Debug.Log($"目标朝向: {targetDirection}({GetDirectionName(targetDirection)}), 攻击方向: {attackDirection}({GetDirectionName(attackDirection)})");

            bool isBack = IsInBackSector(targetDirection, attackDirection);
            //Debug.Log($"是否为背面攻击: {isBack}");

            return isBack;
        }

        // 将八方向位置关系映射到四方向
        private static int CalculateFourDirectionFromEight(int dx, int dz)
        {
            // 八方向到四方向的映射：
            // 左(0): dx<0 且 |dx|>=|dz|
            // 右(2): dx>0 且 |dx|>=|dz|  
            // 下(1): dz<0 且 |dz|>|dx|
            // 上(3): dz>0 且 |dz|>|dx|

            int absDx = Math.Abs(dx);
            int absDz = Math.Abs(dz);

            if (absDx >= absDz)
            {
                // 水平方向为主
                return (dx > 0) ? 2 : 0; // 右或左
            }
            else
            {
                // 垂直方向为主
                return (dz > 0) ? 3 : 1; // 上或下
            }
        }

        // 判断攻击方向是否在目标的后方扇形区域内
        private static bool IsInBackSector(int targetDir, int attackDir)
        {
            // 定义每个朝向的后方扇形区域（包含正后方和两个斜后方）
            // 使用数组表示：{左后方, 正后方, 右后方}
            int[][] backSectors = new int[][]
            {
            new int[] { 1, 2, 3 }, // 目标朝左(0)：下、右、上算背面
            new int[] { 2, 3, 0 }, // 目标朝下(1)：右、上、左算背面  
            new int[] { 3, 0, 1 }, // 目标朝右(2)：上、左、下算背面
            new int[] { 0, 1, 2 }  // 目标朝上(3)：左、下、右算背面
            };

            bool isInBackSector = backSectors[targetDir].Contains(attackDir);

            //Debug.Log($"目标朝{GetDirectionName(targetDir)}，后方区域: [{string.Join(",", backSectors[targetDir].Select(d => GetDirectionName(d)))}]，攻击从{GetDirectionName(attackDir)}来，是否在背面区域: {isInBackSector}");

            return isInBackSector;
        }

        // 或者使用更宽松的判断：180度后半圆都算背面
        private static bool IsInBackHalfCircle(int targetDir, int attackDir)
        {
            // 四方向的180度后半圆：
            // 朝左(0)：右(2)、上(3)、下(1) 都算背面（除了左）
            // 朝下(1)：上(3)、左(0)、右(2) 都算背面（除了下）
            // 朝右(2)：左(0)、下(1)、上(3) 都算背面（除了右）
            // 朝上(3)：下(1)、右(2)、左(0) 都算背面（除了上）

            bool isInBackHalf = (attackDir != targetDir);

            //Debug.Log($"目标朝{GetDirectionName(targetDir)}，攻击从{GetDirectionName(attackDir)}来，是否在180度后半圆: {isInBackHalf}");

            return isInBackHalf;
        }

        // 或者使用精确的90度背面扇形
        private static bool IsIn90DegreeBack(int targetDir, int attackDir)
        {
            // 每个朝向的90度背面扇形：
            int[][] back90Sectors = new int[][]
            {
            new int[] { 1, 2 },    // 朝左：下、右算背面
            new int[] { 2, 3 },    // 朝下：右、上算背面
            new int[] { 3, 0 },    // 朝右：上、左算背面  
            new int[] { 0, 1 }     // 朝上：左、下算背面
            };

            bool isInBack90 = back90Sectors[targetDir].Contains(attackDir);

            //Debug.Log($"目标朝{GetDirectionName(targetDir)}，90度背面区域: [{string.Join(",", back90Sectors[targetDir].Select(d => GetDirectionName(d)))}]，攻击从{GetDirectionName(attackDir)}来，是否在90度背面: {isInBack90}");

            return isInBack90;
        }

        private static string GetDirectionName(int direction)
        {
            return direction switch
            {
                0 => "左",
                1 => "下",
                2 => "右",
                3 => "上",
                _ => "未知"
            };
        }
    }

}

namespace InuiBackAttack.info
{


    [BepInPlugin("InuiDameBackAttack", "背刺模块", "1.0.0")]
    internal class InuiDameBackAttack : BaseUnityPlugin
    {
        private void Start() {
            Harmony harmony = new Harmony("inui.BackAttack");
            harmony.PatchAll();
            //Debug.Log("背刺补丁已加载");
        }
    }
}