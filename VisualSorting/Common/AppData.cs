using System.Windows.Controls;

namespace VisualSorting.Common
{
    /// <summary>
    /// 应用全局数据容器
    /// 存储主窗口和内容控件的引用，供ViewModel在导航时使用
    /// 例如：LoginViewModel中切换到主界面时需要访问Container来替换页面内容
    /// </summary>
    public static class AppData
    {
        /// <summary>
        /// 主窗口实例引用
        /// 用于设置窗口居中位置等操作
        /// </summary>
        public static MainWindow MainWindow { get; set; }

        /// <summary>
        /// 主窗口中的内容控件（ContentControl）
        /// 用于在不同页面间切换（登录页→主页→检测页等）
        /// 通过修改Content属性实现页面导航
        /// </summary>
        public static ContentControl Container { get; set; }
    }
}
