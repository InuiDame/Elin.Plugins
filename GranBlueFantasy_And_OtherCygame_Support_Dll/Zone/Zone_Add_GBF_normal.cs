using System.IO;
using System.Reflection;
using UnityEngine;

namespace GBF.zone.Zone_Add_Tiger_Quest;

public class Zone_GBF_N_HG : Zone_Civilized
{
    public override string idExport => "GBF_Hidden_Graveyard";
    public override bool IsExplorable => false;        // 不可探索区域 / Unexplorable zone / 探索不可ゾーン
    public override bool CanDigUnderground => false;   // 不允许挖掘地下 / Cannot dig underground / 地下掘削不可
    public override bool AllowCriminal => true;       // 允许罪犯进入 / Criminals allowed / 犯罪者進入可
    public override bool RestrictBuild => true;  // 禁止玩家建造
        
    // 区域导出路径 / Zone export path / ゾーンエクスポートパス
    public override string pathExport
    {
        get
        {
            string dllDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            string fullPath = Path.Combine(dllDir, $"Map/{idExport}.z");
            Debug.Log($"[Beachcidala] pathExport = {fullPath}");  
            return fullPath;
        }
    }
}