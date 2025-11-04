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

namespace Condition_Noraml
//为什么有些注释只有英文和日文，你不要问，问就是中文和日文部分大差不差能看出来。
{
    public class ConGBFEarth1 : Timebuff
    //大地の刹那
    //Mountain's Celere
    {
        public override int GetPhase() => 0;
    }
    
    public class ConGBFEarth2 : Timebuff
    //Terra's Aegis
    //地裂の守護
    {
        public override int GetPhase() => 0;
    }
    
    public class ConGBFEarth3 : Timebuff
    //大地の守護
    //Mountain's Aegis
    {
        public override int GetPhase() => 0;
    }
    
    public class ConGBFFire1 : Timebuff
    //Inferno's Aegis
    //紅蓮の守護
    {
        public override int GetPhase() => 0;
    }
    
    public class ConGBFWater1 : Timebuff
    //Hoarfrost's Might
    //霧氷の攻刃
    {
        public override int GetPhase() => 0;
    }
    
    public class ConGBFWind1 : Timebuff
    //乱気の技巧
    //Ventosus's Verity
    {
        public override int GetPhase() => 0;
    }
    
    public class ConGBFWind2 : Timebuff
    //Whirlwind's Might
    //竜巻の攻刃
    {
        public override int GetPhase() => 0;
    }
    
    public class ConGBFDark1 : Timebuff
    //憎悪の刹那
    //Hatred's Celere
    {
        public override int GetPhase() => 0;
    }
    
    public class ConGBFLight1 : Timebuff
    //光の攻刃
    //Light's Might
    {
        public override int GetPhase() => 0;
    }
    
    public class ConGBFLight2 : Timebuff
    //騎解方陣 神威
    //Knightcode's Majesty
    {
        public override int GetPhase() => 0;
    }
    
    public class ConGBFLight3 : Timebuff
    //騎解方陣 攻刃III
    //Knightcode's Might III
    {
        public override int GetPhase() => 0;
    }
    
    public class ConGBFLight4 : Timebuff
    //騎解方陣 守護II
    //Knightcode's Aegis II
    {
        public override int GetPhase() => 0;
    }
    
    public class ConSK1641 : Timebuff
    //良運招虎
    //Tiger's Prosperity
    {
        public override int GetPhase() => 0;
    }
    
    public class ConSK1642 : Timebuff
    //双虎風水
    //Twin Tiger Feng Shui
    {
        public override int GetPhase() => 0;
    }
    
    public class ConSK2508 : Timebuff
    //Venom and Vigor
    //常山蛇勢
    {
        public override int GetPhase() => 0;
    }
    
    public class ConSK1460 : Timebuff
    //槿花泡影
    //Effervescent Hibiscus
    {
        public override int GetPhase() => 0;
    }
    
    public class ConSK1461 : Timebuff
    //寂寞無為
    //Isolated Idling
    {
        public override int GetPhase() => 0;
    }
    
    public class ConSK1462 : Timebuff
    //花風・薄紅舞
    //Dance of Pink Petals
    {
        public override int GetPhase() => 0;
    }
    
    public class ConGBFWind3 : Timebuff
    //Ventosus's Majesty
    //乱気の神威
    {
        public override int GetPhase() => 0;
    }
    
    public class ConGBFWind4 : Timebuff
    //Wind's Aegis
    //風の守護
    {
        public override int GetPhase() => 0;
    }
    
    public class ConGBF_Sin_SK1 : Timebuff
    //Vigilante's Nature
    //奸雄の賦性
    //奸雄的赋性
    {
        public override void Tick()
        {
            // 基于DEX属性计算元素加成值 / Calculate element bonus based on DEX attribute / DEX属性に基づいてエレメントボーナスを計算
            int num = Mathf.Max(owner.DEX / 10, 1);  // 最小值为1 / Minimum value is 1 / 最小値は1
            // 设置元素414的基础值 / Set base value for element 414 / エレメント414の基本値を設定
            owner.elements.SetBase(414, num);
            // 减少持续时间 / Reduce duration / 持続時間を減少
            Mod(-1);
        }
    }
    
    public class ConSK0534 : Timebuff
    //犬牙相制
    //Canid Growl
    {
        public override int GetPhase() => 0;
    }

    public class ConSK0535 : Timebuff
    //飛鷹走狗
    //Hound of the Hunt
    {
        public override int GetPhase() => 0;
    }
    
    public class ConSK2743 : Timebuff
    //腕白小犬
    //Daredoggy Mischief
    {
        public override int GetPhase() => 0;
    }
    
    public class ConSK1786 : Timebuff
    //戌遊戌予
    //Work Hard, Play Harder
    {
        public override int GetPhase() => 0;
    }
    
    public class ConSK1787 : Timebuff
    //犬心一意
    //Hounding Focus
    {
        public override int GetPhase() => 0;
    }
    
    public class ConGBFDark2 : Timebuff
    {
        public override int GetPhase() => 0;
    }
    
    public class ConGBFWater2 : Timebuff
    {
        public override int GetPhase() => 0;
    }
    
    public class ConGBFLight5 : Timebuff
    {
        public override int GetPhase() => 0;
    }
    
    public class ConGBFLight6 : Timebuff
    {
        public override int GetPhase() => 0;
    }
    
    public class ConPCR_SpeedUp : Timebuff
    {
        public override int GetPhase() => 0;
    }
    
    public class ConSnow_Boots : Timebuff
    {
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
    
    public class ConSnow_Boots2 : Timebuff
    {
        public override int GetPhase() => 0;
    }
}
