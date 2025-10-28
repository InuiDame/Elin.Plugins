using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace GBF.trait.TraitItem_Quest
{

    // 次元达人任务书特质 / 次元達人任務書特性 / Dimensional Expert Mission Book Trait
public class Traitcidaladojin : TraitItem 
{
    // 控制物品是否可被摧毁 / アイテムが破壊可能か制御 / Control whether item can be destroyed
    public override bool CanBeDestroyed
    {
        get
        {
            // NPC财产不可摧毁，玩家财产可摧毁 / NPC財産は破壊不可、プレイヤー財産は破壊可能 / NPC property cannot be destroyed, player property can be destroyed
            if (!owner.isNPCProperty)
            {
                return base.CanBeDestroyed;
            }
            return false;
        }
    }

    // 不可被偷窃 / 盗難不可 / Cannot be stolen
    public override bool CanBeStolen => false;

    // 不可堆叠 / スタック不可 / Cannot stack
    public override bool CanStack => false;

    // 使用物品时调用 / アイテム使用時に呼び出される / Called when item is used
    public override bool OnUse(Chara c)
    {
        // 播放使用效果消息 / 使用効果メッセージを表示 / Display usage effect message
        Msg.Say("cidala_dojin_mission", owner);

        // 生成指定敌人组 / 指定敵グループを生成 / Summon specified enemy group
        SummonEnemyGroup();

        // 给予玩家物品 / プレイヤーにアイテムを付与 / Give items to player
        GivePlayerItems();

        // 播放音效 / サウンドを再生 / Play sound
        EClass.Sound.Play("tape");

        // 减少物品数量 / アイテム数を減少 / Decrease item count
        owner.ModNum(-1);

        return false;
    }

    /// <summary>
    /// 生成指定敌人组 - 使用SpawnSetting的静态方法 / 指定敵グループを生成 - SpawnSettingの静的メソッドを使用 / Summon specified enemy group - using SpawnSetting static methods
    /// </summary>
    private void SummonEnemyGroup()
    {
        // 获取生成点：物品位置或玩家位置 / スポーン地点取得：アイテム位置またはプレイヤー位置 / Get spawn point: item position or player position
        Point spawnPoint = owner.ExistsOnMap ? owner.pos : EClass.pc.pos;

        // 获取最近的可用生成点 / 最近の利用可能なスポーン地点を取得 / Get nearest available spawn point
        Point spawnPos = spawnPoint.GetNearestPoint(allowBlock: false, allowChara: false);

        int LV = Math.Max(Math.Min(owner.LV, 50), 20); // 设置等级为自身等级和50的最小值，但至少20级 / レベルを自身のレベルと50の最小値に設定、ただし最低20レベル / Set level to minimum of own level and 50, but at least level 20
        int BossLV = LV + 5;                                      // Boss等级比普通高5级 / Bossレベルは通常より5高い / Boss level 5 higher than normal

        // 定义要生成的敌人配置 / 生成する敵の設定を定義 / Define enemy spawn configurations
        var enemySpawns = new[]
        {
            new { Id = "merc", IsBoss = true },        // 佣兵Boss / 傭兵Boss / Mercenary Boss
            new { Id = "gangster", IsBoss = false },   // 黑帮成员 / ギャングメンバー / Gangster member
            new { Id = "gangster", IsBoss = false },   // 黑帮成员 / ギャングメンバー / Gangster member
            new { Id = "gangster", IsBoss = false }    // 黑帮成员 / ギャングメンバー / Gangster member
        };

        // 生成每个敌人 / 各敵を生成 / Spawn each enemy
        foreach (var enemy in enemySpawns)
        {
            SpawnSetting setting;

            if (enemy.IsBoss)
            {
                // 使用Boss生成设置 / Boss生成設定を使用 / Use Boss spawn setting
                setting = SpawnSetting.Boss(enemy.Id, null, BossLV);
                setting.hostility = SpawnHostility.Enemy;  // 设置为敌对 / 敵対的に設定 / Set as hostile
            }
            else
            {
                // 使用普通敌人生成设置 / 通常敵生成設定を使用 / Use normal enemy spawn setting
                setting = SpawnSetting.Mob(enemy.Id, null, LV);
                setting.hostility = SpawnHostility.Enemy;  // 设置为敌对 / 敵対的に設定 / Set as hostile
            }

            // 在地图上生成敌人 / マップ上に敵を生成 / Spawn enemy on map
            Chara spawnedEnemy = EClass._zone.SpawnMob(spawnPos, setting);
            if (spawnedEnemy != null)
            {
                // 播放传送特效 / テレポートエフェクトを再生 / Play teleport effect
                spawnedEnemy.PlayEffect("teleport");
            }
        }
    }

    // 给予玩家物品 / プレイヤーにアイテムを付与 / Give items to player
    private void GivePlayerItems()
    {
        // 播放获得道具的音效 / アイテム獲得音效を再生 / Play item acquisition sound effect
        SE.Play("dropReward");

        // 直接使用Pick方法给玩家道具 / Pickメソッドで直接プレイヤーにアイテムを付与 / Use Pick method to directly give item to player
        EClass.pc.Pick(ThingGen.Create("cidaladojin2"));
    }
}

// 次元达人任务书2特质 / 次元達人任務書2特性 / Dimensional Expert Mission Book 2 Trait
public class Traitcidaladojin2 : TraitItem 
{
    // 不可被摧毁 / 破壊不可 / Cannot be destroyed
    public override bool CanBeDestroyed => false;

    // 不可被偷窃 / 盗難不可 / Cannot be stolen
    public override bool CanBeStolen => false;

    // 人物对象 / 人物オブジェクト / Person object
    public Person person;
    
    // 角色对象 / キャラクターオブジェクト / Character object
    public Chara chara => person.chara;
    
    // 使用物品时调用 / アイテム使用時に呼び出される / Called when item is used
    public override bool OnUse(Chara c) 
    {
        if (c == null)
        {
            // 创建新人物对象 / 新規人物オブジェクトを作成 / Create new person object
            person = new Person();
            return true;
        }
        
        // 从角色创建人物对象 / キャラクターから人物オブジェクトを作成 / Create person object from character
        person = new Person(c);
        
        // 显示对话 / ダイアログを表示 / Show dialog
        person.chara.ShowDialog("Cidala", "really_cidala_dojinbook");
        return true;
    }
}
}
