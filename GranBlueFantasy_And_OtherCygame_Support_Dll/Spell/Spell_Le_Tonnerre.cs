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
using GBF.spell.Spell_Effects.Spell_Effect_Custom_Effect;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace GBF.spell.Spell_Le_Tonnerre
{
    internal class SpellGBF_2700 : Spell
{
    // 执行法术效果 / Perform spell effect / スペル効果を実行
    public override bool Perform()
    {
        // 获取父级元素 / Get parent element / 親エレメントを取得
        Element element = Act.CC.elements.GetElement(base.source.aliasParent);
        // 调用内部攻击处理 / Call internal attack processing / 内部攻撃処理を呼び出す
        SpellGBF_2700.ProcAt_Internal((element != null) ? element.Value : 1, base.source, base.act);
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

    // 内部攻击处理 - 小型十字形法术 / Internal attack processing - Small cross-shaped spell / 内部攻撃処理 - 小型十字形スペル
    private static void ProcAt_Internal(int power, SourceElement.Row source, Act act)
    {
        // （1）元素构造和中心点 / (1) Element construction and center point / (1) エレメント構築と中心点
        Element element = Element.Create(source.aliasRef, power / 10);
        Point origin = Act.TP;
        const int crossRadius = 1; // 横竖各 1 格 → 3×3 的十字 / Horizontal and vertical 1 tile each → 3×3 cross / 縦横各1タイル → 3×3の十字

        // （2）沿四个方向各采一条「线」 / (2) Collect "lines" along four directions / (2) 四方向に沿って「線」を収集
        var map = EClass._map;
        var hits = new List<Point>();

        // → 向右 / → Right / → 右
        hits.AddRange(map.ListPointsInLine(origin, new Point(origin.x + crossRadius, origin.z), crossRadius));
        // ← 向左 / ← Left / ← 左
        hits.AddRange(map.ListPointsInLine(origin, new Point(origin.x - crossRadius, origin.z), crossRadius));
        // ↑ 向上（+z 方向） / ↑ Up (+z direction) / ↑ 上（+z方向）
        hits.AddRange(map.ListPointsInLine(origin, new Point(origin.x, origin.z + crossRadius), crossRadius));
        // ↓ 向下（-z 方向） / ↓ Down (-z direction) / ↓ 下（-z方向）
        hits.AddRange(map.ListPointsInLine(origin, new Point(origin.x, origin.z - crossRadius), crossRadius));

        // （3）去重 & 去中心 / (3) Remove duplicates & remove center / (3) 重複除去 & 中心除去
        var list = hits
            //.Where(p => !p.Equals(origin))// 可选：移除中心点 / Optional: Remove center point / オプション：中心点を除去
            .GroupBy(p => new { p.x, p.z })  // 按坐标分组 / Group by coordinates / 座標でグループ化
            .Select(g => g.First())  // 取每组第一个 / Take first of each group / 各グループの最初を取得
            .ToList();

        // （4）播放前摇、音效 / (4) Play casting animation, sound effects / (4) 詠唱アニメーション、音声効果を再生
        Act.CC.Chara.Say("Spell_GBF_2700", Act.CC.Chara, element.Name.ToLower(), null);
        EClass.Wait(0.8f, Act.CC.Chara);  // 等待0.8秒 / Wait 0.8 seconds / 0.8秒待機
        ActEffect.TryDelay(() =>
        {
            Act.CC.Chara.PlaySound("spell_ball", 1f, true);  // 播放球状法术音效 / Play ball spell sound effect / 球状スペル音声効果を再生
        });

        // （5）实际发弹 / (5) Actually launch projectiles / (5) 実際に弾を発射
        GBFSpellEffects_2700.Atk(
            Act.CC,
            origin,             // 从中心施放 / Cast from center / 中心から発射
            power / 10,         // 基础伤害 / Base damage / 基本ダメージ
            element,
            list,               // 十字形目标列表 / Cross-shaped target list / 十字形ターゲットリスト
            act,
            source.alias,
            0f,
            false,
            "ball_"
        );
        
        // 额外的安全检查 / Additional safety checks / 追加の安全チェック
        if (Act.CC == null || Act.CC.Chara == null)
        {
            Debug.LogError("Act.CC or Act.CC.Chara is null!");  // 施法者为空错误 / Caster is null error / キャスターがnullのエラー
            return;
        }

        Act.CC.Chara.Say("Spell_GBF_2700", Act.CC.Chara, element.Name.ToLower(), null);  // 再次播放语音 / Play voice again / 再度音声を再生
        
        if (element == null)
        {
            Debug.LogError("Failed to create element!");  // 元素创建失败错误 / Element creation failed error / エレメント作成失敗エラー
            return;
        }

        if (string.IsNullOrEmpty(element.Name))
        {
            Debug.LogError("Element name is null or empty!");  // 元素名称为空错误 / Element name is empty error / エレメント名が空のエラー
            return;
        }
    }

    // 允许自动发射 / Allow auto fire / オート発射可能
    public override bool CanAutofire => true;

    // 允许按住重复 / Allow press repeat / 押し続け繰り返し可能
    public override bool CanPressRepeat => true;

    // 允许快速射击 / Allow rapid fire / ラピッド射撃可能
    public override bool CanRapidFire => true;
}
}
