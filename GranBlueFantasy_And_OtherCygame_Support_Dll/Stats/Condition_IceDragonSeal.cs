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

namespace Condition_IceDragonSeal
{
    public class ConIceDragonSeal : Timebuff
    {
        public override void Tick()
        {
            if (base.value > 20)
            {
                base.value = 20;  
            }
            if (base.value <= 0)
            {
                Kill();  
            }
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
    }
    public class ConPCRFrozen_Lake : Timebuff
{
    private Dice? hotDice;            
    private bool diceInitialized;
    
    private void InitDice()
    {
        if (diceInitialized) return;
        
        // 检查是否存在冰龙封印BUFF / Check if Ice Dragon Seal buff exists / 氷龍封印BUFFが存在するかチェック
        int bonusPower = power;
        if (owner.HasCondition<ConIceDragonSeal>())
        {
            ConIceDragonSeal iceSeal = owner.GetCondition<ConIceDragonSeal>();
            if (iceSeal != null)
            {
                // 基础power增加 (10 + ConIceDragonSeal的Tick) / Base power increases by (10 + ConIceDragonSeal ticks) / 基本powerを(10 + ConIceDragonSealのTick)増加
                bonusPower += 10 + iceSeal.value;
            }
        }
        
        hotDice = Dice.Create("SpHOT", bonusPower);
        diceInitialized = true;
    }

    public override void OnStart()
    {
        InitDice(); 
        
        // 检查是否存在冰龙封印BUFF / Check if Ice Dragon Seal buff exists / 氷龍封印BUFFが存在するかチェック
        int elementBonus = power;
        if (owner.HasCondition<ConIceDragonSeal>())
        {
            ConIceDragonSeal iceSeal = owner.GetCondition<ConIceDragonSeal>();
            if (iceSeal != null)
            {
                // owner.elements的power乘以 (1 + ConIceDragonSeal的Tick) / owner.elements power multiplied by (1 + ConIceDragonSeal ticks) / owner.elementsのpowerを(1 + ConIceDragonSealのTick)倍
                elementBonus = (int)(power * (1 + iceSeal.value));
            }
        }
        
        owner.elements.ModBase(67, elementBonus);
    }

    public override void Tick()
    {
        InitDice();
        if (hotDice != null)
        {
            owner.HealHPHost(hotDice.Roll(), HealSource.HOT);
        }

        Mod(-1);  
    }

    public override void OnWriteNote(List<string> list)
    {
        if (hotDice != null)
        {
            // 三语显示HOT信息 / Display HOT info in three languages / 三言語でHOT情報を表示
            list.Add($"<color=green>持续恢复生命/HOT Healing: +{hotDice}</color>");
            list.Add($"<color=green>持続回復: +{hotDice}</color>");
        }
    
        // 显示普通的伤害加成信息 / Display normal damage bonus info / 通常のダメージボーナス情報を表示
        list.Add($"<color=orange>伤害加成/Damage Bonus: +{power}</color>");
        list.Add($"<color=orange>ダメージボーナス: +{power}</color>");
        
        // 显示冰龙封印加成信息 / Display Ice Dragon Seal bonus info / 氷龍封印ボーナス情報を表示
        if (owner.HasCondition<ConIceDragonSeal>())
        {
            ConIceDragonSeal iceSeal = owner.GetCondition<ConIceDragonSeal>();
            if (iceSeal != null)
            {
                list.Add($"<color=blue>冰龙封印加成: HOT+{10 + iceSeal.value}, 伤害×{1 + iceSeal.value}</color>");
                list.Add($"<color=blue>Ice Dragon Bonus: HOT+{10 + iceSeal.value}, Damage×{1 + iceSeal.value}</color>");
                list.Add($"<color=blue>氷龍封印ボーナス: HOT+{10 + iceSeal.value}, ダメージ×{1 + iceSeal.value}</color>");
            }
        }
    }
    
    public override int GetPhase() => 0; 
}
}
