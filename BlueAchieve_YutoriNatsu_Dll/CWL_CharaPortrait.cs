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

namespace CWL_CharaPortrait
{
    public class BA_natsu_CharaPortrait
    {
        // 特殊肖像角色ID集合 / Special portrait character ID collection / 特殊ポートレートキャラIDコレクション
        private static readonly HashSet<string> SpecialPortraitIds = new()
        {
            "inui_natsu"  // 需要特殊肖像的角色ID / Character ID requiring special portrait / 特殊ポートレートが必要なキャラID
        };

        // 角色创建时的事件处理 / Event handling when character is created / キャラ作成時のイベント処理
        [CwlCharaOnCreateEvent]
        internal static void OnCharaInstantiation(Chara __instance)
        {
            // 检查角色ID是否在特殊肖像列表中 / Check if character ID is in special portrait list / キャラIDが特殊ポートレートリストにあるか確認
            if (SpecialPortraitIds.Contains(__instance.id))
            {
                // 设置角色使用自身ID作为肖像ID / Set character to use own ID as portrait ID / キャラが自身のIDをポートレートIDとして使用するように設定
                __instance.c_idPortrait = __instance.id;
            }
        }
    }
}

