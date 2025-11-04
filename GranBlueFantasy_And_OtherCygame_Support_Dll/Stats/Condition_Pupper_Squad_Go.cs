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

namespace Condition_Pupper_Squad_Go
//Pupper Squad, Go! / 狛戌犬慮 / 狛戌犬虑
{
    public class ConSK1374 : BaseBuff
    {
        public override bool WillOverride => false;  // 不覆盖其他效果 / Does not override other effects / 他の効果を上書きしない
    
        private const int FixedguardValue = 4000;  // 固定护盾值 / Fixed shield value / 固定シールド値
    
        // 获取护盾量 / Get shield amount / シールド量を取得
        public static int guardAmount(int power)
        {
            return FixedguardValue;  // 返回固定护盾值 / Return fixed shield value / 固定シールド値を返す
        }
    
        public override void OnStartOrStack()
        {
            base.value = FixedguardValue;  // 设置护盾值为固定值 / Set shield value to fixed amount / シールド値を固定値に設定
            base.OnStartOrStack();  // 调用基类方法 / Call base class method / 基本クラスメソッドを呼び出す
        }
    
        public override void Tick()
        {
            // 护盾值为0时移除效果 / Remove effect when shield value is 0 / シールド値が0の時に効果を削除
            if (base.value <= 0)
            {
                Kill();  // 移除状态 / Remove condition / 状態を削除
            }
        }
    
        // 增加护盾值 / Add shield amount / シールド量を追加
        public void Addguard(int amount)
        {
            base.value += amount;  // 增加护盾值 / Increase shield value / シールド値を増加
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
                base.value = p;  // 更新为更大的护盾值 / Update to larger shield value / より大きいシールド値に更新
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
            }
        }
    }
    
    public class ConPCRLunchtime : BaseBuff
    {
        private Dice? healDice;
        private bool diceInitialized; 
        public override bool WillOverride => false;  
        
        private void InitDice()
        {
            if (diceInitialized) return;
            int num = Mathf.Max(EClass.curve(owner.END * 10, 400, 100), 100);
            healDice = Dice.Create("SpHealEris", num);
            owner.HealHPHost(healDice.Roll(), HealSource.Magic);

            diceInitialized = true;
        }
        public int GetGuardAmount()
        {
            return owner.hp;
        }
    
        public override void OnStartOrStack()
        {
            InitDice();
            base.value = GetGuardAmount();  
            base.OnStartOrStack();  
        }
    
        public override void Tick()
        {
            if (base.value <= 0)
            {
                Kill();  
            }
        }
        
        public void Addguard(int amount)
        {
            base.value += amount;  
            OnValueChanged();  
        }
    
        public override bool CanStack(Condition c)
        {
            return c.GetType() == GetType();
        }
    
        public override void OnStacked(int p)
        {
            if (p > base.value)
            {
                base.value = p;  
            }
            SetPhase(); 
        }
    
        public override void OnWriteNote(List<string> list)
        {
            if (base.value > 0)
            {
                list.Add($"<color=#87CEEB>当前护盾/Shiled: {base.value}</color>");
            }
        }
    }
}
