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

namespace Trait_Item
{
    public class TraitBSItem : TraitItem
    {
        // Token: 0x06000002 RID: 2 RVA: 0x000021D4 File Offset: 0x000003D4
        private EffectId IdEffect => GetParam(1).ToEnum<EffectId>();

        private string n1 => GetParam(2);

        public override int CraftNum => GetCraftNum();

        public override bool CanChangeHeight => false;

        private int GetCraftNum()
        {
            if (owner.id == "BS_bleed")
            {
                return 2 + EClass.rnd(2);
            }
            if (owner.id == "BS_hujiao")
            {
                return 3 + EClass.rnd(2);
            }

            return 0;
        }
        public override bool OnUse(Chara c)
        {
            int num = owner.Power;
            if (IdEffect == EffectId.Buff && n1 == "ConEuphoric")
            {
                num += owner.Evalue(750) * 5;
                num = num * (100 + (int)Mathf.Sqrt(c.Evalue(300)) * 5) / 100;
            }

            if (IdEffect == EffectId.Buff && n1 == "ConInvisibility")
            {
                num += owner.Evalue(750) * 5;
                num = num * (100 + (int)Mathf.Sqrt(c.Evalue(300)) * 5) / 100;
            }

            ActEffect.Proc(IdEffect, GetParamInt(3, num), owner.blessedState, c, null, new ActRef
            {
                n1 = n1
            });
            if (c.ExistsOnMap)
            {
                FoodEffect.ProcTrait(c, owner);
            }

            owner.ModNum(-1);
            return true;
        }

        public override Action GetHealAction(Chara c)
        {
            if (IdEffect == EffectId.Buff && n1 == "ConEuphoric" && !c.HasCondition<ConEuphoric>())
            {
                return delegate
                {
                    OnUse(c);
                };
            }

            if (IdEffect == EffectId.Buff && n1 == "ConInvisibility" && !c.HasCondition<ConInvisibility>())
            {
                return delegate
                {
                    OnUse(c);
                };
            }

            return null;
        }
    }
}
