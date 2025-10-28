using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using Cwl.API.Custom;
using HarmonyLib;
using UnityEngine;

namespace GBF.trait.TraitItem_Summon
{
    /// 由于工作原理大差不差，我只会注释第一个内容。
    /// Since the working principles are quite similar, I will only comment on the first content.
    /// 動作原理がほぼ同じため、最初の内容のみに注釈を付けます。
    /// 如果是道具召唤友军，看TraitCidalaSummon，如果是道具召唤敌军，看TraitLuminieraSummon
    /// If using items to summon allies, refer to TraitCidalaSummon; if using items to summon enemies, refer to TraitLuminieraSummon
    /// アイテムで味方を召喚する場合はTraitCidalaSummonを参照し、アイテムで敵を召喚する場合はTraitLuminieraSummonを参照してください

   internal class TraitCidalaSummon : TraitItem
{
    // 使用特质物品召唤角色 / Use trait item to summon character / 特性アイテムを使用してキャラを召喚
    public override bool OnUse(Chara c)
    {
        // 检查是否为玩家角色 / Check if it's a player character / プレイヤーキャラか確認
        if (!c.IsPC)
        {
            c.SayNothingHappans();  // 非玩家无法使用提示 / Non-player usage prompt / 非プレイヤー使用不可メッセージ
            return false;
        }
        
        // 检查Cidala是否已存在 / Check if Cidala already exists / Cidalaが既に存在するか確認
        Chara chara = EClass.game.cards.globalCharas.Values.FirstOrDefault((Chara gc) => gc.id == "Cidala");
        if (chara != null)
        {
            c.Say("Cidala1", null, null);  // 角色已存在提示 / Character already exists prompt / キャラ既存メッセージ
            this.owner.ModNum(-1, true);   // 消耗物品但召唤失败 / Consume item but summon fails / アイテム消費するが召喚失敗
            return false;
        }
        
        // 创建标记的Cidala角色 / Create tagged Cidala character / タグ付きCidalaキャラを作成
        if (!CustomChara.CreateTaggedChara("Cidala", out chara, new string[]
        {
            "GBF_Tigrisius#Artifact"  // 神器品质标签 / Artifact quality tag / アーティファクト品質タグ
        }, null) || chara == null)
        {
            c.Say("Cidala2", null, null);  // 创建失败提示 / Creation failure prompt / 作成失敗メッセージ
            if (chara != null)
            {
                chara.Destroy();  // 清理失败创建的角色 / Clean up failed character creation / 失敗したキャラ作成をクリーンアップ
            }
            this.owner.ModNum(-1, true);  // 消耗物品 / Consume item / アイテム消費
            return false;
        }
        
        // 成功创建后的处理 / Processing after successful creation / 作成成功後の処理
        EClass._zone.AddCard(chara, c.pos.GetNearestPoint(false, false, true, false));  // 添加到最近可用位置 / Add to nearest available position / 最寄りの利用可能位置に追加
        chara.MakeAlly(true);        // 设置为盟友 / Set as ally / 味方に設定
        chara.PlayEffect("teleport", true, 0f, default(Vector3));  // 播放传送特效 / Play teleport effect / テレポートエフェクトを再生
        this.owner.ModNum(-1, true); // 消耗物品 / Consume item / アイテム消費
        return true;  // 使用成功 / Usage successful / 使用成功
    }
}
    internal class TraitNarmayaSummon1 : TraitItem
    {
        public override bool OnUse(Chara c)
        {
            if (!c.IsPC)
            {
                c.SayNothingHappans();
                return false;
            }
            Chara chara = EClass.game.cards.globalCharas.Values.FirstOrDefault((Chara gc) => gc.id == "NarmayaWind");
            if (chara != null)
            {
                c.Say("Narmaya1", null, null);
                this.owner.ModNum(-1, true);
                return false;
            }
            if (!CustomChara.CreateTaggedChara("NarmayaWind", out chara, new string[]
            {
                "GBF_Evanescence2#Artifact"
            }, null) || chara == null)
            {
                c.Say("Narmaya2", null, null);
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
    internal class TraitCagliostroClarisseSummon : TraitItem
    {
        public override bool OnUse(Chara c)
        {
            if (!c.IsPC)
            {
                c.SayNothingHappans();
                return false;
            }
            Chara chara = EClass.game.cards.globalCharas.Values.FirstOrDefault((Chara gc) => gc.id == "Cagliostro_Clarisse");
            if (chara != null)
            {
                c.Say("Cagliostro_Clarisse1", null, null);
                this.owner.ModNum(-1, true);
                return false;
            }
            if (!CustomChara.CreateTaggedChara("Cagliostro_Clarisse", out chara, new string[]
            {
                "GBF_Ouroboric2#Artifact"
            }, null) || chara == null)
            {
                c.Say("Cagliostro_Clarisse2", null, null);
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
    internal class TraitBerceauSummon : TraitItem
    {
        public override bool OnUse(Chara c)
        {
            if (!c.IsPC)
            {
                c.SayNothingHappans();
                return false;
            }
            Chara chara = EClass.game.cards.globalCharas.Values.FirstOrDefault((Chara gc) => gc.id == "Berceau");
            if (chara != null)
            {
                c.Say("Berceau1", null, null);
                this.owner.ModNum(-1, true);
                return false;
            }
            if (!CustomChara.CreateTaggedChara("Berceau", out chara, new string[]
            {
                "GBF_Epee_Scintillante2#Artifact"
            }, null) || chara == null)
            {
                c.Say("Berceau2", null, null);
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
    internal class TraitLuminieraSummon : TraitItem
{
    // 使用特质物品召唤敌方BOSS / Use trait item to summon enemy BOSS / 特性アイテムを使用して敵BOSSを召喚
    public override bool OnUse(Chara c)
    {
        // 检查是否为玩家角色 / Check if it's a player character / プレイヤーキャラか確認
        if (!c.IsPC)
        {
            c.SayNothingHappans();  // 非玩家无法使用提示 / Non-player usage prompt / 非プレイヤー使用不可メッセージ
            return false;
        }
        
        // 检查BOSS是否已存在 / Check if BOSS already exists / BOSSが既に存在するか確認
        Chara chara = EClass.game.cards.globalCharas.Values.FirstOrDefault((Chara gc) => gc.id == "GBF_Boss_Luminiera_1");
        if (chara != null)
        {
            this.owner.ModNum(-1, true);  // 消耗物品但召唤失败 / Consume item but summon fails / アイテム消費するが召喚失敗
            return false;
        }
        
        // 创建BOSS角色（无特殊标签） / Create BOSS character (no special tags) / BOSSキャラを作成（特殊タグなし）
        if (!CustomChara.CreateTaggedChara("GBF_Boss_Luminiera_1", out chara, new string[]
        {
            // 空标签数组 - 敌方BOSS通常不需要特殊标签 / Empty tag array - Enemy BOSS usually doesn't need special tags / 空タグ配列 - 敵BOSSは通常特殊タグ不要
        }, null) || chara == null)
        {
            if (chara != null)
            {
                chara.Destroy();  // 清理失败创建的角色 / Clean up failed character creation / 失敗したキャラ作成をクリーンアップ
            }
            this.owner.ModNum(-1, true);  // 消耗物品 / Consume item / アイテム消費
            return false;
        }
        
        // 成功创建后的处理 / Processing after successful creation / 作成成功後の処理
        EClass._zone.AddCard(chara, c.pos.GetNearestPoint(false, false, true, false));  // 添加到最近可用位置 / Add to nearest available position / 最寄りの利用可能位置に追加
        chara.PlayEffect("teleport", true, 0f, default(Vector3));  // 播放传送特效 / Play teleport effect / テレポートエフェクトを再生
        this.owner.ModNum(-1, true);  // 消耗物品 / Consume item / アイテム消費
        
        // 注意：敌方BOSS不会设置为盟友 / Note: Enemy BOSS is not set as ally / 注意：敵BOSSは味方に設定されない
        return true;  // 使用成功 / Usage successful / 使用成功
    }
}
}
