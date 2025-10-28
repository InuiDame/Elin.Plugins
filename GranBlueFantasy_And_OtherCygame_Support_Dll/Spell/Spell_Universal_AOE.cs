using GBF.spell.Spell_Effects.Spell_Effect_Custom_Effect_Large;
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
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace GBF.spell.Spell_Universal_AOE
{
    // 我比较推荐使用这个方法，使用通用的AOE特效
// I recommend using this method, using generic AOE effects
// この方法を使用することをお勧めします。汎用的なAOEエフェクトを使用します
    internal class SpellGBF_2701 : Spell
{
    // 执行法术效果 / Perform spell effect / スペル効果を実行
    public override bool Perform()
    {
        // 获取父级元素 / Get parent element / 親エレメントを取得
        Element element = Act.CC.elements.GetElement(base.source.aliasParent);
        // 调用内部攻击处理 / Call internal attack processing / 内部攻撃処理を呼び出す
        SpellGBF_2701.ProcAt_Internal((element != null) ? element.Value : 1, base.source, base.act);
        // 计算法术经验值 / Calculate spell experience / スペル経験値を計算
        int spellExp = Act.CC.Chara.elements.GetSpellExp(Act.CC.Chara, base.act, 100);
        // 修改法术经验 / Modify spell experience / スペル経験値を修正
        Act.CC.Chara.ModExp(base.source.alias, spellExp);
        
        // 处理法术附加效果 / Process spell additional effects / スペル追加効果を処理
        if (base.source.proc.Length != 0)
        {
            string text = base.source.aliasRef.IsEmpty(CC.MainElement.source.alias);
            string text2 = base.source.proc[0];
            // 处理特殊效果类型 / Process special effect types / 特殊効果タイプを処理
            if (text2 == "LulwyTrick" || text2 == "BuffStats")
            {
                text = base.source.proc[1];
            }
            else if (text == "mold")
            {
                text = CC.MainElement.source.alias;
            }

            // 设置目标和位置 / Set target and position / ターゲットと位置を設定
            if (TargetType.Range == TargetRange.Self && !forcePt)
            {
                TC = CC;
                TP.Set(CC.pos);
            }

            // 计算威力并执行效果 / Calculate power and execute effect / 威力を計算して効果を実行
            int power2 = CC.elements.GetOrCreateElement(base.source.id).GetPower(CC);
            ActEffect.ProcAt(base.source.proc[0].ToEnum<EffectId>(), power2, BlessedState.Normal, CC, TC, TP, base.source.tag.Contains("neg"), new ActRef
            {
                n1 = base.source.proc.TryGet(1, returnNull: true),
                aliasEle = text,
                act = this
            });
        }
        return true;  // 返回执行成功 / Return execution success / 実行成功を返す
    }

    // 内部攻击处理 - 圆形范围法术 / Internal attack processing - Circular area spell / 内部攻撃処理 - 円形範囲スペル
    private static void ProcAt_Internal(int power, SourceElement.Row source, Act act)
    {
        // （1）元素构造和中心点 / (1) Element construction and center point / (1) エレメント構築と中心点
        Element element = Element.Create(source.aliasRef, power / 10);
        Point origin = Act.TP;
        const int radius = 5; // 半径为5的圆形 / Circle with radius 5 / 半径5の円形

        // （2）获取圆形范围内的所有点 / (2) Get all points within circular range / (2) 円形範囲内の全ポイントを取得
        var map = EClass._map;
        var list = map.ListPointsInCircle(origin, radius, true, true);  // 圆形区域目标点 / Circular area target points / 円形エリアターゲットポイント

        // （3）播放前摇、音效 / (3) Play casting animation, sound effects / (3) 詠唱アニメーション、音声効果を再生
        Act.CC.Chara.Say("Spell_GBF_2701", Act.CC.Chara, element.Name.ToLower(), null);
        EClass.Wait(0.8f, Act.CC.Chara);  // 等待0.8秒 / Wait 0.8 seconds / 0.8秒待機
        ActEffect.TryDelay(() =>
        {
            Act.CC.Chara.PlaySound("spell_ball", 1f, true);  // 播放球状法术音效 / Play ball spell sound effect / 球状スペル音声効果を再生
        });
        
        // 动态特效处理 / Dynamic effect processing / 動的エフェクト処理
        string effectName = $"{source.alias}_effect";
        Debug.Log($"DynamicSpell: 技能alias={source.alias}, 生成效果名称={effectName}");  // 特效名称日志 / Effect name log / エフェクト名ログ
        
        try
        {
            // 检查效果名称是否为空 / Check if effect name is empty / エフェクト名が空かチェック
            if (string.IsNullOrEmpty(effectName))
            {
                Debug.LogError($"DynamicSpell: effectName is null or empty! 技能alias={source.alias}");  // 特效名称为空错误 / Effect name empty error / エフェクト名が空のエラー
                return;
            }

            Debug.Log($"DynamicSpell: 准备播放效果 {effectName} 在位置 {origin}");  // 准备播放特效日志 / Prepare to play effect log / エフェクト再生準備ログ
            origin.PlayEffect(effectName);  // 播放动态特效 / Play dynamic effect / 動的エフェクトを再生
            Debug.Log($"DynamicSpell: 效果播放完成 {effectName}");  // 特效播放完成日志 / Effect play completed log / エフェクト再生完了ログ
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"DynamicSpell: 播放效果时发生异常 - {ex.Message}");  // 特效播放异常错误 / Effect play exception error / エフェクト再生例外エラー
            Debug.LogError($"DynamicSpell: 堆栈跟踪: {ex.StackTrace}");  // 堆栈跟踪 / Stack trace / スタックトレース

            // 尝试使用默认效果作为备选 / Try using default effect as fallback / デフォルトエフェクトを代替として使用
            try
            {
                Debug.LogWarning($"DynamicSpell: 尝试使用默认效果 ");  // 尝试默认效果警告 / Try default effect warning / デフォルトエフェクト試行警告
                origin.PlayEffect("Element/ball_Fire");  // 播放默认火球特效 / Play default fireball effect / デフォルト火球エフェクトを再生
            }
            catch (System.Exception ex2)
            {
                Debug.LogError($"DynamicSpell: 备选效果也失败 - {ex2.Message}");  // 备选效果失败错误 / Fallback effect failed error / 代替エフェクト失敗エラー
            }
        }

        // （4）实际发弹 / (4) Actually launch projectiles / (4) 実際に弾を発射
        GBFSpellEffects_2701.Atk(
            Act.CC,
            origin,             // 从中心施放 / Cast from center / 中心から発射
            power / 10,         // 基础伤害 / Base damage / 基本ダメージ
            element,
            list,               // 圆形范围内的目标列表 / Target list within circular range / 円形範囲内のターゲットリスト
            act,
            source.alias,
            0f,
            false,
            "ball_"
        );
    }
    
    // 允许自动发射 / Allow auto fire / オート発射可能
    public override bool CanAutofire => true;

    // 允许按住重复 / Allow press repeat / 押し続け繰り返し可能
    public override bool CanPressRepeat => true;

    // 允许快速射击 / Allow rapid fire / ラピッド射撃可能
    public override bool CanRapidFire => true;
}
}
