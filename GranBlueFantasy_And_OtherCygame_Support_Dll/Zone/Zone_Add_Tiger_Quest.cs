using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Cwl.Helper.Extensions;
using Cwl.LangMod;
using GBF.zone.Zone_Add_Tiger_Quest;
using UnityEngine;

namespace GBF.zone.Zone_Add_Tiger_Quest
{
    public class Zone_Beachcidala : Zone_Civilized
    {
        public override bool IsExplorable => false;        // 不可探索区域 / Unexplorable zone / 探索不可ゾーン
        public override bool CanDigUnderground => false;   // 不允许挖掘地下 / Cannot dig underground / 地下掘削不可
        public override bool AllowCriminal => false;       // 不允许罪犯进入 / Criminals not allowed / 犯罪者進入不可
        
        // 区域导出路径 / Zone export path / ゾーンエクスポートパス
        public override string pathExport => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
        $"Map/{idExport}.z");
    }

    public static class ZoneExt_OLD
    {
        // 在指定坐标生成区域 / Spawn zone at specified coordinates / 指定座標にゾーンを生成
        public static void SpawnZoneAt(Region region, string zoneFullName, int eloX, int eloY)
        {
            // 记录方法开始和参数 / Log method start and parameters / メソッド開始とパラメータを記録
            Debug.Log($"[ZoneExt.SpawnZoneAt] 开始生成区域: zoneFullName={zoneFullName}, eloX={eloX}, eloY={eloY}, region={region?.name ?? "null"}");

            // 检查区域是否已存在 / Check if zone already exists / ゾーンが既に存在するか確認
            if (region.children.Find(z => z.x == eloX && z.y == eloY) is { } existZone)
            {
                Debug.LogWarning($"[ZoneExt.SpawnZoneAt] 区域已存在，跳过生成: 坐标({eloX},{eloY}) 已存在区域 {existZone.name}");
                return;
            }
            else
            {
                Debug.Log($"[ZoneExt.SpawnZoneAt] 检查通过: 坐标({eloX},{eloY}) 没有重复区域");
            }

            // 验证区域名称 / Validate zone name / ゾーン名を検証
            if (!zoneFullName.ValidateZone(out var zone) || zone is null)
            {
                Debug.LogError($"[ZoneExt.SpawnZoneAt] 区域验证失败: zoneFullName={zoneFullName} 无效或为空");
                return;
            }
            else
            {
                Debug.Log($"[ZoneExt.SpawnZoneAt] 区域验证成功: {zoneFullName} -> {zone.name}");
            }

            try
            {
                // 设置区域坐标 / Set zone coordinates / ゾーン座標を設定
                zone.x = eloX;
                zone.y = eloY;
                Debug.Log($"[ZoneExt.SpawnZoneAt] 设置区域坐标: ({eloX},{eloY})");

                zone.parent?.RemoveChild(zone);  // 从原父节点移除 / Remove from original parent / 元の親ノードから削除

                // 设置到地图 / Set to map / マップに設定
                region.elomap.SetZone(eloX, eloY, zone, true);
                Debug.Log($"[ZoneExt.SpawnZoneAt] 成功设置到地图: 坐标({eloX},{eloY})");

                // 添加到区域子节点 / Add to region children / リージョンの子ノードに追加
                region.AddChild(zone);
                Debug.Log($"[ZoneExt.SpawnZoneAt] 成功添加到区域子节点: {zone.name}");

                Debug.Log($"[ZoneExt.SpawnZoneAt] 区域生成完成: {zoneFullName} 在坐标({eloX},{eloY})");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ZoneExt.SpawnZoneAt] 生成区域时发生异常: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        // 查找或创建指定等级的层级 / Find or create level with specified level / 指定レベルのレベルを検索または作成
        public static Zone? FindOrCreateLevel(Zone zone, int lv)
        {
            try
            {
                var newZone = zone.FindZone(lv);  // 查找现有层级 / Find existing level / 既存レベルを検索
                if (newZone is not null)
                {
                    return newZone;
                }

                // 创建新层级 / Create new level / 新しいレベルを作成
                newZone = (SpatialGen.Create(zone.GetNewZoneID(lv), zone, true) as Zone)!;
                newZone.lv = lv;      // 设置层级 / Set level / レベルを設定
                newZone.x = zone.x;   // 继承X坐标 / Inherit X coordinate / X座標を継承
                newZone.y = zone.y;   // 继承Y坐标 / Inherit Y coordinate / Y座標を継承

                return newZone;
            }
            catch (Exception ex)
            {
                // 静默处理异常，返回null / Silently handle exception, return null / 例外を静かに処理、nullを返す
                return null;
                // noexcept
            }
        }
    }
}
