using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace GBF.trait.TraitItem_Summon
{
    /// 由于工作原理大差不差，我只会注释第一个内容。
    /// Since the working principles are quite similar, I will only comment on the first content.
    /// 動作原理がほぼ同じため、最初の内容のみに注釈を付けます。
    /// 如果是道具召唤友军，看TraitCidalaSummon，如果是道具召唤敌军，看TraitLuminieraSummon
    /// If using items to summon allies, refer to TraitCidalaSummon; if using items to summon enemies, refer to TraitLuminieraSummon
    /// アイテムで味方を召喚する場合はTraitCidalaSummonを参照し、アイテムで敵を召喚する場合はTraitLuminieraSummonを参照してください

   internal class TraitCidalaSummon : TraitItem
{
    // 使用特质物品召唤角色 / Use trait item to summon character / 特性アイテムを使用してキャラを召喚
    public override bool OnUse(Chara c)
    {
        if (!c.IsPC)
        {
            c.SayNothingHappans();
            return false;
        }
        
        if (EClass.game.cards.globalCharas.Values.Any(gc => gc.id == "Cidala"))
        {
            c.Say("Cidala1", null, null);
            this.owner.ModNum(-1, true);   
            return false;
        }
        
        Chara chara = CharaGen.Create("Cidala", -1);
        chara.AddThing("GBF_Tigrisius#Artifact");   
        
        EClass._zone.AddCard(chara, c.pos.GetNearestPoint(false, false, true, false));
        
        chara.MakeAlly(true);
        chara.PlayEffect("teleport", true, 0f, default(Vector3));
        
        this.owner.ModNum(-1, true);
        return true;
    }
}
    internal class TraitNarmayaSummon1 : TraitItem
    {
        public override bool OnUse(Chara c)
        {
            if (!c.IsPC)
            {
                c.SayNothingHappans();
                return false;
            }

            if (EClass.game.cards.globalCharas.Values.Any(gc => gc.id == "NarmayaWind"))
            {
                c.Say("Narmaya1", null, null);
                this.owner.ModNum(-1, true);
                return false;
            }

            Chara chara = CharaGen.Create("NarmayaWind", -1);
            chara.AddThing("GBF_Evanescence2#Artifact"); 

            EClass._zone.AddCard(chara, c.pos.GetNearestPoint(false, false, true, false));
            chara.MakeAlly(true);
            chara.PlayEffect("teleport", true, 0f, default(Vector3));
            this.owner.ModNum(-1, true);
            return true;
        }

    }
    internal class TraitCagliostroClarisseSummon : TraitItem
    {
        public override bool OnUse(Chara c)
        {
            if (!c.IsPC)
            {
                c.SayNothingHappans();
                return false;
            }

            if (EClass.game.cards.globalCharas.Values.Any(gc => gc.id == "Cagliostro_Clarisse"))
            {
                c.Say("Cagliostro_Clarisse1", null, null);
                this.owner.ModNum(-1, true);
                return false;
            }

            Chara chara = CharaGen.Create("Cagliostro_Clarisse", -1);
            chara.AddThing("GBF_Ouroboric2#Artifact");

            EClass._zone.AddCard(chara, c.pos.GetNearestPoint(false, false, true, false));
            chara.MakeAlly(true);
            chara.PlayEffect("teleport", true, 0f, default(Vector3));
            this.owner.ModNum(-1, true);
            return true;
        }

    }
    internal class TraitBerceauSummon : TraitItem
    {
        public override bool OnUse(Chara c)
        {
            if (!c.IsPC)
            {
                c.SayNothingHappans();
                return false;
            }

            if (EClass.game.cards.globalCharas.Values.Any(gc => gc.id == "Berceau"))
            {
                c.Say("Berceau1", null, null);
                this.owner.ModNum(-1, true);
                return false;
            }

            Chara chara = CharaGen.Create("Berceau", -1);
            chara.AddThing("GBF_Epee_Scintillante2#Artifact");

            EClass._zone.AddCard(chara, c.pos.GetNearestPoint(false, false, true, false));
            chara.MakeAlly(true);
            chara.PlayEffect("teleport", true, 0f, default(Vector3));
            this.owner.ModNum(-1, true);
            return true;
        }

    }
    
    internal class TraitEuropaSummon : TraitItem
    {
        public override bool OnUse(Chara c)
        {
            if (!c.IsPC)
            {
                c.SayNothingHappans();
                return false;
            }

            if (EClass.game.cards.globalCharas.Values.Any(gc => gc.id == "Europa"))
            {
                c.Say("Berceau1", null, null);
                this.owner.ModNum(-1, true);
                return false;
            }

            Chara chara = CharaGen.Create("Europa", -1);
            chara.AddThing("GBF_Epee_Scintillante#Artifact");

            EClass._zone.AddCard(chara, c.pos.GetNearestPoint(false, false, true, false));
            chara.MakeAlly(true);
            chara.PlayEffect("teleport", true, 0f, default(Vector3));
            this.owner.ModNum(-1, true);
            return true;
        }

    }
    internal class TraitLuminieraSummon : TraitItem
{
    public override bool OnUse(Chara c)
    {
        if (!c.IsPC)
        {
            c.SayNothingHappans();
            return false;
        }

        if (EClass.game.cards.globalCharas.Values.Any(gc => gc.id == "GBF_Boss_Luminiera_1"))
            return false;

        Chara chara = CharaGen.Create("GBF_Boss_Luminiera_1", -1);
        // 无标签，不调用 AddThing

        EClass._zone.AddCard(chara, c.pos.GetNearestPoint(false, false, true, false));
        chara.PlayEffect("teleport", true, 0f, default(Vector3));
        this.owner.ModNum(-1, true);
        return true;
    }
}
    internal class TraitColossusSummon : TraitItem
{
    public override bool OnUse(Chara c)
    {
        if (!c.IsPC)
            return false;

        if (EClass.game.cards.globalCharas.Values.Any(gc => gc.id == "GBF_Boss_Colossus"))
            return false;

        Chara chara = CharaGen.Create("GBF_Boss_Colossus", -1);
        // 无标签

        EClass._zone.AddCard(chara, c.pos.GetNearestPoint(false, false, true, false));
        chara.PlayEffect("teleport", true, 0f, default(Vector3));
        this.owner.ModNum(-1, true);
        return true;
    }
}
}
