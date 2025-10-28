using System;
using System.Collections;
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
using static NoticeManager;
using static UnityEngine.UI.GridLayoutGroup;

namespace Condition_Ailes_Enlacantes
//Ailes Enlacantes
//エール・ド・レトラント
//交缠双翼
{
    public class ConSK2699 : BaseBuff
    {
        public override bool WillOverride => false;  // 不覆盖其他效果 / Does not override other effects / 他の効果を上書きしない

        private const int FixedguardValue = 10000;  // 固定护盾值 / Fixed shield value / 固定シールド値

        public override void OnStart()
        {
            // 基于END属性计算效果强度 / Calculate effect strength based on END attribute / END属性に基づいて効果強度を計算
            int num = Mathf.Max(EClass.curve(owner.END * 10, 400, 100), 100);
            // 添加交缠双翼效果2 / Add Ailes Enlacantes effect 2 / エール・ド・レトラント効果2を追加
            base.owner.AddCondition<ConSK2699_2>(num);
        }
        
        // 获取护盾量 / Get shield amount / シールド量を取得
        public static int guardAmount(int power)
        {
            return FixedguardValue;  // 返回固定护盾值 / Return fixed shield value / 固定シールド値を返す
        }

        public override void OnStartOrStack()
        {
            base.value = FixedguardValue;  // 设置护盾值为固定值 / Set shield value to fixed amount / シールド値を固定値に設定
            base.OnStartOrStack();
        }

        public override void Tick()
        {
            // 护盾值为0时移除效果 / Remove effect when shield value is 0 / シールド値が0の時に効果を削除
            if (base.value <= 0)
            {
                Kill();
            }
        }

        // 增加护盾值 / Add shield amount / シールド量を追加
        public void Addguard(int amount)
        {
            base.value += amount;
            OnValueChanged();  // 触发值变化事件 / Trigger value changed event / 値変更イベントをトリガー
        }

        public override bool CanStack(Condition c)
        {
            // 允许同类型效果堆叠 / Allow stacking of same type effects / 同じタイプの効果のスタックを許可
            return c.GetType() == GetType();
        }

        public override void OnStacked(int p)
        {
            // 堆叠时取最大值 / Take maximum value when stacking / スタック時に最大値を取る
            if (p > base.value)
            {
                base.value = p;
            }
            SetPhase();  // 设置阶段 / Set phase / フェーズを設定
        }

        public override void OnWriteNote(List<string> list)
        {
            if (base.value > 0)
            {
                // 显示当前护盾值 / Display current shield value / 現在のシールド値を表示
                list.Add($"<color=#87CEEB>当前护盾/Shiled: {base.value}</color>");
                // 显示最大护盾值 / Display maximum shield value / 最大シールド値を表示
                list.Add($"<color=#87CEEB>最大护盾/MaxShiled: {FixedguardValue}</color>");
                // 护盾低于30%时显示警告 / Show warning when shield is below 30% / シールドが30%未満の場合警告を表示
                if (base.value < FixedguardValue * 0.3f)
                {
                    list.Add($"<color=#FF6B6B>护盾即将耗尽!/The shield is about to run out!</color>");
                }
            }
        }
    }

    public class ConSK2699_2 : Timebuff
    {
        private const int MaxHOTHeal = 1000;  // 最大持续治疗量 / Maximum HOT heal amount / 最大持続治療量

        public override void Tick()
        {
            // 每回合持续治疗 / Heal over time each turn / 毎ターン持続治療
            owner.HealHPHost(MaxHOTHeal, HealSource.HOT);
            // 驱散负面效果 / Dispels negative effects / ネガティブ効果を解除
            DispelNegative(owner);
            // 减少持续时间 / Reduce duration / 持続時間を減少
            Mod(-1);
        }

        // 驱散负面状态效果 / Dispels negative status effects / ネガティブ状態効果を解除
        private void DispelNegative(Chara target)
        {
            var toRemove = new List<Condition>();
            // 收集所有负面和减益状态 / Collect all negative and debuff statuses / 全てのネガティブ及びデバフ状態を収集
            foreach (var cond in target.conditions)
                if (cond.Type == ConditionType.Bad || cond.Type == ConditionType.Debuff)
                    toRemove.Add(cond);
            // 移除所有负面状态 / Remove all negative statuses / 全てのネガティブ状態を削除
            foreach (var cond in toRemove)
                cond.Kill();
        }
        
        public override void OnWriteNote(List<string> list)
        {
            if (base.value > 0)
            {
                // 显示效果描述 / Display effect description / 効果説明を表示
                list.Add($"庇护之壳翼/Wings of the Cradle");
                list.Add($"格挡20%伤害/DMG taken 20%");
                list.Add($"<color=green>每回合最高回复1000HP/Recover 1000 HP each turn</color>");
                list.Add($"回合结束净化自身一个DEBUFF/Remove 1 debuff at end of turn");
            }
        }
        public override int GetPhase() => 0;  // 固定阶段0 / Fixed phase 0 / 固定フェーズ0
    }
}
