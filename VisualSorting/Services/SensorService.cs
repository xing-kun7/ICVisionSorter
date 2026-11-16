using Leadshine;

namespace VisualSorting.Services
{
    /// <summary>
    /// 传感器服务实现
    /// 通过运动控制卡的IO输入端口读取光电传感器状态
    /// 传感器安装在传送带旁，当芯片经过时触发信号
    /// </summary>
    public class SensorService : ISensorService
    {
        /// <summary>
        /// 读取传感器状态
        /// 读取控制卡0的输入端口0（IN0）
        /// </summary>
        /// <returns>true=芯片经过（传感器触发），false=无芯片（传感器常态）</returns>
        public bool ReadSensorState()
        {
            int bitio0 = LTSMC.smc_read_inbit(0, 0);
            return bitio0 == 0;
        }
    }
}
