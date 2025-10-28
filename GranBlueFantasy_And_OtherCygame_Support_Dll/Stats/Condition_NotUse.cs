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

namespace Condition_NotUse
{
    public class ConGBFEnmity : Timebuff
    //背水
    {
        public override void OnStart()
        {
            float lostHPRatio = (owner.MaxHP - owner.hp) / (float)owner.MaxHP;
            int steps = Mathf.FloorToInt(lostHPRatio * 20); // 20 = 100% / 5%
            float damageMultiplier = 1 + steps * 0.05f;
            owner.elements.SetBase(67, (int)(damageMultiplier * 200));
            owner.elements.SetBase(140008, (int)(damageMultiplier * 100));
        }
        public override void Tick()
        {

            float lostHPRatio = (owner.MaxHP - owner.hp) / (float)owner.MaxHP;
            int steps = Mathf.FloorToInt(lostHPRatio * 20); // 20 = 100% / 5%
            float damageMultiplier = 1 + steps * 0.05f;
            owner.elements.SetBase(67, (int)(damageMultiplier * 200));
            owner.elements.SetBase(140008, (int)(damageMultiplier * 100));
            Mod(-1);

        }

        public override int GetPhase() => 0;

    }
    public class ConGBFStamina : Timebuff
    //浑身
    {

        public override void OnStart()
        {
            float HPRatio = owner.hp / (float)owner.MaxHP;
            int steps = Mathf.FloorToInt(HPRatio * 20); // 20 = 100% / 5%
            float damageMultiplier = 1 + steps * 0.05f;
            owner.elements.SetBase(67, (int)(damageMultiplier * 200));
            owner.elements.SetBase(140008, (int)(damageMultiplier * 100));
        }
        public override void Tick()
        {

            float HPRatio = owner.hp / (float)owner.MaxHP;
            int steps = Mathf.FloorToInt(HPRatio * 20); // 20 = 100% / 5%
            float damageMultiplier = 1 + steps * 0.05f;
            owner.elements.SetBase(67, (int)(damageMultiplier * 200));
            owner.elements.SetBase(140008, (int)(damageMultiplier * 100));
            Mod(-1);

        }
 
        public override int GetPhase() => 0;

    }
}
