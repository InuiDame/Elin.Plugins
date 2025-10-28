using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace GBF.trait.TraitItem_GBFChest
{
    // 宝箱特质基类 / 宝箱特性基类 / Chest Trait Base Class
public class TraitGBFChestBase : TraitItem
{
    // 武器基础效果ID数组 / 武器基础效果ID配列 / Weapon base effect ID array
    private static readonly int[] Effect_Weapon = new int[2] { 66, 67 };
    
    // 武器附加效果ID数组 / 武器追加效果ID配列 / Weapon additional effect ID array
    private static readonly int[] Effect_Weapon_sub = new int[26] { 380, 381, 382, 435, 436, 437, 438, 460, 461, 462, 463, 464, 465, 466, 467, 468, 608, 609, 620, 621, 622, 623, 624, 90, 91, 92 };
    
    // 护甲基础效果ID数组 / 防具基础效果ID配列 / Armor base effect ID array
    private static readonly int[] Effect_Armor = new int[2] { 64, 65 };
    
    // 护甲附加效果ID数组 / 防具追加效果ID配列 / Armor additional effect ID array
    private static readonly int[] Effect_Armor_sub = new int[15] { 950, 951, 952, 953, 954, 955, 956, 957, 958, 959, 960, 961, 962, 963, 964 };
    
    // 战斗效果ID数组 / 战斗效果ID配列 / Battle effect ID array
    private static readonly int[] Effect_Battle = new int[52] {  70, 71, 72, 73, 74, 75, 76, 77, 79, 80, 93, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 120, 122, 123, 130, 131, 132, 133, 134, 135, 150, 151, 152, 300, 301, 302, 303, 304, 305, 414, 440, 441, 442, 443, 444, 445, 446, 447, 450, 666 };
    
    // 通用效果ID数组 / 共通效果ID配列 / Common effect ID array
    private static readonly int[] Effect_Common = new int[35] { 200, 207, 210, 220, 225, 226, 227, 230, 235, 237, 240, 241, 242, 245, 250, 255, 256, 257, 258, 259, 260, 261, 280, 281, 285, 286, 287, 288, 289, 290, 291, 292, 293, 306, 307 };
    
    // 稀有度效果ID数组 / レアリティ效果ID配列 / Rarity effect ID array
    private static readonly int[] Effect_Rarity = new int[2] { 411, 482 };
    
    // 证明效果ID数组 / 证明效果ID配列 / Proof effect ID array
    private static readonly int[] Effect_Proof = new int[2] { 50, 51 };
    
    // 抵消效果ID数组 / 无効化效果ID配列 / Negate effect ID array
    private static readonly int[] Effect_Negate = new int[10] { 420, 421, 422, 423, 424, 425, 426, 427, 430, 437 };
    
    // 元素效果ID数组 / 元素效果ID配列 / Element effect ID array
    private static readonly int[] Effect_Ele = new int[12] { 910, 911, 912, 913, 914, 915, 916, 917, 918, 919, 920, 921 };  

    // 使用宝箱时调用 / 宝箱使用時に呼び出される / Called when chest is used
    public override bool OnUse(Chara c)
    {
        // 检查是否在区域中 / 地域内かチェック / Check if in region
        if (EClass._zone.IsRegion)
        {
            Msg.SayCannotUseHere();
            return false;
        }

        int originalLv = c.LV;
        
        // 查找队伍中最高等级的角色 / パーティ内の最高レベルキャラを検索 / Find highest level character in party
        Chara chara = c.party._members.FindMax((Chara chara2) => chara2.IsPC ? originalLv : chara2.LV);
        if (!chara.IsPC)
        {
            originalLv = chara.LV;
        }
        
        // 限制最大输入等级 / 最大入力レベルを制限 / Limit maximum input level
        int maxInputLevel = 1000;
        originalLv = Math.Min(originalLv, maxInputLevel);
        
        // 计算有效等级 / 有效レベルを計算 / Calculate effective level
        int effectiveLv = CalculateEffectiveLevel(originalLv);

        // 播放音效 / サウンド再生 / Play sound
        SE.Play("dropReward");

        // 生成装备 / 装備を生成 / Generate equipment
        int num = owner.Num;
        for (int num2 = 0; num2 < num; num2++)
        {
            SpawnEq(effectiveLv);
            owner.ModNum(-1);
        }

        return true;
    }

    // 计算有效等级 / 有效レベルを計算 / Calculate effective level
    private int CalculateEffectiveLevel(int originalLevel)
    {
        if (originalLevel <= 100)
        {
            // 100级及以下：线性增长 / 100レベル以下：線形成長 / Level 100 and below: linear growth
            return originalLevel;
        }
        else if (originalLevel <= 200)
        {
            // 100-200级：每1级取2，即增长减半 / 100-200レベル：1レベル毎に2を取る（成長半減） / Level 100-200: take 2 for every level (half growth)
            return 100 + (originalLevel - 100) / 2;
        }
        else if (originalLevel <= 300)
        {
            // 200-300级：每2级取3，增长更慢 / 200-300レベル：2レベル毎に3を取る（成長更に遅い） / Level 200-300: take 3 for every 2 levels (slower growth)
            return 150 + (originalLevel - 200) * 3 / 2;
        }
        else if (originalLevel <= 500)
        {
            // 300-500级：每3级取1 / 300-500レベル：3レベル毎に1を取る / Level 300-500: take 1 for every 3 levels
            return 200 + (originalLevel - 300) / 3;
        }
        else
        {
            // 500级以上：每30级取1 / 500レベル以上：30レベル毎に1を取る / Level 500 and above: take 1 for every 30 levels
            return 266 + (originalLevel - 500) / 30;
        }
    }

    // 稀有度权重配置 / レアリティ重み設定 / Rarity weight configuration
    protected virtual Dictionary<Rarity, int> RarityWeights => new Dictionary<Rarity, int>
    {
        { Rarity.Normal, 25 },
        { Rarity.Superior, 25 },
        { Rarity.Legendary, 25 },
        { Rarity.Mythical, 25 }
    };

    // 设置稀有度 / レアリティを設定 / Set rarity
    private Rarity SetRarity()
    {
        var weights = RarityWeights;

        // 计算总权重 / 総重みを計算 / Calculate total weight
        int totalWeight = weights.Values.Sum();
        if (totalWeight <= 0)
            return Rarity.Normal;

        // 随机选择 / ランダム選択 / Random selection
        int randomValue = EClass.rnd(totalWeight);
        int currentWeight = 0;

        foreach (var kvp in weights)
        {
            currentWeight += kvp.Value;
            if (randomValue < currentWeight)
            {
                return kvp.Key;
            }
        }

        return Rarity.Normal;
    }

    // 获取稀有度加成 / レアリティボーナスを取得 / Get rarity bonus
    private int GetRarityBonus(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Normal => 0,
            Rarity.Superior => 1,
            Rarity.Legendary => 2,
            Rarity.Mythical => 3,
            Rarity.Artifact => 4,
            _ => 0 // 其他稀有度默认0 / その他レアリティはデフォルト0 / Other rarities default to 0
        };
    }

    // 从数组中随机选择不重复的元素 / 配列から重複なしでランダム要素を選択 / Select random unique elements from array
    private List<int> GetRandomUniqueElements(int[] sourceArray, int count)
    {
        if (sourceArray == null || sourceArray.Length == 0 || count <= 0)
            return new List<int>();

        // 打乱数组并取前count个元素 / 配列をシャッフルして前count個を取得 / Shuffle array and take first count elements
        var shuffled = sourceArray.OrderBy(x => EClass.rnd(1000)).ToArray();
        return shuffled.Take(Math.Min(count, sourceArray.Length)).ToList();
    }

    // 分组随机选择，增加多样性 / グループ分けランダム選択で多様性を増加 / Grouped random selection for increased diversity
    private List<int> GetDiverseRandomElements(int[] sourceArray, int count, int groupSize = 5)
    {
        if (sourceArray.Length <= groupSize)
            return GetRandomUniqueElements(sourceArray, count);

        // 将数组分成几个组，从不同组中随机选择 / 配列をグループ分けし、異なるグループからランダム選択 / Divide array into groups, randomly select from different groups
        var groups = new List<List<int>>();
        for (int i = 0; i < sourceArray.Length; i += groupSize)
        {
            var group = sourceArray.Skip(i).Take(groupSize).ToList();
            if (group.Count > 0) groups.Add(group);
        }

        var result = new List<int>();
        while (result.Count < count && groups.Count > 0)
        {
            int groupIndex = EClass.rnd(groups.Count);
            var group = groups[groupIndex];
            if (group.Count > 0)
            {
                int elementIndex = EClass.rnd(group.Count);
                result.Add(group[elementIndex]);
                group.RemoveAt(elementIndex);
            }

            if (group.Count == 0) groups.RemoveAt(groupIndex);
        }

        return result;
    }

    // 生成随机值 / ランダム値を生成 / Generate random value
    private int GenerateRandomValue(int baseValue, float randomFactor = 0.3f)
    {
        int randomRange = Math.Max(1, (int)(baseValue * randomFactor));
        int minValue = Math.Max(1, baseValue - randomRange);
        int maxValue = baseValue + randomRange;
        return minValue + EClass.rnd(maxValue - minValue + 1);
    }

    // 修改装备属性 / 装備属性を修正 / Modify equipment attributes
    private void ModEq(Thing item, Rarity rarity, int effectiveLv)
    {
        int rarityBonus = GetRarityBonus(rarity);

        // 重新设计各类属性的等级加成 / 各種属性のレベルボーナスを再設計 / Redesign level bonuses for various attributes
        int levelBonusCommon = Math.Max(1, effectiveLv / 20);  // 通用属性：每20级+1 / 共通属性：20レベル毎+1 / Common attributes: +1 every 20 levels
        int levelBonusResist = Math.Max(1, effectiveLv / 6);   // 抗性属性：每6级+1 / 抵抗属性：6レベル毎+1 / Resistance attributes: +1 every 6 levels
        int levelBonusMain = Math.Max(1, effectiveLv / 12);    // 主要属性：每12级+1 / 主要属性：12レベル毎+1 / Main attributes: +1 every 12 levels
        int levelBonusOther = Math.Max(1, effectiveLv / 12);   // 其他属性：每12级+1 / その他属性：12レベル毎+1 / Other attributes: +1 every 12 levels

        if (item.IsWeapon)
        {
            // 武器基础属性 - 大幅削弱 / 武器基础属性 - 大幅削弱 / Weapon base attributes - significantly weakened
            foreach (int ele in Effect_Weapon)
            {
                int baseValue = 1 +
                               (rarityBonus * 1) +  // 武器稀有度加成只有1倍 / 武器レアリティボーナスは1倍のみ / Weapon rarity bonus only 1x
                               (levelBonusMain * 2 / 3); // 武器等级加成只有2/3 / 武器レベルボーナスは2/3のみ / Weapon level bonus only 2/3
                baseValue = Math.Max(1, baseValue);
                item.elements.ModBase(ele, baseValue);
            }
        }
        else
        {
            // 护甲基础属性 - 保持原有强度 / 防具基础属性 - 元の強度を維持 / Armor base attributes - maintain original strength
            foreach (int ele in Effect_Armor)
            {
                int baseValue = 1 +
                               (rarityBonus * 2) +  // 护甲稀有度加成2倍 / 防具レアリティボーナスは2倍 / Armor rarity bonus 2x
                               levelBonusMain;      // 护甲全额等级加成 / 防具全额レベルボーナス / Armor full level bonus
                baseValue = Math.Max(1, baseValue);
                item.elements.ModBase(ele, baseValue);
            }
        }

        // 生成额外属性 / 追加属性を生成 / Generate additional attributes
        int[] subArray = item.IsWeapon ? Effect_Weapon_sub : Effect_Armor_sub;
        int subCount = EClass.rnd(2) + 1 + rarityBonus;
        subCount = Math.Min(subCount, subArray.Length);

        var selectedSubEffects = GetDiverseRandomElements(subArray, subCount, 6);
        foreach (int ele in selectedSubEffects)
        {
            int basePower = (EClass.rndSqrt(3) + 1) * (1 + rarityBonus);
            int baseValue = basePower * 2 + levelBonusOther;
            int value = GenerateRandomValue(baseValue, 0.4f); // ±40%随机范围 / ±40%ランダム範囲 / ±40% random range
            item.elements.ModBase(ele, Math.Max(1, value));
        }

        // 生成战斗属性 / 战斗属性を生成 / Generate battle attributes
        int battleChance = 30 + (rarityBonus * 10);
        if (EClass.rnd(100) < battleChance)
        {
            int battleCount = EClass.rnd(2) + 1 + rarityBonus;
            battleCount = Math.Min(battleCount, Effect_Battle.Length);

            var selectedBattleEffects = GetRandomUniqueElements(Effect_Battle, battleCount);
            foreach (int ele in selectedBattleEffects)
            {
                int baseValue = (3 + rarityBonus * 2) + levelBonusOther;
                int value = GenerateRandomValue(baseValue, 0.4f); // ±40%随机范围 / ±40%ランダム範囲 / ±40% random range
                item.elements.ModBase(ele, Math.Max(1, value));
            }
        }

        // 护甲生成通用属性 / 防具で共通属性を生成 / Generate common attributes for armor
        if (!item.IsWeapon)
        {
            int commonChance = 30 + (rarityBonus * 10);
            if (EClass.rnd(100) < commonChance)
            {
                int commonCount = EClass.rnd(2) + 1 + rarityBonus;
                commonCount = Math.Min(commonCount, Effect_Common.Length);

                var selectedCommonEffects = GetDiverseRandomElements(Effect_Common, commonCount, 8);
                foreach (int ele in selectedCommonEffects)
                {
                    int baseValue = 5 + EClass.rnd(3 + rarityBonus) + levelBonusCommon;
                    int value = GenerateRandomValue(baseValue, 0.3f); // ±30%随机范围 / ±30%ランダム範囲 / ±30% random range
                    item.elements.ModBase(ele, Math.Max(1, value));
                }
            }
        }

        // 生成稀有度属性 / レアリティ属性を生成 / Generate rarity attributes
        int rarityChance = 5 + (rarityBonus * 5);
        if (EClass.rnd(100) < rarityChance)
        {
            int ele = Effect_Rarity[EClass.rnd(Effect_Rarity.Length)];
            int baseValue = 8 + (rarityBonus * 3) + levelBonusOther;
            int value = GenerateRandomValue(baseValue, 0.35f); // ±35%随机范围 / ±35%ランダム範囲 / ±35% random range
            item.elements.ModBase(ele, Math.Max(1, value));
        }

        // 护甲生成抵消属性 / 防具で无効化属性を生成 / Generate negate attributes for armor
        if (!item.IsWeapon)
        {
            int negateChance = 50 + (rarityBonus * 15);
            if (EClass.rnd(100) < negateChance)
            {
                int negateCount = EClass.rnd(2) + 1 + rarityBonus;
                negateCount = Math.Min(negateCount, Effect_Negate.Length);

                var selectedNegateEffects = GetRandomUniqueElements(Effect_Negate, negateCount);
                foreach (int ele in selectedNegateEffects)
                {
                    int baseValue = 8 + rarityBonus * 2 + levelBonusResist;
                    int value = GenerateRandomValue(baseValue, 0.35f); // ±35%随机范围 / ±35%ランダム範囲 / ±35% random range
                    item.elements.ModBase(ele, Math.Max(1, value));
                }
            }
        }

        // 武器生成元素属性 / 武器で元素属性を生成 / Generate element attributes for weapons
        if (item.IsWeapon)
        {
            int eleChance = 5 + (rarityBonus * 15);
            if (EClass.rnd(100) < eleChance)
            {
                int eleCount = EClass.rnd(2);
                eleCount = Math.Min(eleCount, Effect_Ele.Length);

                var selectedEleEffects = GetRandomUniqueElements(Effect_Ele, eleCount);
                foreach (int ele in selectedEleEffects)
                {
                    int baseValue = 10 + (rarityBonus * 3) + levelBonusOther;
                    int value = GenerateRandomValue(baseValue, 0.4f); // ±40%随机范围 / ±40%ランダム範囲 / ±40% random range
                    item.elements.ModBase(ele, Math.Max(1, value));
                }
            }
        }
        
        // 添加证明效果 / 证明效果を追加 / Add proof effects
        foreach (int ele in Effect_Proof)
        {
            item.elements.ModBase(ele, 1);
        }
    }

    // 生成装备 / 装備を生成 / Spawn equipment
    private void SpawnEq(int lv)
    {
        Rarity rarity = SetRarity();
        CardBlueprint.Set(new CardBlueprint
        {
            rarity = rarity
        });
        Thing thing = ThingGen.CreateFromFilter("eq", lv);
        ModEq(thing, rarity, lv);
        Point point = owner.GetRootCard().pos;
        if (point.IsBlocked)
        {
            point = point.GetNearestPoint();
        }
        thing.isHidden = false;
        thing.SetInt(116);
        thing.c_IDTState = 0;
        EClass._zone.AddCard(thing, point);
    }
}

