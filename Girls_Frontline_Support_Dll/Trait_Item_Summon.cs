using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Reflection.Emit;
using UnityEngine;
using Cwl.API.Processors;
using Microsoft.CodeAnalysis;
using HarmonyLib;
using BepInEx;
using BepInEx.Configuration;
using Cwl;
using Cwl.API.Custom;
using Cwl.LangMod;
using Cwl.Helper;
using Newtonsoft.Json;
using DG.Tweening.Plugins;

namespace Trait_Item_Summon
{
    internal class TraitMP5Summon : TraitItem
    {
        public override bool OnUse(Chara c)
        {
            // 检查是否为玩家角色 / Check if it's a player character / プレイヤーキャラクターか確認
            if (!c.IsPC)
            {
                c.Say("GFMP52", null, null);  // 非玩家使用提示 / Non-player usage prompt / 非プレイヤー使用時のメッセージ
                return false;
            }
        
            // 检查GFMP5是否已存在 / Check if GFMP5 already exists / GFMP5が既に存在するか確認
            Chara chara = EClass.game.cards.globalCharas.Values.FirstOrDefault((Chara gc) => gc.id == "GFMP5");
            if (chara != null)
            {
                this.owner.ModNum(-1, true);  // 消耗物品但召唤失败 / Consume item but summon fails / アイテム消費するが召喚失敗
                return false;
            }
        
            // 创建标记角色GFMP5 / Create tagged character GFMP5 / タグ付きキャラクターGFMP5を作成
            if (!CustomChara.CreateTaggedChara("GFMP5", out chara, new string[]
            {
                "GF_MP5#Mythical"  // 神话品质标签 / Mythical quality tag / 神話品質タグ
            }, null) || chara == null)
            {
                c.Say("GFMP53", null, null);  // 创建失败提示 / Creation failure prompt / 作成失敗メッセージ
                if (chara != null)
                {
                    chara.Destroy();  // 清理失败创建的角色 / Clean up failed character creation / 失敗したキャラ作成をクリーンアップ
                }
                this.owner.ModNum(-1, true);  // 消耗物品 / Consume item / アイテム消費
                return false;
            }
        
            // 成功创建后的处理 / Processing after successful creation / 作成成功後の処理
            EClass._zone.AddCard(chara, c.pos.GetNearestPoint(false, false, true, false)); // 添加到最近可用位置 / Add to nearest available position / 最寄りの利用可能位置に追加
            chara.MakeAlly(true);                                                          // 设置为盟友 / Set as ally / 味方に設定
            chara.PlayEffect("teleport", true, 0f, default(Vector3));                      // 播放传送特效 / Play teleport effect / テレポートエフェクトを再生
            this.owner.ModNum(-1, true);                                                   // 消耗物品 / Consume item / アイテム消費
            return true;                                                                   // 使用成功 / Usage successful / 使用成功
        }
    }
    
    internal class TraitMP5SummonSkin1 : TraitItem
    {

        public override bool OnUse(Chara c)
        {
            if (!c.IsPC)
            {
                c.Say("GFMP52", null, null);
                return false;
            }
            Chara chara = EClass.game.cards.globalCharas.Values.FirstOrDefault((Chara gc) => gc.id == "GFMP5ssz");
            if (chara != null)
            {
                this.owner.ModNum(-1, true);
                return false;
            }
            if (!CustomChara.CreateTaggedChara("GFMP5ssz", out chara, new string[]
            {
                "GF_MP5#Mythical"
            }, null) || chara == null)
            {
                c.Say("GFMP53", null, null);
                if (chara != null)
                {
                    chara.Destroy();
                }
                this.owner.ModNum(-1, true);
                return false;
            }
            EClass._zone.AddCard(chara, c.pos.GetNearestPoint(false, false, true, false));
            chara.MakeAlly(true);
            chara.PlayEffect("teleport", true, 0f, default(Vector3));
            this.owner.ModNum(-1, true);
            return true;
        }
    }
    
    internal class TraitMP5SummonSkin2 : TraitItem
    {

