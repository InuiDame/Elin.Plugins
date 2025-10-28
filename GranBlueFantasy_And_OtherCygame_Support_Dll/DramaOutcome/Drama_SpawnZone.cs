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

namespace GBF.dramaOutcome.Drama_SpawnZone
{
    internal class Tiger_Drama_SpawnZone : DramaOutcome
    {
        // 在指定坐标生成海滩区域 / Spawn beach zone at specified coordinates / 指定座標にビーチゾーンを生成
        public static bool SpawnZonebeach_cidala(DramaManager dm, Dictionary<string, string> line, params string[] parameters)
        {
            EClass.world.region.SpawnZoneAt("beach_cidala", -17, -16);  // 生成海滩区域 / Spawn beach zone / ビーチゾーンを生成
            return true;
        }
    }
}
