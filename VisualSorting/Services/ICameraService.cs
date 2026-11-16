using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualSorting.Services
{
    /// <summary>
    /// 相机服务接口
    /// 定义相机操作的标准契约，支持依赖注入和单元测试
    /// 实现类：CameraService
    /// </summary>
    public interface ICameraService
    {
        /// <summary>
        /// 相机是否已打开
        /// 用于界面状态显示和操作权限控制
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// 是否正在采集图像
        /// 区分"预览模式"和"检测模式"
        /// </summary>
        bool IsGrabbing { get; }

        /// <summary>
        /// 当前图像对象
        /// 用于界面实时显示
        /// </summary>
        object CurrentImage { get; }

        /// <summary>
        /// 图像采集完成事件
        /// 每采集一帧图像触发一次，参数为HObject类型的图像对象
        /// 订阅者：MainViewModel.OnImageGrabbed
        /// </summary>
        event EventHandler<HObject> ImageGrabbed;

        /// <summary>
        /// 打开相机设备
        /// 初始化相机参数，准备采集
        /// </summary>
        void Open();

        /// <summary>
        /// 关闭相机设备
        /// 释放相机资源
        /// </summary>
        void Close();

        /// <summary>
        /// 开始连续采集图像
        /// 触发ImageGrabbed事件持续输出图像帧
        /// </summary>
        void StartGrabbing();

        /// <summary>
        /// 停止采集图像
        /// 停止触发ImageGrabbed事件
        /// </summary>
        void StopGrabbing();
    }
}