        public override bool OnUse(Chara c)
        {
            if (!c.IsPC)
            {
                c.Say("GFMP52", null, null);
                return false;
            }
            Chara chara = EClass.game.cards.globalCharas.Values.FirstOrDefault((Chara gc) => gc.id == "GFMP5aysm");
            if (chara != null)
            {
                this.owner.ModNum(-1, true);
                return false;
            }
            if (!CustomChara.CreateTaggedChara("GFMP5aysm", out chara, new string[]
            {
                "GF_MP5#Mythical"
            }, null) || chara == null)
            {
                c.Say("GFMP53", null, null);
                if (chara != null)
                {
                    chara.Destroy();
                }
                this.owner.ModNum(-1, true);
                return false;
            }
            EClass._zone.AddCard(chara, c.pos.GetNearestPoint(false, false, true, false));
            chara.MakeAlly(true);
            chara.PlayEffect("teleport", true, 0f, default(Vector3));
            this.owner.ModNum(-1, true);
            return true;
        }
    }
    
    internal class TraitMP5SummonSkin3 : TraitItem
    {

        public override bool OnUse(Chara c)
        {
            if (!c.IsPC)
            {
                c.Say("GFMP52", null, null);
                return false;
            }
            Chara chara = EClass.game.cards.globalCharas.Values.FirstOrDefault((Chara gc) => gc.id == "GFMP5hwm");
            if (chara != null)
            {
                this.owner.ModNum(-1, true);
                return false;
            }
            if (!CustomChara.CreateTaggedChara("GFMP5hwm", out chara, new string[]
            {
                "GF_MP5#Mythical"
            }, null) || chara == null)
            {
                c.Say("GFMP53", null, null);
                if (chara != null)
                {
                    chara.Destroy();
                }
                this.owner.ModNum(-1, true);
                return false;
            }
            EClass._zone.AddCard(chara, c.pos.GetNearestPoint(false, false, true, false));
            chara.MakeAlly(true);
            chara.PlayEffect("teleport", true, 0f, default(Vector3));
            this.owner.ModNum(-1, true);
            return true;
        }
    }
    
    internal class TraitMP5SummonSkin4 : TraitItem
    {

        public override bool OnUse(Chara c)
        {
            if (!c.IsPC)
            {
                c.Say("GFMP52", null, null);
                return false;
            }
            Chara chara = EClass.game.cards.globalCharas.Values.FirstOrDefault((Chara gc) => gc.id == "GFMP5zacq");
            if (chara != null)
            {
                this.owner.ModNum(-1, true);
                return false;
            }
            if (!CustomChara.CreateTaggedChara("GFMP5zacq", out chara, new string[]
            {
                "GF_MP5#Mythical"
            }, null) || chara == null)
            {
                c.Say("GFMP53", null, null);
                if (chara != null)
                {
                    chara.Destroy();
                }
                this.owner.ModNum(-1, true);
                return false;
            }
            EClass._zone.AddCard(chara, c.pos.GetNearestPoint(false, false, true, false));
            chara.MakeAlly(true);
            chara.PlayEffect("teleport", true, 0f, default(Vector3));
            this.owner.ModNum(-1, true);
            return true;
        }
    }
    
    internal class TraitMP5SummonSkin5 : TraitItem
    {

        public override bool OnUse(Chara c)
        {
            if (!c.IsPC)
            {
                c.Say("GFMP52", null, null);
                return false;
            }
            Chara chara = EClass.game.cards.globalCharas.Values.FirstOrDefault((Chara gc) => gc.id == "GFMP5zjmwhs");
            if (chara != null)
            {
                this.owner.ModNum(-1, true);
                return false;
            }
            if (!CustomChara.CreateTaggedChara("GFMP5zjmwhs", out chara, new string[]
            {
                "GF_MP5#Mythical"
            }, null) || chara == null)
            {
                c.Say("GFMP53", null, null);
                if (chara != null)
                {
                    chara.Destroy();
                }
                this.owner.ModNum(-1, true);
                return false;
            }
            EClass._zone.AddCard(chara, c.pos.GetNearestPoint(false, false, true, false));
            chara.MakeAlly(true);
            chara.PlayEffect("teleport", true, 0f, default(Vector3));
            this.owner.ModNum(-1, true);
            return true;
        }
    }
    
    internal class TraitMP5SummonLove : TraitItem
    {

