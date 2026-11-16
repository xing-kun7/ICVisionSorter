using System;
using System.Diagnostics;

namespace VisualSorting.Services
{
    /// <summary>
    /// 软件在环仿真：无运控卡时替代 LTSMC，运动、传感器、电磁阀全部在软件内闭环
    /// </summary>
    public class SimMotionControlService : IMotionControlService
    {
        public short LastErrorCode => 0;
        public bool Connect() => true;
        public bool Sevon() => true;
        public bool Vmove() => true;
        public bool Vstop() => true;
        public bool ChangeSpeed(double targetSpeed) => true;
    }

    /// <summary>仿真光电传感器：每个周期内输出一段高电平，产生上升沿模拟芯片经过</summary>
    public class SimSensorService : ISensorService
    {
        private readonly Stopwatch _clock = Stopwatch.StartNew();
        private readonly int _periodMs;
        private readonly int _pulseMs;

        public SimSensorService() : this(6000, 300) { }

        public SimSensorService(int periodMs, int pulseMs)
        {
            _periodMs = periodMs;
            _pulseMs = pulseMs;
        }

        public bool ReadSensorState() => _clock.ElapsedMilliseconds % _periodMs < _pulseMs;
    }

    /// <summary>仿真电磁阀：只记录动作</summary>
    public class SimSolenoidValueService : ISolenoidValueService
    {
        public bool IsOpen { get; private set; }

        public bool OpenValue()
        {
            IsOpen = true;
            Debug.WriteLine("[SIM] 电磁阀打开 (NG 剔除)");
            return true;
        }

        public bool CloseValue()
        {
            IsOpen = false;
            Debug.WriteLine("[SIM] 电磁阀关闭");
            return true;
        }
    }
}
