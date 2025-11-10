using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace CEEF_Craft
{

    [HarmonyPatch(typeof(CraftUtil), "MixIngredients", typeof(Card), typeof(List<Thing>), typeof(CraftUtil.MixType), typeof(int), typeof(Chara))]
    public static class Patch_CraftUtil_MixIngredients_AllElementMerge
    {
        public static bool Prefix(Card product, List<Thing> ings, CraftUtil.MixType type, int maxQuality, Chara crafter, ref Card __result)
        {
            Thing thingProduct = product as Thing;
            if (thingProduct == null || thingProduct.source == null || thingProduct.source.tag == null || !thingProduct.source.tag.Contains("Inui_CCEF"))
            {
                return true; 
            }

            bool isFood = type == CraftUtil.MixType.Food;
            int nutFactor = 100 - (ings.Count - 1) * 5;

            if (crafter != null && crafter.Evalue(1650) >= 3)
                nutFactor -= 10;

            int totalWeight = 0;
            int totalPrice = 0;

            foreach (Thing ing in ings)
            {
                if (ing == null) continue;

                
                foreach (var kv in ing.elements.dict)
                {
                    Element e = kv.Value;
                    int val = e.Value;

                    
                    if (val == 0 || e.id == 64 || e.id == 65|| e.id == 66 || e.id == 67) continue;

                    product.elements.ModBase(e.id, val);
                }

                if (isFood)
                {
                    totalWeight += Mathf.Clamp(ing.SelfWeight * 80 / 100, 50, 400 + ing.SelfWeight / 20);
                    totalPrice += ing.GetValue();
                }
            }

            
            if (isFood)
            {
                product.isWeightChanged = true;
                product.c_weight = totalWeight;
                product.c_priceAdd = totalPrice;
            }

            
            product.c_priceCopy = ings.Sum(i => i?.c_priceCopy ?? 0);

            
            if (product.elements.Value(2) > maxQuality)
            {
                product.elements.SetTo(2, maxQuality);
            }

            __result = product;
            return false; 
        }
    }
}
