using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using MonsterInvasionMod;
using Newtonsoft.Json;
using ReflexCLI;
using ReflexCLI.UI;
using Object = UnityEngine.Object;

namespace MonsterInvasionMod
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class MonsterInvasionMod : BaseUnityPlugin
    {
        public const string PluginGuid = "inui.outbreak.invasion";
        public const string PluginName = "MonsterInvasion";
        public const string PluginVersion = "1.0.2";

        public static ConfigEntry<int> CFG_Chance;
        public static ConfigEntry<int> CFG_CooldownDays;
        public static ConfigEntry<bool> CFG_EnableLog;
        public static ConfigEntry<int> CFG_NpcMinLv;   // NPC最低等级
        public static ConfigEntry<int> CFG_NpcMaxLv;   // NPC最高等级
        public static ConfigEntry<int> CFG_SizeRoll;

        public static Dictionary<int, long> zoneLastInvasion = new Dictionary<int, long>();
        

        private void Awake()
        {
            CFG_Chance       = Config.Bind("General", "InvasionChance", 15);
            CFG_CooldownDays = Config.Bind("General", "CooldownDays", 90);
            CFG_EnableLog    = Config.Bind("General", "EnableLog", true);
            CFG_NpcMinLv     = Config.Bind("General", "NpcMinLevel", 1);
            CFG_NpcMaxLv     = Config.Bind("General", "NpcMaxLevel", 1999);
            CFG_SizeRoll = Config.Bind("General", "InvasionSizeRoll", 0,
                "0=随机 1=强制小型 2=中型 3=大型 4=超大型 ≥5=自定义数量");
            zoneLastInvasion = zoneLastInvasion ?? new Dictionary<int, long>();


            CommandRegistry.assemblies.Add(GetType().Assembly);
            _ = typeof(InvasionConsole);
            CommandRegistry.Rebuild();
            
            new Harmony(PluginGuid).PatchAll();
            Log("插件加载完成！");

        }

        public static void Log(object msg)
        {
            if (CFG_EnableLog.Value) Debug.Log($"[{PluginName}] {msg}");
        }

        /* -------------------------------------------------- */
        // 多语言封装
        public static string Localize(string jp, string en, string cn)
        {
            string lang = EClass.core.config.lang;
            if (lang == "JP") return jp;
            if (lang == "CN" || lang == "ZHTW") return cn;
            return en;
        }
    }

    /* ====================================================== */
    public class ZoneEventMonsterInvasion : ZoneEvent
    {
        
        public override string id => "monsterInvasion";
        public override string TextWidgetDate =>
            MonsterInvasionMod.Localize(
                $"怪物入侵中…残り{invaderUIDs.Count}体",
                $"Monster Invasion…{invaderUIDs.Count} left",
                $"怪物入侵中…剩余{invaderUIDs.Count}只");

        public override int hoursToKill => 24 * 7;

        private List<int> invaderUIDs = new List<int>();
        private bool rewardGiven = false;
        
        private QuestTaskMonsterInvasion task;
        private Quest quest;

        public override void OnInit()
        {
            List<Chara> citizens = zone.map.charas.Where(c =>
                !c.IsPC && !c.IsPCParty && !c.IsMinion).ToList();

            if (!citizens.Any()) return;
            
            int pop = zone.map.charas.Count(c => !c.IsPC && !c.IsPCParty && !c.IsMinion);
            int size = MonsterInvasionMod.CFG_SizeRoll.Value switch
            {
                1 => pop / 8, // 小型
                2 => pop / 4, // 中型
                3 => pop / 2, // 大型
                4 => pop,     // 超大型（新增）
                _ when MonsterInvasionMod.CFG_SizeRoll.Value >= 5 => 
                    Mathf.Min(MonsterInvasionMod.CFG_SizeRoll.Value, 200), // 自定义数量，上限100
                _ => EClass.rnd(100) switch                                // 默认随机（0或其他无效值）
                {
                    < 50 => pop / 8,
                    < 85 => pop / 4,
                    _    => pop / 2
                }
            };
            size = Mathf.Clamp(size, 1, 200);   // 上限 200 只

            int avgLevel = (int)citizens.Average(c => (int)c.LV);
            int count    = size;
            int lv       = avgLevel + EClass.rnd(3) + 1;
            
            
            for (int i = 0; i < count; i++)
            {
                var setting = new SpawnSetting
                {
                    filterLv   = avgLevel,
                    rarity = Rarity.Legendary,
                    hostility  = SpawnHostility.Enemy   // 明确指定敌对
                };
                
                // 地图中央
                int cx = zone.map.Size / 2;
                int cz = zone.map.Size / 2;
                Point center = new Point(cx, cz);

// 找最近可站立格子
                center = center.GetNearestPoint(allowBlock: false, allowChara: false, allowInstalled: true, ignoreCenter: true);

// 生成怪物
                Chara mob = zone.SpawnMob(center, setting);
                if (mob != null)
                {
                    mob.LV = lv;
                    mob.hostility = Hostility.Enemy;         // 立即改敌对
                    invaderUIDs.Add(mob.uid);
                }
            }
            
            // 创建任务并绑定
            task = new QuestTaskMonsterInvasion();
            task.SetCount(invaderUIDs.Count);

            quest = Quest.Create("custom_monsterInvasion");
            quest.SetTask(task);
            quest.SetClient(EClass.pc); // 让玩家成为委托人
            quest.Start();              // 开始计时/刷新日志
            EClass.game.quests.globalList.Add(quest);
            quest.track = true;
            quest.UpdateJournal(); 

            string msg = MonsterInvasionMod.Localize(
                "怪物が街に侵入した！倒して報酬を得よう！",
                "Monsters have invaded the town! Defeat them for rewards!",
                "怪物入侵了城镇！击败它们以获得奖励！");
            Msg.Say(msg);
            MonsterInvasionMod.Log($"生成入侵：{count} 只，平均等级 {lv}");
        }

        public override void OnTickRound()
        {
            if (zone?.map == null) return;
            // 更新存活列表
            invaderUIDs.RemoveAll(uid =>
            {
                Chara c = zone.map.charas.Find(ch => ch.uid == uid);
                return c == null || c.isDead;
            });

// 实时刷新任务进度
            int alive = invaderUIDs.Count;
            if (task != null)           // 再加个空保护
                task.killed = task.total - alive;

            if (alive == 0 && !rewardGiven)
            {
                rewardGiven = true;
                quest.Complete(); // 任务完成 + 弹出奖励面板
                GiveRewards();    // 你原来的金钱/装备
                BuffCitizens();
                Kill();             // 事件自我销毁
            }
        }

        private void GiveRewards()
        {
            List<Chara> citizens = zone.map.charas.Where(c =>
                !c.IsPC && !c.IsPCParty && !c.IsMinion).ToList();
            int avg = citizens.Any() ? (int)citizens.Average(c => (int)c.LV) : 1;

            int money  = 5000 + avg * 100;
            int money2 = 5 + avg / 5;
            int plat   = 10 + avg / 3;

            EClass.pc.AddThing(ThingGen.Create("money").SetNum(money));
            EClass.pc.AddThing(ThingGen.Create("money2").SetNum(money2));
            EClass.pc.AddThing(ThingGen.Create("plat").SetNum(plat));

            /* -------- 装备：件数、保底蓝装、更高附魔等级 -------- */
            int eqCount = 2;
            for (int i = 0; i < eqCount; i++)
            {
                Thing eq = ThingGen.CreateFromFilter("eq", avg);

                // 1. 保底蓝装（Superior）
                eq.rarity = Rarity.Superior;

                // 2. 50级起步必然金装
                if (avg >= 50) eq.rarity = Rarity.Legendary;

                // 3. 神话几率（基础2%，50级后每10级+1%，100级后每5级+1%）
                int mythBonus = avg < 50 ? 0 :
                    avg < 100 ? (avg - 50) / 10 :
                    5 + (avg - 100) / 5;
                if (EClass.rnd(100) < 2 + mythBonus) eq.rarity = Rarity.Mythical;
                
                int minEnc = eq.rarity switch
                {
                    Rarity.Superior => 3,
                    Rarity.Legendary => 5,
                    Rarity.Mythical => 7,
                    _ => 1
                };
                for (int k = 0; k < minEnc; k++)
                    eq.AddEnchant(avg);    

                // 4. 附魔等级数值（次数不变，只提高等级）
                int extraLicks = avg / 5;          // 比原来 /10 翻倍
                for (int l = 0; l < 3 + extraLicks; l++)
                    eq.TryLickEnchant(EClass.pc, msg: false);

                EClass.pc.AddThing(eq);
            }


            string msg = MonsterInvasionMod.Localize(
                "報酬を獲得した！金貨、金塊、白金币、そして装備2件！",
                "Rewards obtained! Gold, gold bars, platinum, and 2 pieces of equipment!",
                "获得奖励！金币、金块、白金币与两件装备！");
            Msg.Say(msg);
            MonsterInvasionMod.Log($"奖励：money={money}, money2={money2}, plat={plat}, 装备等级≈{avg}");
            
            // 确保字典存在
            if (MonsterInvasionMod.zoneLastInvasion == null)
                MonsterInvasionMod.zoneLastInvasion = new Dictionary<int, long>();
            MonsterInvasionMod.zoneLastInvasion[zone.uid] = EClass.world.date.GetRaw();
        }

        private void BuffCitizens()
        {
            int add = EClass.rnd(5) + 1; // 1~5
            foreach (Chara c in zone.map.charas.Where(ch =>
                !ch.IsPC && !ch.IsPCParty))
            {
                int next = c.LV + add;
                // 等级保护
                if (next > MonsterInvasionMod.CFG_NpcMaxLv.Value)
                    next = MonsterInvasionMod.CFG_NpcMaxLv.Value;
                if (next < MonsterInvasionMod.CFG_NpcMinLv.Value)
                    next = MonsterInvasionMod.CFG_NpcMinLv.Value;
                c.LV = next;
            }
            string msg = MonsterInvasionMod.Localize(
                $"町のNPCのレベルが最大{add}上昇した！",
                $"Town NPCs level increased by up to {add}!",
                $"城镇NPC等级最高上升了{add}！");
            Msg.Say(msg);
        }

        public override void OnKill()
        {
            invaderUIDs.Clear();
        }

        public override void OnLoad()
        {
            invaderUIDs.RemoveAll(uid =>
            {
                Chara c = zone.map.charas.Find(ch => ch.uid == uid);
                return c == null || c.isDead;
            });
        }
    }

    /* ====================================================== */
    [HarmonyPatch(typeof(Zone), "OnVisit")]
    static class Zone_OnVisit_Patch
    {
        static void Prefix(Zone __instance)
        {
            if (!__instance.IsTown && !__instance.IsPCFaction) return;
            
            long now = EClass.world.date.GetRaw();
            int uid  = __instance.uid;
            long cd  = MonsterInvasionMod.CFG_CooldownDays.Value * 24 * 60;
            

            if (MonsterInvasionMod.zoneLastInvasion.TryGetValue(uid, out long last) &&
                now - last < cd) return;

            if (EClass.rnd(100) >= MonsterInvasionMod.CFG_Chance.Value) return;

            var ev = new ZoneEventMonsterInvasion { zone = __instance };
            ev.Init();
            __instance.events.list.Add(ev);
            MonsterInvasionMod.zoneLastInvasion[uid] = now;
            MonsterInvasionMod.Log($"触发入侵事件，区域UID={uid}");
        }
    }

    /* ====================================================== */
    // 写入多语言条目
    [HarmonyPatch(typeof(SourceManager), "Init")]
