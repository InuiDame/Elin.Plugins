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



namespace MRPE
{

    [HarmonyPatch]
    public class Element200001FullPenetration
    {
        // 根据元素等级获取穿透级别
        private static int GetPenetrationLevel(int elementLevel)
        {
            return elementLevel switch
            {
                >= 1 and <= 20 => 1,  // 1-20级穿透1级
                >= 21 and <= 40 => 2, // 21-40穿透2级
                >= 41 and <= 60 => 3, // 41-60穿透3级
                >= 61 and <= 80 => 4, // 61-80穿透4级
                >= 81 => 5,           // 81+穿透5级
                _ => 0                // 默认无穿透
            };
        }

        // 方法1：直接修改GetResistDamage方法，通过AttackProcess.Current获取攻击者
        [HarmonyPatch(typeof(Element), nameof(Element.GetResistDamage))]
        [HarmonyPrefix]
        public static bool OverrideResistDamage(ref int __result, int dmg, int v, int power = 0)
        {
            try
            {
                // 通过AttackProcess.Current获取当前攻击者
                var attackProcess = AttackProcess.Current;
                if (attackProcess?.CC != null)
                {
                    // 检查攻击者是否拥有200001元素
                    int element200001Level = attackProcess.CC.Evalue(200001);
                    if (element200001Level > 0)
                    {
                        int penetrationLevel = GetPenetrationLevel(element200001Level);

                        // 获取原始抗性等级
                        int originalResistLevel = Element.GetResistLv(v);
                        int finalResistLevel = originalResistLevel;

                        // 应用穿透效果：每级穿透降低1级抗性
                        if (penetrationLevel > 0 && originalResistLevel > 0)
                        {
                            // 穿透力直接降低抗性等级
                            finalResistLevel = Mathf.Max(originalResistLevel - penetrationLevel, 0);

                            // 同时也要使用原始穿透力(power)进行进一步降低
                            if (power > 0 && finalResistLevel > 0)
                            {
                                finalResistLevel = Mathf.Max(finalResistLevel - power, 0);
                            }
                        }
                        else
                        {
                            // 如果没有元素穿透，使用原始逻辑
                            if (power > 0 && originalResistLevel > 0)
                            {
                                finalResistLevel = Mathf.Max(originalResistLevel - power, 0);
                            }
                        }

                        // 根据最终抗性等级计算伤害
                        if (finalResistLevel >= 4)
                        {
                            __result = 0;
                        }
                        else
                        {
                            __result = finalResistLevel switch
                            {
                                3 => dmg / 4,
                                2 => dmg / 3,
                                1 => dmg / 2,
                                0 => dmg,
                                -1 => dmg * 3 / 2,
                                -2 => dmg * 2,
                                _ => dmg * 2,
                            };
                        }

                        // 记录调试信息
                        if (attackProcess.CC.IsPC)
                        {
                            Debug.Log($"[200001穿透] 等级{element200001Level} → 穿透级别{penetrationLevel} → 原始抗性{originalResistLevel} → 最终抗性{finalResistLevel} → 最终伤害{__result}");
                        }

                        return false; // 跳过原始方法
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[200001元素穿透] 错误: {ex}");
            }

            return true; // 继续执行原始方法
        }

    }
}

namespace MRPE.info
{


    [BepInPlugin("MRPE", "冷却修复", "1.0.0")]
    internal class MRPE : BaseUnityPlugin
    {
        private void Start() {
            Harmony harmony = new Harmony("inui.MRPE");
            harmony.PatchAll();
        }
    }
}