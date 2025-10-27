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

namespace Trait_Food
{
    public class TraitBlackSoul : TraitFood
    //black soul
    {
        public override void OnEat(Chara c)
        {
            if (this.owner.blessedState is < BlessedState.Blessed and > BlessedState.Cursed)
            {
                EClass.rnd(2);
            }
            for (int i = 70; i <= 77; i++)
            {
                Element orCreateElement = c.elements.GetOrCreateElement(i);
                int v = this.owner.elements.GetOrCreateElement(i).Value / 10;
                int vBase = orCreateElement.vBase;
                c.elements.ModBase(i, v);
                c.elements.OnLevelUp(orCreateElement, vBase);
            }
            for (int j = 0; j < this.owner.LV; j++)
            {
                c.LevelUp();
            }
        }
    }
}
