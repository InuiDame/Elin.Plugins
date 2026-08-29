using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Reflection.Emit;
using UnityEngine;
using Microsoft.CodeAnalysis;
using HarmonyLib;
using BepInEx;
using BepInEx.Configuration;
using Newtonsoft.Json;
using DG.Tweening.Plugins;

namespace Trait_Item_Summon
{
    internal class TraitMP5Summon : TraitItem
    {
        public override bool OnUse(Chara c)
        {
            if (!c.IsPC)
            {
                c.Say("GFMP52", null, null);
                return false;
            }

            if (EClass.game.cards.globalCharas.Values.Any(gc => gc.id == "GFMP5"))
            {
                this.owner.ModNum(-1, true);
                return false;
            }

            Chara chara = CharaGen.Create("GFMP5", -1);
            chara.AddThing("GF_MP5#Mythical");

            EClass._zone.AddCard(chara, c.pos.GetNearestPoint(false, false, true, false));
            chara.MakeAlly(true);
            chara.PlayEffect("teleport", true, 0f, default(Vector3));
            this.owner.ModNum(-1, true);
            return true;
        }
    }

    internal class TraitMP5SummonSkin1 : TraitItem
    {
        public override bool OnUse(Chara c)
        {
            if (!c.IsPC)
            {
                c.Say("GFMP52", null, null);
                return false;
            }

            if (EClass.game.cards.globalCharas.Values.Any(gc => gc.id == "GFMP5ssz"))
            {
                this.owner.ModNum(-1, true);
                return false;
            }

            Chara chara = CharaGen.Create("GFMP5ssz", -1);
            chara.AddThing("GF_MP5#Mythical");

            EClass._zone.AddCard(chara, c.pos.GetNearestPoint(false, false, true, false));
            chara.MakeAlly(true);
            chara.PlayEffect("teleport", true, 0f, default(Vector3));
            this.owner.ModNum(-1, true);
            return true;
        }
    }

    internal class TraitMP5SummonSkin2 : TraitItem
    {
        public override bool OnUse(Chara c)
        {
            if (!c.IsPC)
            {
                c.Say("GFMP52", null, null);
                return false;
            }

            if (EClass.game.cards.globalCharas.Values.Any(gc => gc.id == "GFMP5aysm"))
            {
                this.owner.ModNum(-1, true);
                return false;
            }

            Chara chara = CharaGen.Create("GFMP5aysm", -1);
            chara.AddThing("GF_MP5#Mythical");

            EClass._zone.AddCard(chara, c.pos.GetNearestPoint(false, false, true, false));
            chara.MakeAlly(true);
            chara.PlayEffect("teleport", true, 0f, default(Vector3));
            this.owner.ModNum(-1, true);
            return true;
        }
    }

    internal class TraitMP5SummonSkin3 : TraitItem
    {
        public override bool OnUse(Chara c)
        {
            if (!c.IsPC)
            {
                c.Say("GFMP52", null, null);
                return false;
            }

            if (EClass.game.cards.globalCharas.Values.Any(gc => gc.id == "GFMP5hwm"))
            {
                this.owner.ModNum(-1, true);
                return false;
            }

            Chara chara = CharaGen.Create("GFMP5hwm", -1);
            chara.AddThing("GF_MP5#Mythical");

            EClass._zone.AddCard(chara, c.pos.GetNearestPoint(false, false, true, false));
            chara.MakeAlly(true);
            chara.PlayEffect("teleport", true, 0f, default(Vector3));
            this.owner.ModNum(-1, true);
            return true;
        }
    }

    internal class TraitMP5SummonSkin4 : TraitItem
    {
        public override bool OnUse(Chara c)
        {
            if (!c.IsPC)
            {
                c.Say("GFMP52", null, null);
                return false;
            }

            if (EClass.game.cards.globalCharas.Values.Any(gc => gc.id == "GFMP5zacq"))
            {
                this.owner.ModNum(-1, true);
                return false;
            }

            Chara chara = CharaGen.Create("GFMP5zacq", -1);
            chara.AddThing("GF_MP5#Mythical");

            EClass._zone.AddCard(chara, c.pos.GetNearestPoint(false, false, true, false));
            chara.MakeAlly(true);
            chara.PlayEffect("teleport", true, 0f, default(Vector3));
            this.owner.ModNum(-1, true);
            return true;
        }
    }

