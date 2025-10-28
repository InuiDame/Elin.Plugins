using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using Cwl.API.Custom;
using HarmonyLib;
using UnityEngine;

namespace GBF.Patch_TryDropBossLoot
{

   [HarmonyPatch(typeof(Chara), "TryDropBossLoot")]
internal static class TryDropBossLoot_Patch
{
    [HarmonyPrefix]
    public static bool Prefix(Chara __instance)
    {
        // 检查是否为PC阵营，如果是则继续执行原方法 / Check if PC faction, if so continue with original method / PC派閥か確認、もしそうなら元のメソッドを継続実行
        if (__instance.IsPCFaction || __instance.IsPCFactionMinion)
        {
            return true;
        }
        else
        {
            // 获取最近的可用点 / Get nearest available point / 最寄りの利用可能ポイントを取得
            Point point = __instance.pos.GetNearestPoint(true, false, false, true) ?? __instance.pos;
            
            // 检查是否为Luminiera第一阶段BOSS / Check if Luminiera phase 1 BOSS / Luminiera第一段階BOSSか確認
            if (__instance.id == "GBF_Boss_Luminiera_1")
            {
                // 创建第二阶段BOSS / Create phase 2 BOSS / 第二段階BOSSを作成
                Chara phase2Boss = CharaGen.Create("GBF_Boss_Luminiera_2", -1);
                EMono._zone.AddCard(phase2Boss, point);  // 添加到区域 / Add to zone / ゾーンに追加
                
                // 基于END属性计算效果强度 / Calculate effect strength based on END attribute / END属性に基づいて効果強度を計算
                int p = Mathf.Max(EClass.curve(__instance.END * 10, 400, 100, 75), 100);
                
                // 添加英雄状态 / Add hero condition / 英雄状態を追加
                phase2Boss.AddCondition<ConHero>(p, false);
                // 添加兴奋状态 / Add euphoric condition / 興奮状態を追加
                phase2Boss.AddCondition<ConEuphoric>(p, false);
                // 设置元素414基础值 / Set base value for element 414 / エレメント414の基本値を設定
                phase2Boss.elements.SetBase(414, 40, 0);
            }
            return true;  // 继续执行原方法 / Continue with original method / 元のメソッドを継続実行
        }
    }
}

[HarmonyPatch(typeof(Chara), "TryDropBossLoot")]
internal static class TryDropBossLoot_Patch2
{
    [HarmonyPostfix]
    public static void Postfix(Chara __instance)
    {
        // 检查是否是PC阵营，如果是则直接返回 / Check if PC faction, if so return directly / PC派閥か確認、もしそうなら直接戻る
        if (__instance.IsPCFaction || __instance.IsPCFactionMinion)
        {
            return;
        }

        // 获取boss的位置 / Get boss position / BOSSの位置を取得
        Point point = __instance.pos.GetNearestPoint(true, false, false, true) ?? __instance.pos;

        // 检查是否包含boss标签 / Check if contains boss tag / bossタグを含むか確認
        if (__instance.source.tag.Contains("boss"))
        {
            // 掉落宝箱 / Drop treasure chests / 宝箱をドロップ
            DropGBFBossLoot(point, __instance);
        }
    }

    // BOSS掉落物品处理 / BOSS loot drop processing / BOSSドロップアイテム処理
    private static void DropGBFBossLoot(Point point, Chara boss)
    {
        // 1. 必定掉落1个10086 / 1. Guaranteed drop 1x 10086 / 1. 必ず10086を1個ドロップ
        Thing chest1 = ThingGen.Create("GBFchest_Red", -1);  // 红色宝箱 / Red chest / 赤い宝箱
        EClass._zone.AddCard(chest1, point);

        // 2. 50%概率掉落1个10087 / 2. 50% chance to drop 1x 10087 / 2. 50%の確率で10087を1個ドロップ
        if (EClass.rnd(100) < 50)
        {
            Thing chest2 = ThingGen.Create("GBFchest_Blue", -1);  // 蓝色宝箱 / Blue chest / 青い宝箱
            EClass._zone.AddCard(chest2, point);
        }

        // 3. 掉落3个10088 / 3. Drop 3x 10088 / 3. 10088を3個ドロップ
        for (int i = 0; i < 3; i++)
        {
            Thing chest3 = ThingGen.Create("GBFchest_Silver", -1);  // 银色宝箱 / Silver chest / 銀の宝箱
            Point dropPoint = point.GetNearestPoint(true, false, false, true) ?? point;  // 获取附近的掉落点 / Get nearby drop point / 近くのドロップポイントを取得
            EClass._zone.AddCard(chest3, dropPoint);
        }

        // 4. 掉落5个10089 / 4. Drop 5x 10089 / 4. 10089を5個ドロップ
        for (int i = 0; i < 5; i++)
        {
            Thing chest4 = ThingGen.Create("GBFchest_Wooden", -1);  // 木质宝箱 / Wooden chest / 木製の宝箱
            Point dropPoint = point.GetNearestPoint(true, false, false, true) ?? point;  // 获取附近的掉落点 / Get nearby drop point / 近くのドロップポイントを取得
            EClass._zone.AddCard(chest4, dropPoint);
        }
    }
}

}
