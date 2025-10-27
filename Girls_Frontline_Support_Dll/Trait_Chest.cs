using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Reflection.Emit;
using UnityEngine;
using Cwl.API.Processors;
using Microsoft.CodeAnalysis;
using HarmonyLib;
using BepInEx;
using BepInEx.Configuration;
using Cwl;
using Cwl.API.Custom;
using Cwl.LangMod;
using Cwl.Helper;
using Newtonsoft.Json;
using DG.Tweening.Plugins;

namespace Trait_Chest
{
    public class TraitFNBag : TraitMagicChest
    {
        public override int Electricity => 0;               // 不需要电力 / No electricity required / 電力不要
        public override bool IsHomeItem => false;           // 非家居物品 / Not a home item / 家庭用アイテムではない
        public override bool IsSpecialContainer => true;    // 特殊容器 / Special container / 特殊コンテナ
        public override bool CanBeOnlyBuiltInHome => false; // 不限于家中建造 / Can be built outside home / 家の中だけで建造可能ではない
        public override bool CanOpenContainer => true;      // 可打开容器 / Container can be opened / コンテナを開けることができる
        public override bool IsFridge => true;              // 具有冰箱功能 / Has fridge functionality / 冷蔵庫機能あり
        public override bool UseAltTiles => false;          // 不使用替代图块 / Don't use alternative tiles / 代替タイルを使用しない
        public override int DecaySpeedChild => 0;           // 子物品无腐坏速度 / No decay speed for child items / 子アイテムの腐敗速度なし
        public override bool CanSearchContent => true;      // 可搜索内容 / Content can be searched / 内容を検索可能

        public override void SetName(ref string s)
        {
            // 名称设置方法（当前为空实现） / Name setting method (currently empty implementation) / 名前設定メソッド（現在は空実装）
        }
    }
}
