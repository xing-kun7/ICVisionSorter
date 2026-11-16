using Leadshine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualSorting.Services
{
    /// <summary>
    /// 电磁阀服务实现类
    /// 封装雷赛运动控制卡的数字输出功能
    /// 用于控制气缸剔除不合格芯片
    /// </summary>
    public class SolenoidValueService : ISolenoidValueService
    {
        /// <summary>
        /// 打开电磁阀
        /// 调用雷赛API设置数字输出口为低电平（0）
        /// 低电平触发气缸动作，剔除芯片
        /// </summary>
        /// <returns>true=操作成功，false=操作失败</returns>
        public bool OpenValue()
        {
            // smc_write_outbit参数：卡号(0), 输出口(2), 电平值(0=低电平)
            int result = LTSMC.smc_write_outbit(0, 2, 0);
            return result == 0;
        }

        /// <summary>
        /// 关闭电磁阀
        /// 调用雷赛API设置数字输出口为高电平（1）
        /// 高电平复位气缸，准备下一次剔除
        /// </summary>
        /// <returns>true=操作成功，false=操作失败</returns>
        public bool CloseValue()
        {
            // smc_write_outbit参数：卡号(0), 输出口(2), 电平值(1=高电平)
            int result = LTSMC.smc_write_outbit(0, 2, 1);
            return result == 0;
        }
    }
}
