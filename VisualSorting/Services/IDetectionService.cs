using HalconDotNet;
using VisualSorting.Models;

namespace VisualSorting.Services
{
    /// <summary>
    /// 视觉检测服务接口
    /// 负责模板匹配、针脚分割、合格判定等核心检测逻辑
    /// </summary>
    public interface IDetectionService
    {
        /// <summary>设置检测所需的模板和检测区域</summary>
        void SetTemplate(HTuple modelID, HObject modelContours, HObject checkRectXid, HObject checkRectXid2);

        /// <summary>对输入图像执行检测，返回检测结果</summary>
        DetectionResult Process(HObject ho_Image);

        /// <summary>重置触发状态（上升沿/下降沿）</summary>
        void ResetTriggerState();

        /// <summary>
        /// 设置针脚数合格范围
        /// 默认要求两个检测区域各恰好4个针脚才算合格
        /// 可通过此方法放宽判定条件，例如设为3-5允许一定误差
        /// </summary>
        void SetPinCountRange(int pinCountMin, int pinCountMax, int pinCount2Min, int pinCount2Max);
    }
}
