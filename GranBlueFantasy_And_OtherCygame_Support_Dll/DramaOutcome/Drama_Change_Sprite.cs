using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Cwl.Helper.Unity;
using UnityEngine;
using Cwl.Helper.Extensions;
using Cwl.LangMod;

namespace GBF.dramaOutcome.Drama_Change_Sprite
{
    internal class Tiger_Drama_Change_Sprite : DramaOutcome
    {
        private static readonly int _mainTex = Shader.PropertyToID("_MainTex");  // 主纹理属性ID / Main texture property ID / メインテクスチャプロパティID

        // 切换Cidala角色精灵图到样式2 / Switch Cidala character sprite to style 2 / Cidalaキャラクタースプライトをスタイル2に切り替え
        public static bool Tiger_change_sprite2(DramaManager dm, Dictionary<string, string> line, params string[] parameters)
        {
            // 查找ID为"Cidala"的角色 / Find character with ID "Cidala" / IDが"Cidala"のキャラクターを検索
            if (_map.charas.FirstOrDefault(c => c.id == "Cidala") is not { } cidala)
            {
                return false;
            }

            // 加载Cidala2精灵图 / Load Cidala2 sprite / Cidala2スプライトをロード
            var dameSprite = SpriteReplacer.dictModItems["Cidala2"].LoadSprite();
            if (dameSprite == null || cidala.renderer?.actor?.sr == null)
            {
                return false;
            }

            // 更新角色精灵图和纹理 / Update character sprite and texture / キャラクタースプライトとテクスチャを更新
            var actor = cidala.renderer.actor;
            actor.sr.sprite = dameSprite;
            actor.mpb.SetTexture(_mainTex, dameSprite.texture);

            // 更新角色肖像ID / Update character portrait ID / キャラクターポートレートIDを更新
            cidala.c_idPortrait = "Cidala_Style";

            return true;
        }

        // 切换Cidala角色精灵图到默认样式 / Switch Cidala character sprite to default style / Cidalaキャラクタースプライトをデフォルトスタイルに切り替え
        public static bool Tiger_change_sprite(DramaManager dm, Dictionary<string, string> line, params string[] parameters)
        {
            // 查找ID为"Cidala"的角色 / Find character with ID "Cidala" / IDが"Cidala"のキャラクターを検索
            if (_map.charas.FirstOrDefault(c => c.id == "Cidala") is not { } cidala)
            {
                return false;
            }

            // 加载Cidala默认精灵图 / Load Cidala default sprite / Cidalaデフォルトスプライトをロード
            var dameSprite = SpriteReplacer.dictModItems["Cidala"].LoadSprite();
            if (dameSprite == null || cidala.renderer?.actor?.sr == null)
            {
                return false;
            }

            // 更新角色精灵图和纹理 / Update character sprite and texture / キャラクタースプライトとテクスチャを更新
            var actor = cidala.renderer.actor;
            actor.sr.sprite = dameSprite;
            actor.mpb.SetTexture(_mainTex, dameSprite.texture);

            // 更新角色肖像ID / Update character portrait ID / キャラクターポートレートIDを更新
            cidala.c_idPortrait = "Cidala";

            return true;
        }
    }
}
