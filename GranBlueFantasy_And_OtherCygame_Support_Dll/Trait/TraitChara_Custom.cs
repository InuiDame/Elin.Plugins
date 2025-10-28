namespace GBF.trait.TraitChara_Custom
{
    // 卡里奥斯特罗角色特质 / カリオストロキャラ特性 / Cagliostro Character Trait
public class TraitCagliostroChara : TraitChara
{
    // 唯一角色 / ユニークキャラクター / Unique character
    public override bool IsUnique => true;

    // 库存样式ID / インベントリスタイルID / Inventory style ID
    public override string IDInvStyle => "bookshelf";

    // 商店重置费用为0 / ショップリロールコスト0 / Shop reroll cost 0
    public override int CostRerollShop => 0;

    // 复制商店类型 / コピーショップタイプ / Copy shop type
    public override CopyShopType CopyShop => CopyShopType.Item; // 物品复制商店 / アイテムコピーショップ / Item copy shop

    // 商店类型 / ショップタイプ / Shop type
    public override ShopType ShopType => ShopType.Copy; // 复制商店 / コピーショップ / Copy shop

    // 价格类型 / 価格タイプ / Price type
    public override PriceType PriceType => PriceType.CopyShop;  // 复制商店价格 / コピーショップ価格 / Copy shop price

    // 不能被放逐 / 追放不可 / Cannot be banished
    public override bool CanBeBanished => false;

    // 补货天数 / 再補充日数 / Restock days
    public override int RestockDay => 14;  // 14天补货 / 14日ごとに補充 / Restock every 14 days

    // 检查物品是否可以复制 / アイテムがコピー可能かチェック / Check if item can be copied
    public override bool CanCopy(Thing t)
    {
        // 不可出售、被盗、有符文、或有1229元素则不能复制 / 販売不可、盗品、ルーン所持、1229要素ありはコピー不可 / No sell, stolen, has rune, or has element 1229 cannot copy
        if (t.noSell || t.isStolen || t.HasRune() || t.HasElement(1229))
        {
            return false;
        }
        
        // 种子特质可以复制 / シード特性はコピー可能 / Seed traits can be copied
        if (t.trait is TraitSeed)
        {
            return true;
        }
        
        // 鱼片食物不能复制 / 魚の切り身フードはコピー不可 / Fish slice food cannot copy
        if (t.trait is TraitFoodFishSlice)
        {
            return false;
        }
        
        // 有不可复制元素则不能复制 / コピー不可要素ありはコピー不可 / Has no-copy elements cannot copy
        if (t.HasElementNoCopy())
        {
            return false;
        }
        
        // 检查插槽 / ソケットをチェック / Check sockets
        if (t.sockets != null)
        {
            foreach (int socket in t.sockets)
            {
                // 有已使用插槽则不能复制 / 使用済みソケットありはコピー不可 / Has used sockets cannot copy
                if (socket != 0)
                {
                    return false;
                }
            }
        }
        
        // 必须是已制作的物品 / クラフト済みアイテムのみ / Must be crafted item
        return t.isCrafted;
    }
}

// 瓦姬拉角色特质 / ヴァジラュラキャラ特性 / Vajra Character Trait
public class TraitVajraChara : TraitChara
{
    // 唯一角色 / ユニークキャラクター / Unique character
    public override bool IsUnique => true;

    // 不能被放逐 / 追放不可 / Cannot be banished
    public override bool CanBeBanished => false;
}
}
