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
    internal class InvasionConsole : EMono
    {
        [ConsoleCommand("Trigger")]
        internal static string Trigger(int avgLevel = 0)
        {
            var zone = EMono._zone;
            if (zone == null || (!zone.IsTown && !zone.IsPCFaction))
                return "只能在玩家城镇地图使用！";

            long now = EMono.world.date.GetRaw();
            int uid = zone.uid;
            long cd = MonsterInvasionMod.CFG_CooldownDays.Value * 24 * 60;

            // 强制清冷却
            if (MonsterInvasionMod.zoneLastInvasion.TryGetValue(uid, out long last) && now - last < cd)
                MonsterInvasionMod.zoneLastInvasion[uid] = now - cd;

            var ev = new ZoneEventMonsterInvasion { zone = zone };
            ev.Init();
            zone.events.list.Add(ev);
            MonsterInvasionMod.zoneLastInvasion[uid] = now;

            int lv = avgLevel > 0 ? avgLevel :
                (int)zone.map.charas
                    .Where(c => !c.IsPC && !c.IsPCParty && !c.IsMinion)
                    .Select(c => (int)c.LV)
                    .DefaultIfEmpty(1)
                    .Average();

            return $"已触发入侵！平均等级 {lv}。";
        }

        [ConsoleCommand("SetCooldown")]
        internal static string SetCooldown(int days)
        {
            MonsterInvasionMod.CFG_CooldownDays.Value = days;
            return $"入侵冷却时间设置为 {days} 天";
        }

        [ConsoleCommand("SetChance")]
        internal static string SetChance(int percent)
        {
            if (percent < 0 || percent > 100)
                return "概率必须在 0-100 之间";

            MonsterInvasionMod.CFG_Chance.Value = percent;
            return $"入侵概率设置为 {percent}%";
        }

        [ConsoleCommand("ClearCooldown")]
        internal static string ClearCooldown()
        {
            MonsterInvasionMod.zoneLastInvasion.Clear();
            return "已清空所有区域的入侵冷却";
        }

        [ConsoleCommand("Status")]
        internal static string Status()
        {
            var zone = EMono._zone;
            if (zone == null || (!zone.IsTown && !zone.IsPCFaction))
                return "当前不在城镇中";

            int uid = zone.uid;
            long now = EMono.world.date.GetRaw();
            bool onCooldown = MonsterInvasionMod.zoneLastInvasion.TryGetValue(uid, out long last) && 
                             now - last < MonsterInvasionMod.CFG_CooldownDays.Value * 24 * 60;

            return $"当前区域: {zone.uid}\n" +
                   $"冷却状态: {(onCooldown ? "冷却中" : "可触发")}\n" +
                   $"触发概率: {MonsterInvasionMod.CFG_Chance.Value}%\n" +
                   $"冷却天数: {MonsterInvasionMod.CFG_CooldownDays.Value}天";
        }
        
        [ConsoleCommand("Clear")]
        internal static string ClearInvasion()
        {
            var zone = EMono._zone;
            if (zone == null) return "必须先进入地图";

            var ev = zone.events.list.Find(
                e => e.GetType().Name == "ZoneEventMonsterInvasion");
            if (ev == null) return "当前地图没有入侵事件";

            int kill = 0;
            foreach (var c in zone.map.charas.ToList())
                if (c.rarity >= Rarity.Legendary &&
                    c.hostility == Hostility.Enemy &&
                    !c.isDead)
                {
                    c.Die(null, null, AttackSource.None, null); //  公开 API
                    kill++;
                }

            ev.Kill(); //  ZoneEvent 的公开方法
            return $"已强制结束入侵，清除 {kill} 只金名敌人。";
        }
    }
}