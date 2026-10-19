using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Linq;
using BepInEx;
using GBFWeaponGrid.Container;
using HarmonyLib;
using UnityEngine;
using Debug = UnityEngine.Debug;



namespace GBFWeaponGrid.Container
{

public class TraitWeaponGrid : TraitContainer
{
    [Serializable]
    public class AppliedEnchantEntry
    {
        public int id;
        public int val;
    }
    public override bool CanStack => false;

    // 用可序列化类代替 ValueTuple
    private List<AppliedEnchantEntry> _appliedEnchants = new();
    private string _lastContentHash = "";
    
    // 读档后需要重新应用（不序列化）
    [NonSerialized] private bool _needsRefresh = true;

    public void ForceRefresh()
    {
        _needsRefresh = true;
        SafeApplyEnchants();
        _lastContentHash = GetContentHash();
    }

    // ====================== 最安全的 Apply ======================
    private void SafeApplyEnchants()
    {
        try
        {
            var pc = EClass.pc;
            if (pc == null || owner == null || owner.things == null)
                return;

            // 1. 撤销旧加成（如果 _appliedEnchants 是空的，
            //    说明是读档/首次，pc 上也未必有残留，先按记录撤）
            foreach (var entry in _appliedEnchants)
            {
                if (entry != null && entry.val != 0)
                    pc.elements?.ModBase(entry.id, -entry.val);
            }
            _appliedEnchants.Clear();

            // 2. 只有第一个武器盘生效
            if (!IsActiveGrid())
            {
                _needsRefresh = false;
                pc.Refresh();
                return;
            }

            // 3. 应用
            foreach (var weapon in owner.things)
            {
                if (weapon?.elements?.dict == null) continue;
                foreach (var kv in weapon.elements.dict)
                {
                    Element e = kv.Value;
                    if (e == null) continue;
                    int add = e.Value;
                    if (add <= 0) continue;

                    pc.elements?.ModBase(kv.Key, add);
                    _appliedEnchants.Add(new AppliedEnchantEntry { id = kv.Key, val = add });
                }
            }

            _needsRefresh = false;
            pc.Refresh();
        }
        catch
        {
            _needsRefresh = false;
        }
    }

    private bool IsActiveGrid()
    {
        try
        {
            if (owner == null || EClass.pc == null || owner.GetRootCard() != EClass.pc)
                return false;

            return EClass.pc.things
                .Where(t => t != null && t.trait is TraitWeaponGrid)
                .OrderBy(t => EClass.pc.things.IndexOf(t))
                .FirstOrDefault() == owner.Thing;
        }
        catch
        {
            return false;
        }
    }

    private string GetContentHash()
    {
        try
        {
            if (owner?.things == null || owner.things.Count == 0) return "";
            return string.Join("|",
                owner.things
                    .Where(t => t != null)
                    .OrderBy(t => t.uid)
                    .Select(t =>
                    {
                        var elems = t.elements?.dict != null
                            ? string.Join(",", t.elements.dict
                                .OrderBy(k => k.Key)
                                .Where(k => k.Value != null)
                                .Select(k => $"{k.Key}:{k.Value.Value}"))
                            : "";
                        return $"{t.id}_{t.c_idRefCard ?? ""}_{elems}";
                    }));
        }
        catch
        {
            return "";
        }
    }

    // ====================== 生命周期 ======================
    public override void OnCreate(int lv)
    {
        base.OnCreate(lv);
        _appliedEnchants = new List<AppliedEnchantEntry>();
        _lastContentHash = "";
        _needsRefresh = true;
    }

    public override void OnSetOwner()
    {
        base.OnSetOwner();
        SafeApplyEnchants();
        _lastContentHash = GetContentHash();
    }

    public override void OnChangePlaceState(PlaceState state)
    {
        base.OnChangePlaceState(state);
        // 位置变化可能影响"谁第一个"，刷新所有武器盘
        RefreshAllGrids();
    }

    public override void OnRemovedFromZone()
    {
        base.OnRemovedFromZone();
        var pc = EClass.pc;
        if (pc != null)
        {
            foreach (var entry in _appliedEnchants)
            {
                if (entry != null && entry.val != 0)
                    pc.elements?.ModBase(entry.id, -entry.val);
            }
        }
        _appliedEnchants.Clear();
        pc?.Refresh();
        RefreshAllGrids();
    }

    public override void Open()
    {
        base.Open();
        SafeApplyEnchants();
        _lastContentHash = GetContentHash();
    }

    public override void OnSimulateHour(VirtualDate date)
    {
        base.OnSimulateHour(date);
        if (_needsRefresh)
        {
            SafeApplyEnchants();
            _lastContentHash = GetContentHash();
            return;
        }
        string hash = GetContentHash();
        if (hash != _lastContentHash)
        {
            SafeApplyEnchants();
            _lastContentHash = hash;
        }
    }

    // ============ 反序列化 ============
    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
        if (_appliedEnchants == null)
            _appliedEnchants = new List<AppliedEnchantEntry>();
        _lastContentHash = "";
        _needsRefresh = true;
        // 反序列化时不直接访问 EClass.pc，等 mod 层协程统一 ForceRefresh
    }
    private static void RefreshAllGrids()
    {
        try
        {
            var pc = EClass.pc;
            if (pc?.things == null) return;
            foreach (var t in pc.things)
            {
                if (t?.trait is TraitWeaponGrid grid)
                    grid.SafeApplyEnchants();
            }
        }
        catch { }
    }
}
}



namespace GBFWeaponGrid.info
{


    [BepInPlugin("GBFWeaponGrid", "GBF武器盘", "1.0.0")]
    internal class GBFWeaponGrid : BaseUnityPlugin
    {
        private void Start() {
            Harmony harmony = new Harmony("inui.GBFWeaponGrid");
            harmony.PatchAll();
            StartCoroutine(InitialRefresh());
            //Debug.Log("武器盘已加载");
        }
        // 读档/进入游戏后，等 pc 就绪再强制刷新一次
        private System.Collections.IEnumerator InitialRefresh()
        {
            yield return null;
            yield return null;
            for (int i = 0; i < 20; i++)
            {
                try
                {
                    if (EClass.pc?.things != null)
                    {
                        var grids = EClass.pc.things
                            .Where(t => t?.trait is TraitWeaponGrid)
                            .Select(t => t.trait as TraitWeaponGrid)
                            .Where(g => g != null)
                            .ToList();
                        if (grids.Count > 0)
                        {
                            foreach (var g in grids) g.ForceRefresh();
                            yield break;
                        }
                    }
                }
                catch { }
                yield return null;
            }
        }
    }
}