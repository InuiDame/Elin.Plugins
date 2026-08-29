using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using UnityEngine;
using Microsoft.CodeAnalysis;
using HarmonyLib;
using BepInEx;
using BepInEx.Configuration;
using BS.magicshop;
using static QuestCraft;
using System.Reflection;

namespace Spell_Normal
{
    internal class ActBSweapon : Spell
    {
    }
    
    public class Actgbf1640 : Spell
    //using in GBF and FN
    {
        public override bool CanAutofire => true;
        public override bool CanPressRepeat => true;
        public override bool CanRapidFire => true;
    }
    internal class SpellBS : Spell
    {
    }
}
