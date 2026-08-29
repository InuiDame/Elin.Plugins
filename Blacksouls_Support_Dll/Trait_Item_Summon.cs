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

namespace Trait_Item_Summon
{
    internal class TraitSummonBS2 : TraitItem
    {
        // 当使用特质物品时 / When using trait item
        public override bool OnUse(Chara c)
        {
            
            if (!c.IsPC)
                return false;

            if (EClass.game.cards.globalCharas.Values.Any(gc => gc.id == "BSBOSS_2"))
                return false;

            Chara chara = CharaGen.Create("BSBOSS_2", -1);
            chara.AddThing("BSBOSS_2T2#Artifact");

            EClass._zone.AddCard(chara, c.pos.GetNearestPoint(false, false, true, false));
            chara.PlayEffect("teleport", true, 0f, default(Vector3));
            this.owner.ModNum(-1, true);
            return true;
        }
    }

}