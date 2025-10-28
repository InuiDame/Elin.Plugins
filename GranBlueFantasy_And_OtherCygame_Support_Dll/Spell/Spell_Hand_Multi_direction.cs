using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Configuration;
using Cwl.API.Custom;
using Cwl.Helper.Unity;
using Cwl.LangMod;
using GBF.Modinfo;
using GBF.spell.Spell_Effects.Spell_Effect_ele;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace GBF.spell.Spell_Hand_Multi_direction
{
    internal class SpellGBF3 : Spell
{
    // 执行法术效果 / Perform spell effect / スペル効果を実行
    public override bool Perform()
    {
        // 获取父级元素 / Get parent element / 親エレメントを取得
        Element element = Act.CC.elements.GetElement(base.source.aliasParent);
        // 调用内部攻击处理 / Call internal attack processing / 内部攻撃処理を呼び出す
        SpellGBF3.ProcAt_Internal((element != null) ? element.Value : 1, base.source, base.act);
        // 计算法术经验值 / Calculate spell experience / スペル経験値を計算
        int spellExp = Act.CC.Chara.elements.GetSpellExp(Act.CC.Chara, base.act, 100);
        // 修改法术经验 / Modify spell experience / スペル経験値を修正
        Act.CC.Chara.ModExp(base.source.alias, spellExp);
        return true;  // 返回执行成功 / Return execution success / 実行成功を返す
    }
    
    // 内部攻击处理 - 多方向手部法术 / Internal attack processing - Multi-direction hand spells / 内部攻撃処理 - 多方向手魔法
    private static void ProcAt_Internal(int power, SourceElement.Row source, Act act)
    {
        // 创建元素实例 / Create element instance / エレメントインスタンスを作成
        Element element = Element.Create(source.aliasRef, power / 10);
        // 计算作用半径 / Calculate effect radius / 効果半径を計算
        int radius = (int)((double)source.radius + 0.01 * (double)power);
        
        // 创建多个攻击区域 / Create multiple attack areas / 複数の攻撃エリアを作成
        List<Point> list = EClass._map.ListPointsInLine(Act.CC.pos, Act.TP, 1);        // 直线区域 / Straight line area / 直線エリア
        List<Point> list2 = EClass._map.ListPointsInArc(Act.CC.pos, Act.TP, radius, 1f);  // 弧形区域1 / Arc area 1 / 弧状エリア1
        List<Point> list3 = EClass._map.ListPointsInArc(Act.CC.pos, Act.TP, radius, 1f);  // 弧形区域2 / Arc area 2 / 弧状エリア2
        List<Point> list4 = EClass._map.ListPointsInArc(Act.CC.pos, Act.TP, radius, 1f);  // 弧形区域3 / Arc area 3 / 弧状エリア3
        
        // 确保每个区域至少有一个目标点 / Ensure each area has at least one target point / 各エリアに少なくとも1つのターゲットポイントがあることを保証
        if (list.Count == 0)
        {
            list.Add(Act.TP.Copy());  // 添加目标点副本 / Add target point copy / ターゲットポイントコピーを追加
        }
        if (list2.Count == 0)
        {
            list2.Add(Act.TP.Copy());
        }
        if (list3.Count == 0)
        {
            list3.Add(Act.TP.Copy());
        }
        if (list4.Count == 0)
        {
            list4.Add(Act.TP.Copy());
        }
        
        // 移除施法者自身位置 / Remove caster's own position / キャスター自身の位置を削除
        list.Remove(Act.CC.pos);
        list2.Remove(Act.CC.pos);
        list3.Remove(Act.CC.pos);
        list4.Remove(Act.CC.pos);
        
        // 播放施法语音 / Play casting voice / 詠唱音声を再生
        Act.CC.Chara.Say("spell_ball", Act.CC.Chara, element.Name.ToLower(), null);
        // 等待0.8秒 / Wait 0.8 seconds / 0.8秒待機
        EClass.Wait(0.8f, Act.CC.Chara);
        // 延迟播放音效 / Delay playing sound effect / 遅延して音声効果を再生
        ActEffect.TryDelay(delegate
        {
            Act.CC.Chara.PlaySound("spell_ball", 1f, true);  // 播放球状法术音效 / Play ball spell sound effect / 球状スペル音声効果を再生
        });
        
        // 执行四个方向的手部法术攻击 / Execute hand spell attacks in four directions / 四方向の手魔法攻撃を実行
        GBFHandSpellEffects.Atk(Act.CC, Act.CC.pos, power / 5, element, list, act, source.alias, 0f, true, "hand_");    // 直线攻击，反向 / Straight attack, reverse / 直線攻撃、逆方向
        GBFHandSpellEffects.Atk(Act.CC, Act.CC.pos, power / 5, element, list2, act, source.alias, 0.1f, false, "hand_"); // 弧形攻击1，0.1秒延迟 / Arc attack 1, 0.1s delay / 弧状攻撃1、0.1秒遅延
        GBFHandSpellEffects.Atk(Act.CC, Act.CC.pos, power / 5, element, list3, act, source.alias, 0f, false, "hand_");   // 弧形攻击2，无延迟 / Arc attack 2, no delay / 弧状攻撃2、遅延なし
        GBFHandSpellEffects.Atk(Act.CC, Act.CC.pos, power / 5, element, list4, act, source.alias, 0.1f, false, "hand_"); // 弧形攻击3，0.1秒延迟 / Arc attack 3, 0.1s delay / 弧状攻撃3、0.1秒遅延
    }
}
}