// 木制宝箱特质 / 木製宝箱特性 / Wooden Chest Trait
public class TraitGBFChestwooden : TraitGBFChestBase
{
    protected override Dictionary<Rarity, int> RarityWeights => new Dictionary<Rarity, int>
    {
        { Rarity.Normal, 80 },  // 普通80% / ノーマル80% / Normal 80%
        { Rarity.Superior, 20 } // 优秀20% / スペリオ20% / Superior 20%
    };
}

// 银制宝箱特质 / 銀製宝箱特性 / Silver Chest Trait
public class TraitGBFChestsliver : TraitGBFChestBase
{
    protected override Dictionary<Rarity, int> RarityWeights => new Dictionary<Rarity, int>
    {
        { Rarity.Normal, 25 },     // 普通25% / ノーマル25% / Normal 25%
        { Rarity.Superior, 60 },   // 优秀60% / スペリオ60% / Superior 60%
        { Rarity.Legendary, 15 }   // 传奇15% / レジェンダリ15% / Legendary 15%
    };
}

// 金制宝箱特质 / 金製宝箱特性 / Gold Chest Trait
public class TraitGBFChestgold : TraitGBFChestBase
{
    protected override Dictionary<Rarity, int> RarityWeights => new Dictionary<Rarity, int>
    {
        { Rarity.Superior, 45 },   // 优秀45% / スペリオ45% / Superior 45%
        { Rarity.Legendary, 30 },  // 传奇30% / レジェンダリ30% / Legendary 30%
        { Rarity.Mythical, 25 }    // 神话25% / ミスティカ25% / Mythical 25%
    };
}

// 红色宝箱特质 / 赤色宝箱特性 / Red Chest Trait
public class TraitGBFChestred : TraitGBFChestBase
{
    protected override Dictionary<Rarity, int> RarityWeights => new Dictionary<Rarity, int>
    {
        { Rarity.Legendary, 50 },  // 传奇50% / レジェンダリ50% / Legendary 50%
        { Rarity.Mythical, 50 }    // 神话50% / ミスティカ50% / Mythical 50%
    };
}

// 蓝色宝箱特质 / 青色宝箱特性 / Blue Chest Trait
public class TraitGBFChestblue : TraitGBFChestBase
{
    protected override Dictionary<Rarity, int> RarityWeights => new Dictionary<Rarity, int>
    {
        { Rarity.Legendary, 65 },  // 传奇65% / レジェンダリ65% / Legendary 65%
        { Rarity.Mythical, 35 }    // 神话35% / ミスティカ35% / Mythical 35%
    };
}
}
