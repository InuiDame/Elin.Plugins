using HarmonyLib;
using System.Linq;

namespace Modinfo.Patch;

[HarmonyPatch(typeof(CraftUtil), "MixIngredients")]
public static class Patch_MixIngredients_Natsu
{
    [HarmonyPostfix]
    private static void Postfix(Card product)
    {
        if (product == null) return;
        if (product.id == "KGR_BA_Natsu")
        {
            product.elements.SetBase("natsu_sk1", 20);
            product.elements.SetBase("natsu_sk2", 20);
        }
    }


}