using System;
using System.Collections.Generic;
using System.Linq;
using Cwl.LangMod;
using UnityEngine;

namespace ExtraSpell;

internal static class SpellTools
{
    public static void FixMagic()
    {
        var sources = Core.Instance.sources;
        foreach (var magic in ModInfos.MagicList)
        {
            var row = sources.elements.rows.FirstOrDefault(r => r.alias == magic);
            if (row == null)
            {
                Debug.LogError($"ES_Fix_Nus".Loc(magic));
                continue;
            }

            row.value *= (int)MagicReprog.SBookValue.Value;
            row.cost = [(int)(row.cost[0] * MagicReprog.MPcost.Value)];
            row.radius *= (int)MagicReprog.Sdistance.Value;

            Debug.Log("ES_Fix_Done".Loc(row.alias,row.radius,row.cost[0],row.value));
        }

        sources.elements.Reset();
        sources.elements.Init();
        Debug.Log("ES_Fix_AllDone".Loc());
    }

    internal static List<Point> ListPointsInStar(Point center, float radius, Map map, bool ro = false)
    {
        var r = (int)radius;
        var size = map.Size;
        var starPoints = new HashSet<Point> { center.Copy() };

        bool InBounds(int x, int z) => x >= 0 && x < size && z >= 0 && z < size;

        for (var i = 1; i <= r; i++)
        {
            if (ro)
            {
                var rr = (int)Math.Round(i * 0.707);
                var x1 = center.x - rr; var z1 = center.z - rr;
                var x2 = center.x + rr; var z2 = center.z - rr;
                var x3 = center.x - rr; var z3 = center.z + rr;
                var x4 = center.x + rr; var z4 = center.z + rr;
                if (InBounds(x1, z1)) starPoints.Add(new Point(x1, z1));
                if (InBounds(x2, z2)) starPoints.Add(new Point(x2, z2));
                if (InBounds(x3, z3)) starPoints.Add(new Point(x3, z3));
                if (InBounds(x4, z4)) starPoints.Add(new Point(x4, z4));
            }
            else
            {
                var x = center.x; var z = center.z;
                var upZ = z - i; var downZ = z + i; var leftX = x - i; var rightX = x + i;
                if (InBounds(x, upZ)) starPoints.Add(new Point(x, upZ));
                if (InBounds(x, downZ)) starPoints.Add(new Point(x, downZ));
                if (InBounds(leftX, z)) starPoints.Add(new Point(leftX, z));
                if (InBounds(rightX, z)) starPoints.Add(new Point(rightX, z));
            }
        }

        return new List<Point>(starPoints);
    }
}