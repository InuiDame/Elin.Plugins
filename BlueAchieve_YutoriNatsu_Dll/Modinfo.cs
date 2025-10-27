using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Configuration;
using Cwl.API.Attributes;
using Cwl.API.Custom;
using Cwl.Helper.Unity;
using Cwl.LangMod;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;


namespace Modinfo
{
    [BepInPlugin("inui.bluearchive.natsu.info", "BA_natsu", "1.0.0")]
    public class BA_natsu_info : BaseUnityPlugin
    { 
        private void Awake()
        {
            new Harmony("BA_natsu_harmony").PatchAll();
            Logger.LogInfo("柚鸟夏补丁激活成功");
            Harmony.DEBUG = true;
        }
    }
}











