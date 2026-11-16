using Leadshine;

namespace VisualSorting.Services
{
    /// <summary>
    /// 运动控制服务实现
    /// 通过Leadshine LTSMC运动控制卡DLL，实现对传送带电机的控制
    /// 包括连接控制卡、电机使能、传送带启停、速度调节
    /// </summary>
    public class MotionControlService : IMotionControlService
    {
        /// <summary>
        /// 连接运动控制卡
        /// 使用以太网连接，IP:192.168.5.11，端口:115200
        /// </summary>
        public bool Connect()
        {
            int res = LTSMC.smc_board_init(0, 2, "192.168.5.11", 115200);
            return res == 0;
        }

        /// <summary>
        /// 电机使能（伺服使能）
        /// 使能后电机才能接受运动指令
        /// </summary>
        public bool Sevon()
        {
            int EN = LTSMC.smc_write_sevon_pin(0, 0, 1);
            return EN == 0;
        }

        /// <summary>
        /// 启动传送带连续运动
        /// 方向=0（正向），传送带将持续运动直到调用Vstop()
        /// </summary>
        public bool Vmove()
        {
            int MV = LTSMC.smc_vmove(0, 0, 0);
            return MV == 0;
        }

        /// <summary>
        /// 停止传送带运动
        /// </summary>
        public bool Vstop()
        {
            int a = LTSMC.smc_stop(0, 0, 0);
            return a == 0;
        }

        /// <summary>
        /// 调整传送带运行速度
        /// 设置运动参数：起始速度3000，目标速度为targetSpeed
        /// 需要重启传送带后新速度才生效
        /// </summary>
        public bool ChangeSpeed(double targetSpeed)
        {
            int CH = LTSMC.smc_set_profile_unit(0, 0, 3000, targetSpeed, 0, 0, 0);
            LastErrorCode = (short)CH;
            return CH == 0;
        }

        /// <summary>
        /// 最近一次操作的错误码
        /// 0=成功，非0=失败（具体含义参考LTSMC文档）
        /// </summary>
        public short LastErrorCode { get; private set; }
    }
}
