using System;
using CommunityToolkit.Mvvm.Messaging;
using NLog;
using VisualSorting.Common;
using VisualSorting.Models;

namespace VisualSorting.Services
{
    /// <summary>
    /// 日志服务实现
    /// 统一的日志记录入口，同时完成两个功能：
    /// 1. 通过WeakReferenceMessenger发布LogMessage消息，ViewModel订阅后显示在界面日志列表
    /// 2. 通过NLog将日志写入本地文件，实现持久化存储
    /// </summary>
    public class LogService : ILogService
    {
        //这是 NLog 日志框架中，获取当前类日志实例的标准写法：
       //• LogManager.GetCurrentClassLogger()：自动根据当前类（LogService）的全名，
       //创建一个对应的 Logger 对象，后续写日志时会自动带上类名，方便排查问题。

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 添加一条日志记录
        /// 步骤：创建LogEntry → 发布LogMessage → NLog写入文件
        /// </summary>
        /// <param name="level">日志级别：INFO/WARN/ERROR/DEBUG</param>
        /// <param name="message">日志消息内容</param>
        public void AddLog(string level, string message)
        {
            try
            {
                var logEntry = new LogEntry
                {
                    Timestamp = DateTime.Now,
                    Level = level,
                    Message = message
                };

                WeakReferenceMessenger.Default.Send(new LogMessage(logEntry));

                switch (level)
                {
                    case "INFO":
                        _logger.Info(message);
                        break;
                    case "WARN":
                        _logger.Warn(message);
                        break;
                    case "ERROR":
                        _logger.Error(message);
                        break;
                    case "DEBUG":
                        _logger.Debug(message);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "添加日志失败");
            }
        }
    }
}
