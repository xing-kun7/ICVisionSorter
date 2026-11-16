using System;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.Extensions.DependencyInjection;
using VisualSorting.Models;

namespace VisualSorting.Common
{
    /// <summary>
    /// 日志消息类型
    /// 使用CommunityToolkit.Mvvm的ValueChangedMessage封装LogEntry
    /// 通过WeakReferenceMessenger在LogService和ViewModel之间传递日志数据
    /// </summary>
    public class LogMessage : ValueChangedMessage<LogEntry>
    {
        public LogMessage(LogEntry value) : base(value) { }
    }

    /// <summary>
    /// 图像尺寸事件参数
    /// 当相机首次采集到图像时，通过此事件通知View设置HalconWindow的显示区域尺寸
    /// </summary>
    public class ImageSizeEventArgs : EventArgs
    {
        /// <summary>
        /// 图像宽度（像素）
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// 图像高度（像素）
        /// </summary>
        public int Height { get; set; }
    }

    /// <summary>
    /// 服务定位器
    /// 封装IServiceProvider，提供全局的服务获取入口
    /// 
    /// 设计说明：
    /// - 内部委托给Microsoft.Extensions.DependencyInjection的IServiceProvider
    /// - 服务在App.ConfigureServices()中集中注册
    /// - ViewModel通过ServiceLocator.GetService<T>()获取所需服务
    /// - 同时提供WeakReferenceMessenger访问入口，用于发布/订阅消息
    /// </summary>
//核心职责： 提供 全局、便捷的服务获取方式
//    它做什么：
//1. 持有 IServiceProvider 引用 （由 App.xaml.cs 注入）
//2. 提供静态方法 GetService <t>()</t> （全局获取服务）
//3. 提供全局 Messenger （跨模块消息通信）
    public static class ServiceLocator
    {
        /// <summary>
        /// 全局消息信使
        /// 使用弱引用模式，不会阻止订阅者被垃圾回收
        /// 用于跨ViewModel/Service的松耦合通信
        /// </summary>
        public static IMessenger Messenger => WeakReferenceMessenger.Default;

        private static IServiceProvider _provider;

        /// <summary>
        /// 初始化服务定位器
        /// 在App构造函数中调用，将DI容器注入
        /// </summary>
        /// <param name="provider">由ServiceCollection构建的IServiceProvider实例</param>
        public static void Initialize(IServiceProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// 获取指定类型的服务实例
        /// </summary>
        /// <typeparam name="T">服务接口类型</typeparam>
        /// <returns>已注册的服务实例</returns>
        /// <exception cref="InvalidOperationException">服务定位器未初始化或服务未注册</exception>
        public static T GetService<T>()
        {
            if (_provider == null)
            {
                throw new InvalidOperationException("ServiceLocator has not been initialized. Call Initialize() first.");
            }
            return _provider.GetRequiredService<T>();
        }
    }
}