    internal class TraitMP5SummonSkin5 : TraitItem
    {
        public override bool OnUse(Chara c)
        {
            if (!c.IsPC)
            {
                c.Say("GFMP52", null, null);
                return false;
            }

            if (EClass.game.cards.globalCharas.Values.Any(gc => gc.id == "GFMP5zjmwhs"))
            {
                this.owner.ModNum(-1, true);
                return false;
            }

            Chara chara = CharaGen.Create("GFMP5zjmwhs", -1);
            chara.AddThing("GF_MP5#Mythical");

            EClass._zone.AddCard(chara, c.pos.GetNearestPoint(false, false, true, false));
            chara.MakeAlly(true);
            chara.PlayEffect("teleport", true, 0f, default(Vector3));
            this.owner.ModNum(-1, true);
            return true;
        }
    }

    internal class TraitMP5SummonLove : TraitItem
    {
        public override bool OnUse(Chara c)
        {
            if (!c.IsPC)
            {
                c.Say("GFMP52", null, null);
                return false;
            }

            if (EClass.game.cards.globalCharas.Values.Any(gc => gc.id == "GFMP5Love"))
            {
                this.owner.ModNum(-1, true);
                return false;
            }

            Chara chara = CharaGen.Create("GFMP5Love", -1);
            chara.AddThing("GF_MP5#Mythical");

            EClass._zone.AddCard(chara, c.pos.GetNearestPoint(false, false, true, false));
            chara.MakeAlly(true);
            chara.PlayEffect("teleport", true, 0f, default(Vector3));
            this.owner.ModNum(-1, true);
            EClass.pc.Pick(ThingGen.Create("GF_MP5_gift", -1, -1), true, true);
            return true;
        }
    }

    internal class TraitMP5SummonMOD3 : TraitItem
    {
        public override bool OnUse(Chara c)
        {
            if (!c.IsPC)
            {
                c.Say("GFMP52", null, null);
                return false;
            }

            if (EClass.game.cards.globalCharas.Values.Any(gc => gc.id == "GFMP5MOD3"))
            {
                this.owner.ModNum(-1, true);
                return false;
            }

            Chara chara = CharaGen.Create("GFMP5MOD3", -1);
            chara.AddThing("GF_MP5#Mythical");

            EClass._zone.AddCard(chara, c.pos.GetNearestPoint(false, false, true, false));
            chara.MakeAlly(true);
            chara.PlayEffect("teleport", true, 0f, default(Vector3));
            this.owner.ModNum(-1, true);
            return true;
        }
    }

    internal class TraitTPSSummon : TraitItem
    {
        public override bool OnUse(Chara c)
        {
            if (!c.IsPC)
            {
                return false;
            }

            if (EClass.game.cards.globalCharas.Values.Any(gc => gc.id == "GFTPS"))
            {
                this.owner.ModNum(-1, true);
                return false;
            }

            Chara chara = CharaGen.Create("GFTPS", -1);
            chara.AddThing("GF_TPS#Mythical");

            EClass._zone.AddCard(chara, c.pos.GetNearestPoint(false, false, true, false));
            chara.MakeAlly(true);
            chara.PlayEffect("teleport", true, 0f, default(Vector3));
            this.owner.ModNum(-1, true);
            return true;
        }
    }

    internal class TraitTPSSummonSkin1 : TraitItem
    {
        public override bool OnUse(Chara c)
        {
            if (!c.IsPC)
            {
                return false;
            }

            if (EClass.game.cards.globalCharas.Values.Any(gc => gc.id == "GFTPSxbtyx"))
            {
                this.owner.ModNum(-1, true);
                return false;
            }

            Chara chara = CharaGen.Create("GFTPSxbtyx", -1);
            chara.AddThing("GF_TPS#Mythical");

            EClass._zone.AddCard(chara, c.pos.GetNearestPoint(false, false, true, false));
            chara.MakeAlly(true);
            chara.PlayEffect("teleport", true, 0f, default(Vector3));
            this.owner.ModNum(-1, true);
            return true;
        }
    }
}