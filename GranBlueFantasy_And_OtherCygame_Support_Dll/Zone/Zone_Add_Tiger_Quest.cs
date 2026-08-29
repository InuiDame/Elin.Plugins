using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GBF.zone.Zone_Add_Tiger_Quest;
using UnityEngine;

namespace GBF.zone.Zone_Add_Tiger_Quest
{
    public class Zone_Beachcidala : Zone_Civilized
    {
        public override string idExport => "beach_cidala";
        public override bool IsExplorable => false;        // 不可探索区域 / Unexplorable zone / 探索不可ゾーン
        public override bool CanDigUnderground => false;   // 不允许挖掘地下 / Cannot dig underground / 地下掘削不可
        public override bool AllowCriminal => false;       // 不允许罪犯进入 / Criminals not allowed / 犯罪者進入不可
        
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
}
