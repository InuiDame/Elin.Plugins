using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Configuration;
using Cwl.API.Custom;
using Cwl.Helper.Unity;
using Cwl.LangMod;
using GBF.Modinfo;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using static NoticeManager;
using static UnityEngine.UI.GridLayoutGroup;

namespace Condition_Empowering_Aid
//Empowering Aid
//ケア・リライアント
//疗护依存
{
    public class ConSK2613 : Timebuff
    {
        private Dice healDice;           // 一次性治疗骰子 / One-time heal dice / 一回性治療ダイス
        private Dice hotDice;            // 持续治疗骰子 / Heal over time dice / 持続治療ダイス
        private bool diceInitialized;    // 骰子初始化标志 / Dice initialization flag / ダイス初期化フラグ

        // 幂等初始化骰子与首次治疗 / Idempotent dice initialization and first heal / 冪等なダイス初期化と初回治療
        private void InitDice()
        {
            if (diceInitialized) return;
            // 1) 计算一次性疗伤骰 / Calculate one-time heal dice / 一回性治療ダイスを計算
            int num = Mathf.Max(EClass.curve(owner.DEX * 10, 400, 100), 100);
            healDice = Dice.Create("SpHealEris", num);
            // 2) HOT持续恢复骰 / HOT continuous recovery dice / HOT持続回復ダイス
            hotDice = Dice.Create("SpHOT", power);
            // 3) 首次立刻治疗 / First immediate heal / 初回即時治療
            owner.HealHPHost(healDice.Roll(), HealSource.Magic);

            diceInitialized = true;
        }

        public override void OnStart()
        {
            InitDice();  // 状态开始时初始化骰子 / Initialize dice when condition starts / 状態開始時にダイスを初期化
        }

        public override void Tick()
        {
            // 确保骰子已初始化 / Ensure dice are initialized / ダイスが初期化済みか確認
            InitDice();

            // HOT持续恢复 / HOT continuous recovery / HOT持続回復
            if (hotDice != null)
            {
                owner.HealHPHost(hotDice.Roll(), HealSource.HOT);
            }

            Mod(-1);  // 减少持续时间 / Reduce duration / 持続時間を減少
        }

        public override void OnWriteNote(List<string> list)
        {
            // 在状态说明中添加治疗信息 / Add heal info to status description / 状態説明に治療情報を追加
            if (hotDice != null)
            {
                list.Add($"<color=green>血量持续恢复/HOT +{hotDice}</color>");      // 持续治疗量 / Heal over time amount / 持続治療量
            }
            if (healDice != null)
            {
                list.Add($"<color=green>血量恢复/Heal +{healDice}</color>");        // 单次治疗量 / Single heal amount / 単回治療量
            }
        }
        public override int GetPhase() => 0;  // 固定阶段0 / Fixed phase 0 / 固定フェーズ0
    }

    [HarmonyPatch(typeof(Card), nameof(Card.HealHPHost))]
    static class Patch_HealHPHost_InjectBoost
    {
        // Prefix在原始方法之前运行；如果有buff则调整'amount' / Prefix runs before original method; adjust 'amount' if buff present / Prefixは元のメソッドの前に実行；バフがある場合は'amount'を調整
        static void Prefix(Chara __instance, int a, HealSource origin)
        {
            // 查询是否有疗护依存buff / Check if Empowering Aid buff is present / ケア・リライアントバフが存在するか確認
            var buff = __instance.GetCondition<ConSK2613>();
            if (buff != null)
            {
                // 增加额外的治疗量 / Add extra healing amount / 追加治療量を加算
                // 基于DEX属性的治疗加成 / Healing bonus based on DEX attribute / DEX属性に基づく治療ボーナス
                float factor = DexBoost(__instance.DEX);
                a = Mathf.RoundToInt(a * (1 + factor));
            }
        }
        
        // DEX属性治疗加成计算 / DEX attribute healing bonus calculation / DEX属性治療ボーナス計算
        public static float DexBoost(int dex)
        {
            // 基础加成计算：每400 DEX提供2%加成 / Base bonus calculation: 2% per 400 DEX / 基本ボーナス計算：400 DEXごとに2%ボーナス
            float boost = (dex / 400f) * 0.02f;

            boost = Mathf.Max(boost, 0.05f);  // 最小加成5% / Minimum bonus 5% / 最小ボーナス5%

            boost = Mathf.Min(boost, 2f);     // 最大加成200% / Maximum bonus 200% / 最大ボーナス200%

            return boost;
        }
    }
}
