using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cwl.API.Attributes;
using Cwl.API.Custom;
using Cwl.Helper.Unity;
using GBF.ModuleFunction.CharaPortrait;
using HarmonyLib;
using UnityEngine;
using System.IO;

namespace GBF.ModuleFunction.Switch_Spirte_ContextMenu_Button
{
    /// 这个模块通过使用CWL的菜单按钮功能来实现同时对游戏中的目标进行——Sprite图，肖像，行为，专长，等进行修改，通常我把这个功能叫做换皮肤，但是由于Noa对于Spirte图刷新的问题，会导致实际上运行起来会因为玩家将对应角色甩出地图或者是切换地图时失效。
    /// This module uses CWL's menu button functionality to simultaneously modify in-game targets - including Sprite images, portraits, behaviors, specialties, etc. I usually call this feature "skin changing". However, due to Noa's Sprite image refresh issues, it may actually fail when players throw the corresponding character off the map or switch maps.
    /// このモジュールはCWLのメニューボタン機能を使用して、ゲーム内のターゲットに対してスプライト画像、ポートレート、行動、特技などを同時に変更します。通常、この機能を「スキンチェンジ」と呼んでいますが、Noaのスプライト画像リフレッシュの問題により、プレイヤーが対応するキャラクターをマップ外に投げ出したり、マップを切り替えたりすると実際には機能しなくなる可能性があります。

    public static class GBFCharacterAppearanceMenu
    {
        private static readonly int _mainTex = Shader.PropertyToID("_MainTex"); // 主纹理属性ID / Main texture property ID / メインテクスチャプロパティID

        [CwlContextMenu("gbf_ui_cidala_combine")]
        private static void SwitchToCidala2()
        {
            ChangeCharacterSprite("Cidala", "Cidala2", "Cidala_Style");
        }

        [CwlContextMenu("gbf_ui_cidala_twin")]
        private static void SwitchToCidala1()
        {
            ChangeCharacterSprite("Cidala", "Cidala", "Cidala");
        }

        [CwlContextMenu("gbf_ui_Vajra_normal")]
        private static void SwitchToVajra1()
        {

            ChangeCharacterSprite("Vajra", "Vajra", "Vajra");
            AddCharacterAction("Vajra", 170032, 100, false);
            AddCharacterAction("Vajra", 170033, 100, false);
            AddCharacterAction("Vajra", 170034, 100, false);
            AddCharacterAction("Vajra", 170040, 100, false);
            RemoveCharacterAction("Vajra", 170035);
            RemoveCharacterAction("Vajra", 170036);
            RemoveCharacterAction("Vajra", 170037);
            RemoveCharacterAction("Vajra", 170041);
            RemoveCharacterAction("Vajra", 170042);
            RemoveCharacterAction("Vajra", 170043);
            AddCharacterFeat("Vajra", 170031);
            RemoveCharacterFeat("Vajra", 170038);
            RemoveCharacterFeat("Vajra", 170044);

        }

        [CwlContextMenu("gbf_ui_Vajra_sea")]
        private static void SwitchToVajra2()
        {

            ChangeCharacterSprite("Vajra", "Vajra2", "Vajra2");
            AddCharacterAction("Vajra", 170041, 100, false);
            AddCharacterAction("Vajra", 170042, 100, false);
            AddCharacterAction("Vajra", 170043, 100, false);
            RemoveCharacterAction("Vajra", 170032);
            RemoveCharacterAction("Vajra", 170033);
            RemoveCharacterAction("Vajra", 170034);
            RemoveCharacterAction("Vajra", 170035);
            RemoveCharacterAction("Vajra", 170036);
            RemoveCharacterAction("Vajra", 170037);
            RemoveCharacterAction("Vajra", 170040);
            AddCharacterFeat("Vajra", 170044);
            RemoveCharacterFeat("Vajra", 170031);
            RemoveCharacterFeat("Vajra", 170038);
        }

