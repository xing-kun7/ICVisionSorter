namespace VisualSorting.Models
{
    /// <summary>
    /// IC 芯片引脚缺陷类型枚举
    /// </summary>
    public enum PinDefectType
    {
        /// <summary>
        /// 无缺陷 / 合格产品
        /// </summary>
        None = 0,

        /// <summary>
        /// 缺脚 (少针脚，连通域计数小于标准值且无粘连)
        /// </summary>
        MissingPin = 1,

        /// <summary>
        /// 连锡 / 短路 (相邻针脚锡膏桥接粘连，连通域面积偏大且计数减少)
        /// </summary>
        SolderBridge = 2,

        /// <summary>
        /// 歪脚 (针脚弯曲扭曲，几何长宽比或角度超差)
        /// </summary>
        BentPin = 3,

        /// <summary>
        /// 光照异常 (亮度均值过暗 &lt;35 或过曝眩光 &gt;220)
        /// </summary>
        LightAbnormal = 4,

        /// <summary>
        /// 模板匹配失败 (未在视野中找到芯片)
        /// </summary>
        MatchFailed = 5,

        /// <summary>
        /// 图像或算法处理异常
        /// </summary>
        Exception = 6
    }
}
