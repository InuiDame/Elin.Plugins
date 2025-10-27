using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using UnityEngine;
using Cwl.API.Processors;
using Microsoft.CodeAnalysis;
using HarmonyLib;
using BepInEx;
using BepInEx.Configuration;
using Cwl;
using Cwl.API.Custom;
using BS.magicshop;
using Cwl.LangMod;
using static QuestCraft;
using System.Reflection;

namespace Condition_Normal
{
    public class ConBshonor : BaseBuff
    //Knight's Pride
    {
    }
    public class ConBShidden : BaseBuff
    //Hide & Seek
    {
    }
    public class ConBsfd : BaseBuff
    //Fight
    {
    }

    public class ConBsrishi : BaseBuff
    //Solar Eclipse
    {
    }


    public class ConBszs : BadCondition
    //Self-Multilation
    {

        public override bool CanManualRemove => true;
        public override Emo2 EmoIcon => Emo2.bleeding;
        //bleed bubble

        public override void Tick()
        {
            owner.DamageHP(EClass.rnd(Mathf.Clamp(owner.hp * (1 + base.value / 4) / 100 + 3, 1, (int)Mathf.Sqrt(owner.MaxHP) + 100)), AttackSource.Condition);
            //HP-
            owner.AddBlood();
            //effect
            Mod(-1);
        }
    }

}
