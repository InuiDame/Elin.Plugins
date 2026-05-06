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

namespace Condition_SK2559
{
    public class ConSK2559 : Timebuff
    {
        public override bool AllowMultipleInstance => false; 
        public override bool SyncRide => true;               
        public virtual int GetExtraAttacks() => 1;
        
        public virtual float GetCounterDamageBonus()
        {
            return 0.5f; // +50%
        }
    }
    
    [HarmonyPatch(typeof(ActMeleeCounter), nameof(ActMeleeCounter.BaseDmgMTP), MethodType.Getter)]
    public class Patch_CounterDamage
    {
        static void Postfix(ActMeleeCounter __instance, ref float __result)
        {
            var cc = Act.CC; // 当前攻击者（反击者）

            if (cc != null && cc.HasCondition<ConSK2559>())
            {
                var con = cc.GetCondition<ConSK2559>();
                __result += con.GetCounterDamageBonus();
            }
        }
    }
}
