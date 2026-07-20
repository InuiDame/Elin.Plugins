using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Linq;
using BepInEx;
using Cwl.Helper.Unity;
using HarmonyLib;
using UnityEngine;
using Debug = UnityEngine.Debug;



namespace GBFWeaponGrid.Container
{

public class TraitWeaponGrid : TraitContainer
{
    public override bool CanStack => false;

    // ====================== 内部状态 ======================
    private List<(int eleId, int value)> _appliedEnchants = new();
    private string _lastContentHash = "";

    // ====================== 构造函数 ======================
    public TraitWeaponGrid()
    {
        _appliedEnchants = new List<(int, int)>();
    }

    // ====================== 最安全的 Apply ======================
    private void SafeApplyEnchants()
    {
        try
        {
            var pc = EClass.pc;
            if (pc == null || owner == null || owner.things == null)
                return;

            // 撤销旧附魔
            foreach (var (id, val) in _appliedEnchants.ToList())
            {
                if (val != 0)
                    pc.elements?.ModBase(id, -val);
            }
            _appliedEnchants.Clear();

            // 只在第一个武器盘时生效
            if (!IsActiveGrid())
                return;

            // 应用武器附魔
            foreach (var weapon in owner.things)
            {
                if (weapon?.elements?.dict == null) continue;

                foreach (var kv in weapon.elements.dict)
                {
                    int eleId = kv.Key;
                    Element e = kv.Value;
                    if (e == null) continue;

                    int add = e.Value;
                    if (add <= 0) continue;

                    pc.elements?.ModBase(eleId, add);
                    _appliedEnchants.Add((eleId, add));
                }
            }

            pc.Refresh();
        }
        catch
        {
            // 什么都不做，防止加载崩溃
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
            if (owner?.things == null || owner.things.Count == 0)
                return "";

            return string.Join("|",
                owner.things
                    .Where(t => t != null)
                    .OrderBy(t => t.uid)
                    .Select(t => $"{t.id}_{t.c_idRefCard ?? ""}"));
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
        _appliedEnchants.Clear();
        _lastContentHash = "";
    }

    public override void OnSetOwner()
    {
        base.OnSetOwner();
        SafeApplyEnchants();
    }

    public override void OnChangePlaceState(PlaceState state)
    {
        base.OnChangePlaceState(state);
        SafeApplyEnchants();
    }

    public override void OnRemovedFromZone()
    {
        base.OnRemovedFromZone();

        var pc = EClass.pc;
        if (pc != null)
        {
            foreach (var (id, val) in _appliedEnchants)
            {
                if (val != 0)
                    pc.elements?.ModBase(id, -val);
            }
        }
        _appliedEnchants.Clear();
        pc?.Refresh();
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

        string hash = GetContentHash();
        if (hash != _lastContentHash)
        {
            SafeApplyEnchants();
            _lastContentHash = hash;
        }
    }

    // ====================== 反序列化处理（最关键） ======================
    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
        if (_appliedEnchants == null)
            _appliedEnchants = new List<(int, int)>();

        _lastContentHash = "";

        // 最安全的延迟方式：下一帧再刷新（不使用任何可能不存在的方法）
        // 我们用一个简单的标志，让 OnSimulateHour 或 Open 时自动触发
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
            //Debug.Log("武器盘已加载");
        }
    }
}