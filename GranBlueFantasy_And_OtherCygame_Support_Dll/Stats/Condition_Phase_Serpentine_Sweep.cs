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

namespace Condition_Phase_Serpentine_Sweep
{
    public class ConSK2506 : BadCondition
// 流刀蛇尾 / Serpentine Sweep / 流刀蛇尾
    {
        public override Emo2 EmoIcon => Emo2.poison; // 使用中毒表情图标 / Use poison emotion icon / 毒エモーションアイコンを使用
        public override bool UseElements => true;    // 使用元素系统 / Use element system / エレメントシステムを使用
        public override bool PreventRegen => true;   // 阻止生命恢复 / Prevent HP regeneration / HP回復を阻止

        public override void OnChangePhase(int lastPhase, int newPhase)
        {
            // 阶段变化时调整元素效果 / Adjust element effects when phase changes / フェーズ変更時にエレメント効果を調整
            switch (newPhase)
            {
                case 1:
                    elements.SetBase(70, -10);  // 阶段1：元素70减少10 / Phase 1: Element 70 reduced by 10 / フェーズ1：エレメント70を10減少
                    break;
                case 2:
                    elements.SetBase(70, -10);  // 阶段2：元素70减少10 / Phase 2: Element 70 reduced by 10 / フェーズ2：エレメント70を10減少
                    break;
                case 3:
                    elements.SetBase(70, -15);  // 阶段3：元素70减少15 / Phase 3: Element 70 reduced by 15 / フェーズ3：エレメント70を15減少
                    break;
                default:
                    elements.SetBase(70, -5);   // 默认阶段：元素70减少5 / Default phase: Element 70 reduced by 5 / デフォルトフェーズ：エレメント70を5減少
                    break;
            }
        }

        public override void Tick()
        {
            // 20%概率造成伤害 / 20% chance to deal damage / 20%の確率でダメージを与える
            if (EClass.rnd(5) == 0)
            {
                // 基于END属性的伤害计算 / Damage calculation based on END attribute / END属性に基づくダメージ計算
                owner.DamageHP(EClass.rnd(owner.END / 10 + 5) + 1, AttackSource.Condition);
            }

            Mod(-1);  // 减少状态持续时间 / Reduce condition duration / 状態持続時間を減少
        }
    }
}
