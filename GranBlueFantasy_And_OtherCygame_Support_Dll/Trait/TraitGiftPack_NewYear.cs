namespace GBF.trait.TraitGiftPack_NewYear
{
    public class TraitGiftBSNewYear : TraitGiftPack
{
    // 使用新年礼物包 / Use New Year gift pack / 新年ギフトパックを使用
    public override bool OnUse(Chara c)
    {
        // 检查是否在区域中 / Check if in region / リージョン内か確認
        if (EClass._zone.IsRegion)
        {
            Msg.SayCannotUseHere();  // 无法在此使用提示 / Cannot use here prompt / ここでは使用不可メッセージ
            return false;
        }
        
        // 播放开启音效和语音 / Play opening sound and voice / 開封音声と音声を再生
        EClass.pc.Say("openDoor", EClass.pc, this.owner, null, null);
        SE.Play("dropReward");  // 播放奖励掉落音效 / Play reward drop sound / 報酬ドロップ音声を再生
        
        // 给予6个eg_sota物品 / Give 6 eg_sota items / eg_sotaアイテムを6個付与
        EClass.pc.Pick(ThingGen.Create("eg_sota", -1, -1), true, true);
        EClass.pc.Pick(ThingGen.Create("eg_sota", -1, -1), true, true);
        EClass.pc.Pick(ThingGen.Create("eg_sota", -1, -1), true, true);
        EClass.pc.Pick(ThingGen.Create("eg_sota", -1, -1), true, true);
        EClass.pc.Pick(ThingGen.Create("eg_sota", -1, -1), true, true);
        EClass.pc.Pick(ThingGen.Create("eg_sota", -1, -1), true, true);
        
        // 给予特殊物品 / Give special items / 特殊アイテムを付与
        EClass.pc.Pick(ThingGen.Create("GBF_Serpentius", -1, -1), true, true);  // 蛇神武器 / Serpentius weapon / 蛇神武器
        EClass.pc.Pick(ThingGen.Create("Festival_ChineseNewyear", -1, -1), true, true);  // 春节物品 / Chinese New Year item / 春節アイテム
        EClass.pc.Pick(ThingGen.Create("Festival_ChineseNewyear", -1, -1), true, true);
        EClass.pc.Pick(ThingGen.Create("Festival_ChineseNewyear", -1, -1), true, true);
        EClass.pc.Pick(ThingGen.Create("Festival_ChineseNewyear", -1, -1), true, true);
        
        // 召唤Indala角色 / Summon Indala character / Indalaキャラを召喚
        Chara chara = CharaGen.Create("Indala", -1);
        EClass._zone.AddCard(chara, (this.owner.ExistsOnMap ? this.owner.pos : EClass.pc.pos).GetNearestPoint(false, false, true, false));
        Msg.Say("package_chara", chara, this.owner, null, null);  // 角色出现提示 / Character appearance prompt / キャラ出現メッセージ
        chara.MakeAlly(true);  // 设置为盟友 / Set as ally / 味方に設定
        
        // 生成3个银铃角色 / Generate 3 silver bell characters / 銀の鈴キャラを3体生成
        for (int i = 0; i < 3; i++)
        {
            chara = CharaGen.Create("bell_silver", -1);
            chara.c_originalHostility = (chara.hostility = Hostility.Neutral);  // 设置为中立敌意 / Set to neutral hostility / 中立敵意に設定
            EClass._zone.AddCard(chara, EClass._map.GetRandomSurface(false, true, false));  // 随机位置添加 / Add at random position / ランダム位置に追加
        }
        
        this.owner.ModNum(-1, true);  // 消耗物品 / Consume item / アイテム消費
        return true;  // 使用成功 / Usage successful / 使用成功
    }
}
}