        [CwlContextMenu("gbf_ui_Vajra_spring")]
        private static void SwitchToVajra3()
        {

            ChangeCharacterSprite("Vajra", "Vajra3", "Vajra3");

        }
        [CwlContextMenu("gbf_ui_Vajra_newyear")]
        private static void SwitchToVajra4()
        {

            ChangeCharacterSprite("Vajra", "Vajra4", "Vajra4");

        }
        [CwlContextMenu("gbf_ui_Vajra_halloween")]
        private static void SwitchToVajra5()
        {

            ChangeCharacterSprite("Vajra", "Vajra5", "Vajra5");
            AddCharacterAction("Vajra", 170035, 100, false);
            AddCharacterAction("Vajra", 170036, 100, false);
            AddCharacterAction("Vajra", 170037, 100, false);
            RemoveCharacterAction("Vajra", 170032);
            RemoveCharacterAction("Vajra", 170033);
            RemoveCharacterAction("Vajra", 170034);
            RemoveCharacterAction("Vajra", 170040);
            RemoveCharacterAction("Vajra", 170041);
            RemoveCharacterAction("Vajra", 170042);
            RemoveCharacterAction("Vajra", 170043);
            AddCharacterFeat("Vajra", 170038);
            RemoveCharacterFeat("Vajra", 170031);
            RemoveCharacterFeat("Vajra", 170044);
        }

        private static bool ChangeCharacterSprite(string charaId, string spriteKey, string portraitId)
        {
            // 获取当前地图实例 / Get current map instance / 現在のマップインスタンスを取得
            var map = EClass._map;
            if (map == null)
            {
                return false; // 地图不存在返回失败 / Map doesn't exist, return failure / マップが存在しない場合失敗を返す
            }

            // 根据角色ID查找角色 / Find character by character ID / キャラIDでキャラクターを検索
            if (map.charas.FirstOrDefault(c => c.id == charaId) is not { } chara)
            {
                UnityEngine.Debug.LogWarning($"未找到角色: {charaId}"); // 角色未找到警告 / Character not found warning / キャラクター未検出警告
                return false;
            }

            // 从Sprite替换器加载新精灵图 / Load new sprite from Sprite replacer / スプライト置換器から新しいスプライトをロード
            var newSprite = SpriteReplacer.dictModItems[spriteKey].LoadSprite();
            if (newSprite == null)
            {
                UnityEngine.Debug.LogWarning($"未找到Sprite: {spriteKey}"); // Sprite未找到警告 / Sprite not found warning / スプライト未検出警告
                return false;
            }

            // 检查角色渲染器是否有效 / Check if character renderer is valid / キャラクターレンダラーが有効か確認
            if (chara.renderer?.actor?.sr == null)
            {
                UnityEngine.Debug.LogWarning($"角色 {charaId} 的渲染器为空"); // 渲染器为空警告 / Renderer is null warning / レンダラーがnullの警告
                return false;
            }

            // 更新角色精灵图和材质 / Update character sprite and material / キャラクタースプライトとマテリアルを更新
            var actor = chara.renderer.actor;
            actor.sr.sprite = newSprite;                       // 设置新精灵图 / Set new sprite / 新しいスプライトを設定
            actor.mpb.SetTexture(_mainTex, newSprite.texture); // 更新主纹理 / Update main texture / メインテクスチャを更新

            // 更新角色肖像ID / Update character portrait ID / キャラクターポートレートIDを更新
            chara.c_idPortrait = portraitId;

            UnityEngine.Debug.Log($"已切换角色 {charaId} 的皮肤为: {spriteKey}"); // 切换成功日志 / Switch success log / 切り替え成功ログ
            return true;                                                 // 返回成功 / Return success / 成功を返す
        }


