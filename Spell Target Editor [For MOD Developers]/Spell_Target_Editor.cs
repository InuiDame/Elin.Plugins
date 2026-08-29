using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using Debug = UnityEngine.Debug;
using BepInEx;
using BepInEx.Bootstrap;


namespace Spell_Target_Editor.info
{


    [BepInPlugin("Spell_Target_Editor", "法术目标编写器", "1.0.0")]
    internal class Spell_Target_Editor : BaseUnityPlugin
    {
        private void Start() {
            // 要检测的另一个 mod 的插件 GUID（或程序集名称）
            string otherModGUID = "com.cygame.gbf.info"; // 例如 "OtherMod.Example"

            bool otherLoaded = AppDomain.CurrentDomain.GetAssemblies() .Any(a => a.GetName().Name == "Cygame_Support");

            if (otherLoaded)
            {
                Logger.LogInfo("检测到另一个法术补丁Mod已加载，本Mod自动跳过相同补丁，避免冲突。");
                // 可以选择只修补不冲突的部分，或者完全跳过 PatchAll
                // 这里我们完全跳过，如果还有别的补丁需要执行，可以单独修补它们。
                // 但如果本Mod只有这个补丁，直接 return。
                return;
            }
            Harmony harmony = new Harmony("inui.SpellTargetEditor");
            harmony.PatchAll();
            Logger.LogInfo("法术目标编写器已加载");
        }
    }
}