static class SourceManager_Init_Patch
{
    static bool done = false;
    static void Postfix(SourceManager __instance)
    {
        if (done) return;
        done = true;

        /* ========== 1. 先注入缺省英文源数据 ========== */
        if (!EClass.sources.quests.map.ContainsKey("custom_monsterInvasion"))
        {
            var sq = new SourceQuest.Row
            {
                id              = "custom_monsterInvasion",
                name_JP         = "怪物入侵",
                name            = "Monster Invasion",
                type            = "",
                drama           = new string[0],
                idZone          = "",
                group           = "",
                tags            = new string[0],
                money           = 0,
                chance          = 100,
                minFame         = 0,
                detail_JP       = "侵略者を全て倒せ！",
                detail          = "Defeat all invading monsters!",
                talkProgress_JP = "",
                talkProgress    = "",
                talkComplete_JP = "",
                talkComplete    = ""
            };
            EClass.sources.quests.map[sq.id] = sq;
        }

        /* ========== 2. 按当前语言给 _L 字段赋值 ========== */
        string lang = EClass.core.config.lang;
        var  rowQ   = EClass.sources.quests.map["custom_monsterInvasion"];
        switch (lang)
        {
            case "CN":
            case "ZHTW":
                rowQ.name_L   = "怪物入侵";
                rowQ.detail_L = "击败所有入侵怪物！";
                break;
            case "JP":
                rowQ.name_L   = "怪物入侵";
                rowQ.detail_L = "侵略者を全て倒せ！";
                break;
            default: // EN / 其他
                rowQ.name_L   = rowQ.name;
                rowQ.detail_L = rowQ.detail;
                break;
        }

        /* ========== 3. 补 LangGame 行（右侧飘字 + 任务日志标题） ========== */
        // 事件飘字
        var rowEv = new LangGame.Row
        {
            id       = "event_monsterInvasion",
            filter   = "",
            group    = "",
            color    = "sign",
            logColor = "",
            sound    = "",
            effect   = "",
            text_JP  = "怪物が街に侵入した！倒して報酬を得よう！",
            text     = "Monsters have invaded the town! Defeat them for rewards!",
            text_L   = lang == "JP" ? "怪物が街に侵入した！倒して報酬を得よう！"
                     : lang == "CN" || lang == "ZHTW"
                     ? "怪物入侵了城镇！击败它们以获得奖励！"
                     : "Monsters have invaded the town! Defeat them for rewards!"
        };
        __instance.langGame.rows.Add(rowEv);
        __instance.langGame.SetRow(rowEv);

        // 任务日志标题
        var rowQt = new LangGame.Row
        {
            id      = "custom_monsterInvasion",
            text_JP = "怪物入侵",
            text    = "Monster Invasion",
            text_L  = lang == "JP" ? "怪物入侵"
                     : lang == "CN" || lang == "ZHTW" ? "怪物入侵"
                     : "Monster Invasion"
        };
        __instance.langGame.rows.Add(rowQt);
        __instance.langGame.SetRow(rowQt);
    }
}
    public class QuestTaskMonsterInvasion : QuestTask
    {
        public int total;
        public int killed;

        public void SetCount(int _total) => total = _total;
        public void IncKill()            => killed++;

        public override string GetTextProgress()
            => $"击败入侵者: {killed} / {total}";

        public override bool IsComplete()
            => killed >= total;
    }
    
    [HarmonyPatch]
    public static class KarmaPatch
    {
        /* 拦截 Player.ModKarma，覆盖 EClass.player.ModKarma(...) 路径 */
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Player), nameof(Player.ModKarma))]
        static bool StopKarmaLossIfInvasion(int a)
        {
            /* 只拦负数（扣善恶），正数（奖励）不管 */
            if (a >= 0) return true;

            /* 当前地图有没有正在进行的入侵事件？ */
            Zone z = EClass._zone;
            if (z?.events?.list?.Any(ev => ev is ZoneEventMonsterInvasion) == true)
            {
                MonsterInvasionMod.Log("入侵中，屏蔽善恶值下降");
                return false; // 跳过原方法
            }

            return true; // 正常扣
        }
    }
}