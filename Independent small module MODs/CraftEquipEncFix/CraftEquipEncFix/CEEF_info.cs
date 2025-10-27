using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using HarmonyLib;

namespace CEEF_info
{
    [BepInPlugin("inui.noadashabi.CEEF.info", "CraftEquipEncFix", "1.0.0")]
    public class GBF_and_PCR_Equipment : BaseUnityPlugin
    {

        private void Start()
        {
            new Harmony("CraftEquipEncFix").PatchAll();
            base.Logger.LogInfo("附魔修复已经激活");
        }
    }
}