        public override bool OnUse(Chara c)
        {
            if (!c.IsPC)
            {
                c.Say("GFMP52", null, null);
                return false;
            }
            Chara chara = EClass.game.cards.globalCharas.Values.FirstOrDefault((Chara gc) => gc.id == "GFMP5Love");
            if (chara != null)
            {
                this.owner.ModNum(-1, true);
                return false;
            }
            if (!CustomChara.CreateTaggedChara("GFMP5Love", out chara, new string[]
            {
                "GF_MP5#Mythical"
            }, null) || chara == null)
            {
                c.Say("GFMP53", null, null);
                if (chara != null)
                {
                    chara.Destroy();
                }
                this.owner.ModNum(-1, true);
                return false;
            }
            EClass._zone.AddCard(chara, c.pos.GetNearestPoint(false, false, true, false));
            chara.MakeAlly(true);
            chara.PlayEffect("teleport", true, 0f, default(Vector3));
            this.owner.ModNum(-1, true);
            EClass.pc.Pick(ThingGen.Create("GF_MP5_gift", -1, -1), true, true);
            return true;
        }
    }
    
    internal class TraitMP5SummonMOD3 : TraitItem
    {

        public override bool OnUse(Chara c)
        {
            if (!c.IsPC)
            {
                c.Say("GFMP52", null, null);
                return false;
            }
            Chara chara = EClass.game.cards.globalCharas.Values.FirstOrDefault((Chara gc) => gc.id == "GFMP5MOD3");
            if (chara != null)
            {
                this.owner.ModNum(-1, true);
                return false;
            }
            if (!CustomChara.CreateTaggedChara("GFMP5MOD3", out chara, new string[]
            {
                "GF_MP5#Mythical"
            }, null) || chara == null)
            {
                c.Say("GFMP53", null, null);
                if (chara != null)
                {
                    chara.Destroy();
                }
                this.owner.ModNum(-1, true);
                return false;
            }
            EClass._zone.AddCard(chara, c.pos.GetNearestPoint(false, false, true, false));
            chara.MakeAlly(true);
            chara.PlayEffect("teleport", true, 0f, default(Vector3));
            this.owner.ModNum(-1, true);
            return true;
        }
    }

    internal class TraitTPSSummon : TraitItem
    {

        public override bool OnUse(Chara c)
        {
            if (!c.IsPC)
            {

                return false;
            }
            Chara chara = EClass.game.cards.globalCharas.Values.FirstOrDefault((Chara gc) => gc.id == "GFTPS");
            if (chara != null)
            {
                this.owner.ModNum(-1, true);
                return false;
            }
            if (!CustomChara.CreateTaggedChara("GFTPS", out chara, new string[]
            {
                "GF_TPS#Mythical"
            }, null) || chara == null)
            {

                if (chara != null)
                {
                    chara.Destroy();
                }
                this.owner.ModNum(-1, true);
                return false;
            }
            EClass._zone.AddCard(chara, c.pos.GetNearestPoint(false, false, true, false));
            chara.MakeAlly(true);
            chara.PlayEffect("teleport", true, 0f, default(Vector3));
            this.owner.ModNum(-1, true);
            return true;
        }
    }
    
    internal class TraitTPSSummonSkin1 : TraitItem
    {

        public override bool OnUse(Chara c)
        {
            if (!c.IsPC)
            {

                return false;
            }
            Chara chara = EClass.game.cards.globalCharas.Values.FirstOrDefault((Chara gc) => gc.id == "GFTPSxbtyx");
            if (chara != null)
            {
                this.owner.ModNum(-1, true);
                return false;
            }
            if (!CustomChara.CreateTaggedChara("GFTPSxbtyx", out chara, new string[]
            {
                "GF_TPS#Mythical"
            }, null) || chara == null)
            {

                if (chara != null)
                {
                    chara.Destroy();
                }
                this.owner.ModNum(-1, true);
                return false;
            }
            EClass._zone.AddCard(chara, c.pos.GetNearestPoint(false, false, true, false));
            chara.MakeAlly(true);
            chara.PlayEffect("teleport", true, 0f, default(Vector3));
            this.owner.ModNum(-1, true);
            return true;
        }
    }
}
