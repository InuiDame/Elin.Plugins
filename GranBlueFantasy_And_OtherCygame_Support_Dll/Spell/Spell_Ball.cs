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
using GBF.spell.Spell_Effects.Spell_Effect_Ball;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace GBF.spell.Spell_Ball
{
    internal class SpellGBF2 : Spell
    {
        public override bool Perform()
        {
            Element element = Act.CC.elements.GetElement(base.source.aliasParent);
            SpellGBF2.ProcAt_Internal((element != null) ? element.Value : 1, base.source, base.act);
            int spellExp = Act.CC.Chara.elements.GetSpellExp(Act.CC.Chara, base.act, 100);
            Act.CC.Chara.ModExp(base.source.alias, spellExp);
            return true;
        }
        
        private static void ProcAt_Internal(int power, SourceElement.Row source, Act act)
        {
            int radius = (int)((double)source.radius + 0.01 * (double)power);
            Element element = Element.Create(source.aliasRef, power / 10);
            List<Point> list = EClass._map.ListPointsInArc(Act.CC.pos, Act.TP, radius, 15f);
            list.Remove(Act.CC.pos);
            Act.CC.Chara.Say("spell_ball", Act.CC.Chara, element.Name.ToLower(), null);
            EClass.Wait(0.8f, Act.CC.Chara);
            ActEffect.TryDelay(delegate
            {
                Act.CC.Chara.PlaySound("spell_ball", 1f, true);
            });
            GBFAreaSpellEffects.Atk(Act.CC, Act.CC.pos, power / 10, element, list, act, source.alias, 0f, false, "ball_");
        }
    }
}
