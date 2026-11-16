using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HalconDotNet;

namespace VisualSorting.Services
{
    /// <summary>
    /// 模板服务接口
    /// 定义创建和管理形状模板的标准契约
    /// 实现类：TemplateService
    /// 用于芯片模板的创建和管理
    /// </summary>
    public interface ITemplateService
    {
        /// <summary>
        /// 模板是否已创建
        /// 用于判断是否可以启动检测
        /// </summary>
        bool IsTemplateCreated { get; }

        /// <summary>
        /// 模板区域（矩形ROI）
        /// 用于界面显示模板位置
        /// </summary>
        HObject TemplateRegion { get; }

        /// <summary>
        /// 形状模板ID
        /// Halcon模板匹配的核心标识，由CreateShapeModel生成
        /// </summary>
        HTuple ModelID { get; }

        /// <summary>
        /// 模板轮廓
        /// 从模板图像提取的边缘轮廓，用于可视化显示
        /// </summary>
        HObject ModelContours { get; }

        /// <summary>
        /// 模板区域的XLD轮廓
        /// 用于界面绘制和显示
        /// </summary>
        HObject TemplateRegionXld { get; set; }

        /// <summary>
        /// 根据图像和矩形区域创建形状模板
        /// 核心方法，用于生成可匹配的模板
        /// </summary>
        /// <param name="image">模板图像（通常是第一帧采集的图像）</param>
        /// <param name="hv_mRow1">矩形区域左上角行坐标</param>
        /// <param name="hv_mColumn1">矩形区域左上角列坐标</param>
        /// <param name="hv_mRow2">矩形区域右下角行坐标</param>
        /// <param name="hv_mColumn2">矩形区域右下角列坐标</param>
        void CreateTemplate(HObject image, double hv_mRow1, double hv_mColumn1, double hv_mRow2, double hv_mColumn2);

        /// <summary>
        /// 设置模板区域（不创建模板）
        /// 仅用于界面显示模板区域位置
        /// </summary>
        void SetTemplateRegion(double hv_mRow1, double hv_mColumn1, double hv_mRow2, double hv_mColumn2);

        /// <summary>
        /// 清除模板资源
        /// 释放Halcon对象，避免内存泄漏
        /// 切换产品或关闭程序时调用
        /// </summary>
        void ClearTemplate();
    }
}
