using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using UnityEngine;
using Cwl.API.Processors;
using Microsoft.CodeAnalysis;
using HarmonyLib;
using BepInEx;
using BepInEx.Configuration;
using Cwl;
using Cwl.API.Custom;
using BS.magicshop;
using Cwl.LangMod;
using static QuestCraft;
using System.Reflection;

namespace Trait_Item_Summon
{
    internal class TraitSummonBS2 : TraitItem
    {
        // 当使用特质物品时 / When using trait item
        public override bool OnUse(Chara c)
        {
            // 只有玩家角色可以使用 / Only player characters can use
            if (!c.IsPC)
            {
                c.SayNothingHappans();
                return false;
            }
        
            // 检查是否已存在该BOSS / Check if the BOSS already exists
            Chara chara = EClass.game.cards.globalCharas.Values.FirstOrDefault((Chara gc) => gc.id == "BSBOSS_2");
            if (chara != null)
            {
                this.owner.ModNum(-1, true);
                return false;
            }
        
            // 创建标记的角色 / Create tagged character
            if (!CustomChara.CreateTaggedChara("BSBOSS_2", out chara, new string[]
            {
                "BSBOSS_2T2#Artifact"
            }, null) || chara == null)
            {
                // 创建失败时清理并消耗物品 / Clean up and consume item when creation fails
                if (chara != null)
                {
                    chara.Destroy();
                }
                this.owner.ModNum(-1, true);
                return false;
            }
        
            // 将角色添加到最近的有效位置 / Add character to nearest valid position
            EClass._zone.AddCard(chara, c.pos.GetNearestPoint(false, false, true, false));
        
            // 消耗物品 / Consume item
            this.owner.ModNum(-1, true);
            return true;
        }
    }

}