        private static void AddCharacterAction(string charaId, int actId, int chance, bool isParty)
        {
            try
            {
                var map = EClass._map;
                if (map == null) return;

                // 查找指定角色 / Find specified character / 指定キャラクターを検索
                if (map.charas.FirstOrDefault(c => c.id == charaId) is not { } chara)
                {
                    UnityEngine.Debug.LogWarning($"添加Action时未找到角色: {charaId}"); // 角色未找到警告 / Character not found warning / キャラクター未検出警告
                    return;
                }

                // 检查是否已拥有该Action / Check if already has this action / 既にこのアクションを持っているか確認
                bool alreadyHasAction = chara.ability.list.items.Any(item => item.act.id == actId);

                if (!alreadyHasAction)
                {
                    // 添加Action到角色能力列表 / Add action to character ability list / キャラクター能力リストにアクションを追加
                    chara.ability.Add(actId, chance, isParty);
                    chara.ability.Refresh(); // 刷新能力列表 / Refresh ability list / 能力リストをリフレッシュ

                    UnityEngine.Debug.Log($"已为角色 {charaId} 添加Action: ID {actId}, 权重 {chance}, 队伍共享 {isParty}"); // 添加成功日志 / Add success log / 追加成功ログ
                }
                else
                {
                    UnityEngine.Debug.Log($"角色 {charaId} 已拥有Action: ID {actId}"); // 已拥有Action日志 / Already has action log / 既にアクション所有済みログ
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"为角色 {charaId} 添加Action失败 (ID: {actId}): {ex.Message}"); // 添加失败警告 / Add failure warning / 追加失敗警告
            }
        }

        private static void RemoveCharacterAction(string charaId, int actId)
        {
            try
            {
                var map = EClass._map;
                if (map == null) return;

                // 查找指定角色 / Find specified character / 指定キャラクターを検索
                if (map.charas.FirstOrDefault(c => c.id == charaId) is not { } chara)
                {
                    UnityEngine.Debug.LogWarning($"移除Action时未找到角色: {charaId}"); // 角色未找到警告 / Character not found warning / キャラクター未検出警告
                    return;
                }

                // 查找并移除Action / Find and remove action / アクションを検索して削除
                var actionToRemove = chara.ability.list.items.FirstOrDefault(item => item.act.id == actId);
                if (actionToRemove != null)
                {
                    chara.ability.Remove(actId);                                   // 移除Action / Remove action / アクションを削除
                    chara.ability.Refresh();                                       // 刷新能力列表 / Refresh ability list / 能力リストをリフレッシュ
                    UnityEngine.Debug.Log($"已移除角色 {charaId} 的Action: ID {actId}"); // 移除成功日志 / Remove success log / 削除成功ログ
                }
                else
                {
                    UnityEngine.Debug.Log($"角色 {charaId} 未拥有Action: ID {actId}"); // 未拥有Action日志 / No action found log / アクション未所有ログ
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"移除角色 {charaId} 的Action失败 (ID: {actId}): {ex.Message}"); // 移除失败警告 / Remove failure warning / 削除失敗警告
            }
        }

// 通用的添加法术方法 / Generic method to add spell / 汎用的な呪文追加メソッド
        private static void AddCharacterSpell(string charaId, int elementId, int elementLv, int potential)
        {
            try
            {
                var map = EClass._map;
                if (map == null) return;

                // 查找指定角色 / Find specified character / 指定キャラクターを検索
                if (map.charas.FirstOrDefault(c => c.id == charaId) is not { } chara)
                {
                    return;
                }

                // 添加或更新法术 / Add or update spell / 呪文を追加または更新
                if (chara.elements.GetOrCreateElement(elementId).ValueWithoutLink == 0)
                {
                    chara.elements.ModBase(elementId, elementLv); // 修改基础等级 / Modify base level / 基本レベルを修正
                }
                chara.elements.SetBase(elementId, elementLv, potential); // 设置基础值和潜力 / Set base value and potential / 基本値と潜在力を設定

                UnityEngine.Debug.Log($"已为角色 {charaId} 添加法术: 元素ID {elementId}, 等级 {elementLv}"); // 添加法术成功日志 / Add spell success log / 呪文追加成功ログ
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"为角色 {charaId} 添加法术失败 (元素ID: {elementId}): {ex.Message}"); // 添加法术失败警告 / Add spell failure warning / 呪文追加失敗警告
            }
        }

// 通用的移除法术方法 / Generic method to remove spell / 汎用的な呪文削除メソッド
        private static void RemoveCharacterSpell(string charaId, int elementId)
        {
            try
            {
                var map = EClass._map;
                if (map == null) return;

                // 查找指定角色 / Find specified character / 指定キャラクターを検索
                if (map.charas.FirstOrDefault(c => c.id == charaId) is not { } chara)
                {
                    return;
                }

                // 检查并移除法术 / Check and remove spell / 呪文をチェックして削除
                if (chara.elements.dict.ContainsKey(elementId))
                {
                    chara.elements.Remove(elementId);                                // 移除元素 / Remove element / エレメントを削除
                    UnityEngine.Debug.Log($"已移除角色 {charaId} 的法术: 元素ID {elementId}"); // 移除法术成功日志 / Remove spell success log / 呪文削除成功ログ
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"移除角色 {charaId} 的法术失败 (元素ID: {elementId}): {ex.Message}"); // 移除法术失败警告 / Remove spell failure warning / 呪文削除失敗警告
            }
        }

