using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Cwl.LangMod;

namespace ExtraSpell;

[HarmonyPatch]
public class MagicShop
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Trait), "OnBarter")]
    // ReSharper disable once InconsistentNaming
    public static void OnBarter(Trait __instance)
    {
        if (__instance.ShopType != ShopType.Magic || !__instance.owner.isRestocking)
            return;

        var shop = __instance.owner.things.Find("chest_merchant");

        // 尝试生成商人的包
        if (shop == null)
        {
            try
            {
                shop = ThingGen.Create("chest_merchant");
                __instance.owner.AddThing(shop);
            }
            catch (Exception e)
            {
                Debug.LogError("ES_Shop_BagSpawnError".Loc(e.Message));
                return;
            }
        }

        if (shop.things.Count + 2 > shop.things.GridSize)
            return;


        //重复生成
        for (var i = 0; i < MagicReprog.Refresh.Value; i++)
        {
            // 随机选择
            var outerIndex = EClass.rnd(ModInfos.MagicRollList.Count);
            var selectedPart = ModInfos.MagicRollList[outerIndex];
            string alias;
            switch (selectedPart)
            {
                // 判断
                case List<string> aliasList:
                    alias = aliasList[EClass.rnd(aliasList.Count)];
                    break;
                case string aliasStr:
                    alias = aliasStr;
                    break;
                default:
                    Debug.LogError("ES_Shop_FormatError".Loc(selectedPart.GetType()));
                    continue;
            }

            if (!EClass.sources.elements.alias.ContainsKey(alias))
            {
                EClass.pc.TalkRaw("*"+"ES_Shop_Nus".Loc(alias));
                Debug.LogError("ES_Shop_Nus".Loc(alias));
                continue;
            }
            shop.AddThing(ThingGen.CreateSpellbook(alias).Identify(false));
        }
    }
}