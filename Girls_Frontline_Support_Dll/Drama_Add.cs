using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Reflection.Emit;
using UnityEngine;
using Cwl.API.Processors;
using Microsoft.CodeAnalysis;
using HarmonyLib;
using BepInEx;
using BepInEx.Configuration;
using Cwl;
using Cwl.API.Custom;
using Cwl.LangMod;
using Cwl.Helper;
using Newtonsoft.Json;

namespace Drama_Add
{
    public partial class GFDramaExpansion : DramaOutcome
    //使用于誓约MP5的食物赠与对话
    //For food gift dialogue used in Oath MP5
    //誓約MP5で使用する食品贈与ダイアログ
    {
        public static bool MP5_food(DramaManager dm, Dictionary<string, string> line, params string[] parameters)
        {


            Msg.Say("dropReward");
            CardBlueprint.SetNormalRarity();
            Thing t = ThingGen.Create("GF_bighappyfood", -1, -1);
            EMono._zone.AddCard(t, EMono.pc.pos);

            return true;
        }

    }
}
