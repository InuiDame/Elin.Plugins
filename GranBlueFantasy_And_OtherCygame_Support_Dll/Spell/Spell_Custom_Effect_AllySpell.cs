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

namespace GBF.spell.Spell_Custom_Effect_AllySpell
{ 
    internal class SpellGBFself : Spell
    {
        // 执行法术效果 / Perform spell effect / スペル効果を実行
        public override bool Perform()
        {
            // 获取父级元素 / Get parent element / 親エレメントを取得
            Element element = Act.CC.elements.GetElement(base.source.aliasParent);
            // 调用内部攻击处理 / Call internal attack processing / 内部攻撃処理を呼び出す
            SpellGBFself.ProcAt_Internal(element?.Value ?? 1, base.source, base.act);
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

        // 内部攻击处理 - 友方法术 / Internal attack processing - Ally spell / 内部攻撃処理 - 味方スペル
        private static void ProcAt_Internal(int power, SourceElement.Row source, Act act)
        {
            // 修复：检查aliasRef是否为空，如果为空则使用默认元素 / Fix: Check if aliasRef is empty, if so use default element / 修正：aliasRefが空かチェック、空の場合はデフォルトエレメントを使用
            string elementAlias = source.aliasRef;
            if (string.IsNullOrEmpty(elementAlias))
            {
                elementAlias = Act.CC.Chara.MainElement.source.aliasRef;
                Debug.LogWarning($"SpellGBFself: source.aliasRef is empty, using default element: {elementAlias}");  // 使用默认元素警告 / Use default element warning / デフォルトエレメント使用警告
            }
            Element element = Element.Create(source.aliasRef, power / 10);
            Point origin = Act.TP;           

            // 播放特效和音效 / Play effects and sound / エフェクトと音声を再生
            Act.CC.Chara.Say("Spell_GBF_2701", Act.CC.Chara, element.Name.ToLower(), null);
            EClass.Wait(0.8f, Act.CC.Chara);  // 等待0.8秒 / Wait 0.8 seconds / 0.8秒待機
            ActEffect.TryDelay(() =>
            {
                Act.CC.Chara.PlaySound("spell_ball", 1f, true);  // 播放球状法术音效 / Play ball spell sound effect / 球状スペル音声効果を再生
            });

            // 动态特效处理 / Dynamic effect processing / 動的エフェクト処理
            string effectName = $"{source.alias}_effect";
            Debug.Log($"DynamicSpell_Ally: 技能alias={source.alias}, 生成效果名称={effectName}");  // 特效名称日志 / Effect name log / エフェクト名ログ

            try
            {
                if (string.IsNullOrEmpty(effectName))
                {
                    Debug.LogError($"DynamicSpell_Ally: effectName is null or empty!");  // 特效名称为空错误 / Effect name empty error / エフェクト名が空のエラー
                    return;
                }
                origin.PlayEffect(effectName);  // 播放动态特效 / Play dynamic effect / 動的エフェクトを再生
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"DynamicSpell_Ally: 播放效果时发生异常 - {ex.Message}");  // 特效播放异常错误 / Effect play exception error / エフェクト再生例外エラー
                try
                {
                    origin.PlayEffect("aura_heaven");  // 播放天堂光环特效作为备选 / Play heaven aura effect as fallback / 代替として天国のオーラエフェクトを再生
                }
                catch (System.Exception ex2)
                {
                    Debug.LogError($"DynamicSpell_Ally: 备选效果也失败 - {ex2.Message}");  // 备选效果失败错误 / Fallback effect failed error / 代替エフェクト失敗エラー
                }
            }

            // 对友方单位应用有益效果 / Apply beneficial effects to ally units / 味方ユニットに有益効果を適用
            ApplyAllyEffects(
                Act.CC.Chara,
                origin,
                power / 10,
                element,
                act,
                source.alias
            );
        }

        // 对友方应用效果 / Apply effects to allies / 味方に効果を適用
        private static void ApplyAllyEffects(Chara caster, Point origin, int power, Element element, Act act, string alias)
        {
            // 施法者自己触发效果 / Caster triggers effect on self / キャスター自身で効果をトリガー
            if (act.source.proc.Length > 0)
            {
                int effectPower = caster.elements.GetOrCreateElement(act.source.id).GetPower(caster);

                ActEffect.ProcAt(
                    act.source.proc[0].ToEnum<EffectId>(),
                    effectPower,
                    BlessedState.Normal,
                    caster,
                    caster,  // 目标和施法者都是自己 / Target and caster are both self / ターゲットとキャスターは両方自分
                    origin,
                    act.source.tag.Contains("neg"),
                    new ActRef
                    {
                        n1 = act.source.proc.TryGet(1, returnNull: true),
                        aliasEle = act.source.aliasRef.IsEmpty(caster.MainElement.source.alias),
                        act = act
                    }
                );
            }
        }

        // 判断是否为友方单位 / Determine if it's an ally unit / 味方ユニットか判断
        private static bool IsAlly(Chara target, Chara caster)
        {
            return target == caster || // 自己是友方 / Self is ally / 自分は味方
                   target.IsPCParty;   // 玩家队伍是友方 / Player party is ally / プレイヤーパーティは味方
        }

        // 允许自动发射 / Allow auto fire / オート発射可能
        public override bool CanAutofire => true;

        // 允许按住重复 / Allow press repeat / 押し続け繰り返し可能
        public override bool CanPressRepeat => true;

        // 允许快速射击 / Allow rapid fire / ラピッド射撃可能
        public override bool CanRapidFire => true;
    }
}
