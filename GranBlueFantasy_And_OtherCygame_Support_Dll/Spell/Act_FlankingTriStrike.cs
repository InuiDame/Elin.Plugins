namespace GBF.spell.Act_FlankingTriStrike
{
    public class ActFlankingTriStrike : Ability
    {
        /// <summary>
        /// チェック是否可以执行技能 / Check if the ability can be performed / 检查是否可以执行技能
        /// </summary>
        /// <returns>実行可能かどうか / Whether executable / 是否可执行</returns>
        public override bool CanPerform()
        {
            if (Act.TC == null)
            {
                return false;
            }
            return ACT.Melee.CanPerform();
        }

        /// <summary>
        /// 三重攻撃を実行 / Perform triple strike / 执行三重攻击
        /// </summary>
        /// <returns>常にtrueを返す / Always returns true / 总是返回true</returns>
        public override bool Perform()
        {
            int num = 0;
            Card orgTC = Act.TC; // 元のターゲットカード / Original target card / 原始目标卡片
            int num2 = 3;        // 固定で3回攻撃 / Fixed 3 attacks / 固定攻击3次
        
            for (int i = 0; i < num2; i++)
            {
                // 生存チェック / Survival check / 生存检查
                if (!Act.CC.IsAliveInCurrentZone || !orgTC.IsAliveInCurrentZone)
                {
                    break;
                }
            
                // エフェクト制御 / Effect control / 特效控制
                bool anime = i % 4 == 0;
                TweenUtil.Delay((float)num * 0.07f, delegate
                {
                    if (anime)
                    {
                        orgTC.pos.PlayEffect("ab_bladestorm"); // 剣刃嵐エフェクト / Blade storm effect / 剑刃风暴特效
                    }
                    orgTC.pos.PlaySound((base.source.id == 6665) ? "ab_shred" : "ab_swarm"); // サウンド再生 / Play sound / 播放音效
                });
                num++;
            
                // 攻撃実行 / Execute attack / 执行攻击
                new ActMeleeBladeStorm().Perform(Act.CC, orgTC);
            }
            return true;
        }
    }
}
