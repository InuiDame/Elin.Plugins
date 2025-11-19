using System.Collections.Generic;
using UnityEngine;

namespace ExtraSpell;

internal static class ModInfos
{
    internal static class ModInfo
    {
        internal const string Guid = "ExtraSpell";
        internal const string Name = "额外魔法";
        internal const string Version = "1.0.2.3";
    }

    internal static readonly HashSet<string> Calcs =
        ["star_", "chain_Lightning", "blowback"];


    private static readonly List<List<string>> GroupSpells =
    [
        [
            "star_Fire", "star_Cold", "star_Lightning",
            "star_Darkness", "star_Mind", "star_Poison",
            "star_Nether", "star_Sound", "star_Nerve",
            "star_Holy", "star_Chaos", "star_Magic",
            "star_Ether", "star_Acid", "star_Cut", "star_Impact"
        ]
    ];

    private static readonly List<string> SingleSpells =
        ["blowback", "chain_Lightning"];

    internal static readonly HashSet<string> MagicList = [];
    internal static readonly List<object> MagicRollList = [];

    static ModInfos()
    {
        foreach (var spell in SingleSpells)
            MagicList.Add(spell);
        MagicRollList.AddRange(SingleSpells);
        foreach (var group in GroupSpells)
        {
            foreach (var spell in group)
                MagicList.Add(spell);
            for (var i = 0; i < group.Count / 4; i++)
            {
                MagicRollList.Add(group);
            }
        }
    }
}