namespace VisualSorting.Services
{
    /// <summary>
    /// 传感器服务接口
    /// 封装对光电传感器的读取操作
    /// 传感器安装在传送带旁，用于检测芯片是否经过
    /// </summary>
    public interface ISensorService
    {
        /// <summary>
        /// 读取传感器状态
        /// </summary>
        /// <returns>true=传感器被触发（芯片经过），false=传感器常态（无芯片）</returns>
        bool ReadSensorState();
    }
}
