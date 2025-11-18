using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using BepInEx;
using Cwl.Helper.Unity;
using HarmonyLib;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Icanseeanybody.spell
{

   [HarmonyPatch(typeof(Chara), "Refresh")]
   public class Chara_Refresh_Patch
   {
       [HarmonyPostfix]
       public static void Postfix(Chara __instance)
       {
        // 在原始方法执行后，强制设置 visibleWithTelepathy 为 true
        __instance.visibleWithTelepathy = true;
        
       } 
   }


}

namespace Icanseeanybody.info
{


    [BepInPlugin("Icanseeanybody", "心灵感应拓展模块", "1.0.0")]
    internal class MetalSlash : BaseUnityPlugin
    {
        private void Start() {
            Harmony harmony = new Harmony("inui.Icanseeanybody");
            harmony.PatchAll();
            Logger.LogInfo("心灵感应拓展模块已加载");
        }
    }
}