        private static void AddCharacterFeat(string charaId, int featId, int baseLv = 1, int potential = 100)
        {
            try
            {
                var map = EClass._map;
                if (map == null) return;

                // 查找指定角色 / Find specified character / 指定キャラクターを検索
                if (map.charas.FirstOrDefault(c => c.id == charaId) is not { } chara)
                {
                    UnityEngine.Debug.LogWarning($"添加Feat时未找到角色: {charaId}"); // 角色未找到警告 / Character not found warning / キャラクター未検出警告
                    return;
                }

                // 检查是否已有该Feat / Check if already has this feat / 既にこの特技を持っているか確認
                var featElement = chara.elements.GetOrCreateElement(featId);
                if (featElement.ValueWithoutLink > 0)
                {
                    UnityEngine.Debug.Log($"角色 {charaId} 已拥有Feat: ID {featId}"); // 已拥有Feat日志 / Already has feat log / 既に特技所有済みログ
                    return;
                }

                // 如果没有则添加 / If not, then add / なければ追加
                chara.elements.ModBase(featId, 1);                                                         // 修改基础值 / Modify base value / 基本値を修正
                chara.elements.SetBase(featId, baseLv, potential);                                         // 设置基础等级和潜力 / Set base level and potential / 基本レベルと潜在力を設定
                UnityEngine.Debug.Log($"已为角色 {charaId} 添加Feat: ID {featId}, 等级 {baseLv}, 潜在 {potential}"); // 添加Feat成功日志 / Add feat success log / 特技追加成功ログ
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"为角色 {charaId} 添加Feat失败 (ID: {featId}): {ex.Message}"); // 添加Feat失败警告 / Add feat failure warning / 特技追加失敗警告
            }
        }

        private static void RemoveCharacterFeat(string charaId, int featId)
        {
            try
            {
                var map = EClass._map;
                if (map == null) return;

                // 查找指定角色 / Find specified character / 指定キャラクターを検索
                if (map.charas.FirstOrDefault(c => c.id == charaId) is not { } chara)
                {
                    UnityEngine.Debug.LogWarning($"移除Feat时未找到角色: {charaId}"); // 角色未找到警告 / Character not found warning / キャラクター未検出警告
                    return;
                }

                // 检查是否存在该Feat / Check if this feat exists / この特技が存在するか確認
                var featElement = chara.elements.GetOrCreateElement(featId);
                if (featElement.ValueWithoutLink == 0)
                {
                    UnityEngine.Debug.Log($"角色 {charaId} 未拥有Feat: ID {featId}"); // 未拥有Feat日志 / No feat found log / 特技未所有ログ
                    return;
                }

                // 执行移除 / Execute removal / 削除を実行
                chara.elements.Remove(featId);                                // 移除Feat / Remove feat / 特技を削除
                UnityEngine.Debug.Log($"已移除角色 {charaId} 的Feat: ID {featId}"); // 移除Feat成功日志 / Remove feat success log / 特技削除成功ログ
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"移除角色 {charaId} 的Feat失败 (ID: {featId}): {ex.Message}"); // 移除Feat失败警告 / Remove feat failure warning / 特技削除失敗警告
            }
        }



    }
}

