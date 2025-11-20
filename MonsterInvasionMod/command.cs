using System.Linq;
using Cwl.API.Attributes;
using Cwl.Helper.Extensions;
using Cwl.LangMod;
using HarmonyLib;
using ReflexCLI.Attributes;
using ReflexCLI.UI;
using UnityEngine;

namespace MonsterInvasionMod
{
    [ConsoleCommandClassCustomizer("invasion")]
    internal class InvasionConsole
    {
        private static string Localize(string jp, string en, string cn)
        {
            string lang = EClass.core?.config?.lang ?? "EN";
            if (lang == "JP") return jp;
            if (lang == "CN" || lang == "ZHTW") return cn;
            return en;
        }

        [ConsoleCommand("Trigger")]
        internal static string Trigger(int avgLevel = 0)
        {
            var zone = EClass._zone;
            if (zone == null || (!zone.IsTown && !zone.IsPCFaction))
                return Localize(
                    "プレイヤーの町マップでのみ使用できます！",
                    "Can only be used in player town maps!",
                    "只能在玩家城镇地图使用！");

            if (zone.events.list.Any(e => e is ZoneEventMonsterInvasion))
                return Localize(
                    "既に侵略イベントが進行中です",
                    "Invasion event already in progress",
                    "已有入侵事件进行中");

            long now = EClass.world.date.GetRaw();
            int uid = zone.uid;
            long cd = MonsterInvasionMod.CFG_CooldownDays.Value * 24 * 60;

            if (MonsterInvasionMod.zoneLastInvasion.TryGetValue(uid, out long last) && now - last < cd)
                MonsterInvasionMod.zoneLastInvasion[uid] = now - cd;

            var ev = new ZoneEventMonsterInvasion { zone = zone };
            ev.Init();
            zone.events.list.Add(ev);
            MonsterInvasionMod.zoneLastInvasion[uid] = now;

            return Localize("侵略をトリガーしました！", "Invasion triggered!", "已触发入侵！");
        }

        [ConsoleCommand("SetCooldown")]
        internal static string SetCooldown(int days)
        {
            MonsterInvasionMod.CFG_CooldownDays.Value = days;
            return Localize(
                $"侵略クールダウンを {days} 日に設定しました",
                $"Invasion cooldown set to {days} days",
                $"入侵冷却时间设置为 {days} 天");
        }

        [ConsoleCommand("SetChance")]
        internal static string SetChance(int percent)
        {
            if (percent < 0 || percent > 100)
                return Localize("確率は0〜100の間でなければなりません", "Probability must be between 0-100", "概率必须在 0-100 之间");

            MonsterInvasionMod.CFG_Chance.Value = percent;
            return Localize(
                $"侵略確率を {percent}% に設定しました",
                $"Invasion probability set to {percent}%",
                $"入侵概率设置为 {percent}%");
        }

        [ConsoleCommand("ClearCooldown")]
        internal static string ClearCooldown()
        {
            MonsterInvasionMod.zoneLastInvasion.Clear();
            return Localize(
                "全ての地域の侵略クールダウンをクリアしました",
                "Cleared invasion cooldown for all regions",
                "已清空所有区域的入侵冷却");
        }

        [ConsoleCommand("Status")]
        internal static string Status()
        {
            var zone = EClass._zone;
            if (zone == null || (!zone.IsTown && !zone.IsPCFaction))
                return Localize("現在町の中にいません", "Not currently in a town", "当前不在城镇中");

            int uid = zone.uid;
            long now = EClass.world.date.GetRaw();
            bool onCooldown = MonsterInvasionMod.zoneLastInvasion.TryGetValue(uid, out long last) &&
                             now - last < MonsterInvasionMod.CFG_CooldownDays.Value * 24 * 60;

            bool hasInvasion = zone.events.list.Any(e => e is ZoneEventMonsterInvasion);

            return Localize(
                $"現在の地域: {uid}\n" +
                $"侵略イベント: {(hasInvasion ? "進行中" : "なし")}\n" +
                $"クールダウン状態: {(onCooldown ? "クールダウン中" : "トリガー可能")}\n" +
                $"トリガー確率: {MonsterInvasionMod.CFG_Chance.Value}%\n" +
                $"クールダウン日数: {MonsterInvasionMod.CFG_CooldownDays.Value}日",

                $"Current region: {uid}\n" +
                $"Invasion event: {(hasInvasion ? "In progress" : "None")}\n" +
                $"Cooldown status: {(onCooldown ? "On cooldown" : "Can trigger")}\n" +
                $"Trigger probability: {MonsterInvasionMod.CFG_Chance.Value}%\n" +
                $"Cooldown days: {MonsterInvasionMod.CFG_CooldownDays.Value} days",

                $"当前区域: {uid}\n" +
                $"入侵事件: {(hasInvasion ? "进行中" : "无")}\n" +
                $"冷却状态: {(onCooldown ? "冷却中" : "可触发")}\n" +
                $"触发概率: {MonsterInvasionMod.CFG_Chance.Value}%\n" +
                $"冷却天数: {MonsterInvasionMod.CFG_CooldownDays.Value}天");
        }

        [ConsoleCommand("Clear")]
        internal static string ClearInvasion()
        {
            var zone = EClass._zone;
            if (zone == null)
                return Localize("まずマップに入る必要があります", "Must enter a map first", "必须先进入地图");

            var invasionEvent = zone.events.list.FirstOrDefault(e => e is ZoneEventMonsterInvasion) as ZoneEventMonsterInvasion;
            if (invasionEvent == null)
                return Localize("現在のマップに侵略イベントはありません", "No invasion event in current map", "当前地图没有入侵事件");

            invasionEvent.OnLeaveZone();
            return Localize("侵略イベントを強制終了しました", "Invasion event forcibly ended", "已强制结束入侵事件");
        }
    }
}