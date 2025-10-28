using System;
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

namespace GBF.spell.Spell_Noraml
{
    public class Actgbf1640 : Spell
    {
        public override bool CanAutofire => true;
        public override bool CanPressRepeat => true;
        public override bool CanRapidFire => true;
    }
    internal class SpellGBF : Spell
    {
    }
}
