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

namespace Condition_Phase_Wolf_in_Sea_Clothing
//Wolf in Sea's Clothing / 大海真神 / 大海真神
{
    public class ConSK1785 : Timebuff
    {
        public override bool UseElements => true;        // 使用元素系统 / Use element system / エレメントシステムを使用
        public override bool CanManualRemove => true;    // 允许手动移除 / Allow manual removal / 手動削除可能

        public override BaseNotification CreateNotification()
        {
            // 创建状态效果通知 / Create status effect notification / 状態効果通知を作成
            return new NotificationBuff
            {
                condition = this
            };
        }

        public override void OnChangePhase(int lastPhase, int newPhase)
        {
            // 根据阶段变化设置元素411的值 / Set element 411 value based on phase change / フェーズ変化に基づいてエレメント411の値を設定
            switch (newPhase)
            {
                case 0:
                    elements.SetBase(411, 10);  // 阶段0：元素411设为10 / Phase 0: Element 411 set to 10 / フェーズ0：エレメント411を10に設定
                    break;
                case 1:
                    elements.SetBase(411, 20);  // 阶段1：元素411设为20 / Phase 1: Element 411 set to 20 / フェーズ1：エレメント411を20に設定
                    break;
                case 2:
                    elements.SetBase(411, 30);  // 阶段2：元素411设为30 / Phase 2: Element 411 set to 30 / フェーズ2：エレメント411を30に設定
                    break;
                case 3:
                    elements.SetBase(411, 40);  // 阶段3：元素411设为40 / Phase 3: Element 411 set to 40 / フェーズ3：エレメント411を40に設定
                    break;
                case 4:
                    elements.SetBase(411, 50);  // 阶段4：元素411设为50 / Phase 4: Element 411 set to 50 / フェーズ4：エレメント411を50に設定
                    break;
                case 5:
                    elements.SetBase(411, 60);  // 阶段5：元素411设为60 / Phase 5: Element 411 set to 60 / フェーズ5：エレメント411を60に設定
                    break;
                case 6:
                    elements.SetBase(411, 70);  // 阶段6：元素411设为70 / Phase 6: Element 411 set to 70 / フェーズ6：エレメント411を70に設定
                    break;
                case 7:
                    elements.SetBase(411, 80);  // 阶段7：元素411设为80 / Phase 7: Element 411 set to 80 / フェーズ7：エレメント411を80に設定
                    break;
                case 8:
                    elements.SetBase(411, 90);  // 阶段8：元素411设为90 / Phase 8: Element 411 set to 90 / フェーズ8：エレメント411を90に設定
                    break;
                case 9:
                    elements.SetBase(411, 100); // 阶段9：元素411设为100 / Phase 9: Element 411 set to 100 / フェーズ9：エレメント411を100に設定
                    break;
                case 10:
                    elements.SetBase(411, 110); // 阶段10：元素411设为110 / Phase 10: Element 411 set to 110 / フェーズ10：エレメント411を110に設定
                    break;
                case 11:
                    elements.SetBase(411, 120); // 阶段11：元素411设为120 / Phase 11: Element 411 set to 120 / フェーズ11：エレメント411を120に設定
                    break;
            }
        }

        public override void OnStartOrStack()
        {
            // 状态开始或堆叠时初始化元素411为10 / Initialize element 411 to 10 when condition starts or stacks / 状態開始またはスタック時にエレメント411を10で初期化
            owner.elements.SetBase(411, 10);
        }

        public override void Tick()
        {
            Mod(-1);  // 减少持续时间 / Reduce duration / 持続時間を減少
        }
    }
}
