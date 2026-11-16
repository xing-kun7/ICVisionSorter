namespace VisualSorting.Services
{
    /// <summary>
    /// 日志服务接口
    /// 提供统一的日志记录入口，同时完成两个功能：
    /// 1. 通过WeakReferenceMessenger发布日志消息到ViewModel（界面显示）
    /// 2. 通过NLog将日志写入文件（持久化存储）
    /// </summary>
    public interface ILogService
    {
        /// <summary>
        /// 添加一条日志记录
        /// </summary>
        /// <param name="level">日志级别：INFO/WARN/ERROR/DEBUG</param>
        /// <param name="message">日志消息内容</param>
        void AddLog(string level, string message);
    }
}
