using System.Collections.Generic;
using Cwl.API.Attributes;
using Cwl.Helper.Extensions;

namespace GBF.ModuleFunction.CharaPortrait
{
    /// 这个模块的作用是不使用Noa官方规定的UN_XXX类型肖像而无法通过代码更换肖像的情况，这通常和切换Sprite图一起使用。
    /// The purpose of this module is to address the situation where portrait changes cannot be made through code without using the officially designated UN_XXX type portraits by Noa. This is typically used in conjunction with switching Sprite images.
    /// このモジュールの目的は、Noaが公式に定めたUN_XXXタイプのポートレートを使用せずにコードを通じてポートレートを変更できない状況に対処するためです。これは通常、スプライト画像の切り替えと一緒に使用されます。

    public class GBF_CharaPortrait
    {
        // 特殊肖像角色ID集合 / Special portrait character ID collection / 特殊ポートレートキャラIDコレクション
        private static readonly HashSet<string> SpecialPortraitIds = new()
        {
            "Cidala",              // 辛妲拉角色 / Cidala character / シンダラキャラクター
            "Indala",              // 茵妲拉角色 / Indala character / インダラキャラクター
            "NarmayaWind",         // 娜尔梅亚角色 / Narmaya character / ナルメアキャラクター
            "Cagliostro_Clarisse", // 卡里奥斯特罗与克拉莉丝角色 / Cagliostro and Clarisse character / カリオストロ＆クラリスキャラクター
            "PCRCal",              // 凯露角色 / Cal character / キャルキャラクター
            "Berceau",             // 贝尔索角色 / Berceau character / ベルソーキャラクター
            "Vajra"                // 瓦姬拉角色 / Vajra character / ヴァジラキャラクター
        };

        // 角色实例化事件处理 / Character instantiation event handling / キャラクターインスタンス化イベント処理
        [CwlCharaOnCreateEvent]
        internal static void OnCharaInstantiation(Chara __instance)
        {
            // 检查角色ID是否在特殊肖像列表中 / Check if character ID is in special portrait list / キャラIDが特殊ポートレートリストにあるか確認
            if (SpecialPortraitIds.Contains(__instance.id))
            {
                // 设置角色使用自身ID作为肖像ID / Set character to use own ID as portrait ID / キャラが自身のIDをポートレートIDとして使用するように設定
                __instance.c_idPortrait = __instance.id;
            }
        }
    }
}
