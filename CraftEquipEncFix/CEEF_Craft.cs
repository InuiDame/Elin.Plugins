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
            if (thingProduct == null || thingProduct.source == null || thingProduct.source.tag == null)
            {
                return true; 
            }
            
            // 检查是否包含 Inui_CCEF tag（大小写不敏感）
            bool hasInuiCCEF = thingProduct.source.tag.Any(tag => 
                tag.Equals("Inui_CCEF", StringComparison.OrdinalIgnoreCase));
            if (!hasInuiCCEF)
            {
                return true;
            }
            
            // 检查是否同时存在 allowop tag（大小写不敏感）
            bool allowOpElements = thingProduct.source.tag.Any(tag => 
                tag.Equals("allowop", StringComparison.OrdinalIgnoreCase));
            
            // 解析附魔百分比标签
            List<float> enchantPercents = new List<float>();

            foreach (var tag in thingProduct.source.tag)
            {
                if (tag == null) continue;
    
                if (tag.StartsWith("percentenchant\\", StringComparison.OrdinalIgnoreCase))
                {
                    string percentStr = tag.Substring("percentenchant\\".Length);
                    if (float.TryParse(percentStr, out float percent))
                    {
                        enchantPercents.Add(percent);
                    }
                }
            }
            

            bool isFood = type == CraftUtil.MixType.Food;
            int nutFactor = 100 - (ings.Count - 1) * 5;

            if (crafter != null && crafter.Evalue(1650) >= 3)
                nutFactor -= 10;

            int totalWeight = 0;
            int totalPrice = 0;
            
            // 记录每个素材的原始附魔
            for (int i = 0; i < ings.Count; i++)
            {
                if (ings[i] != null)
                {
                    foreach (var kv in ings[i].elements.dict)
                    {
                        if (kv.Value.Value != 0)
                        {
                            
                        }
                    }
                }
            }

            for (int i = 0; i < ings.Count; i++)
            {
                Thing ing = ings[i];
                if (ing == null) continue;

                float enchantPercent = 100f; // 默认100%
                if (i < enchantPercents.Count)
                {
                    enchantPercent = enchantPercents[i]; // 按顺序取百分比
                }
                
                foreach (var kv in ing.elements.dict)
                {
                    Element e = kv.Value;
                    int val = e.Value;

                    
                    if (val == 0) continue;
                    
                    // 应用附魔百分比
                    int adjustedVal = (int)(val * enchantPercent / 100f);
                    if (adjustedVal <= 0) continue;

                    if (allowOpElements)
                    {
                        product.elements.ModBase(e.id, adjustedVal);
                    }
                    else
                    {
                        if (val == 64 || val == 65 || val == 66 || val == 67)
                        {
                            continue;
                        }
                        product.elements.ModBase(e.id, adjustedVal);
                    }
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
