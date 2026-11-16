using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VisualSorting.Common;
using VisualSorting.Repositories;
using VisualSorting.Services;
using VisualSorting.ViewModels;
using VisualSorting.Views;

namespace VisualSorting
{
    /// <summary>
    /// 主窗口，承载登录页和主界面
    /// 使用MetroWindow实现无边框全屏效果
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            Loaded += MainWindow_Loaded;
            AppData.MainWindow = this;
            AppData.Container = this;
            this.WindowStyle = WindowStyle.None;
            this.Topmost = true;
        }

        /// <summary>
        /// 窗口加载完成后显示登录界面
        /// </summary>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UserControl view = new Login();
            container.Content = view;
        }

        /// <summary>
        /// 窗口关闭时的资源释放
        /// 按顺序释放：停止检测→停止相机→释放ViewModel→释放CameraService
        /// 防止退出时因后台线程未停止而卡死
        /// </summary>
        protected override void OnClosing(CancelEventArgs e)
        {
            try
            {
                // 从主界面获取ViewModel并释放资源
                if (container.Content is MainView mainView)
                {
                    if (mainView.DataContext is MainViewModel vm)
                    {
                        vm.StopLive();
                        vm.StopAutoRefresh();
                        vm.Dispose();
                    }
                }

                // 释放相机服务资源（停止采集、关闭相机）
                var cameraService = ServiceLocator.GetService<ICameraService>();
                if (cameraService != null)
                {
                    cameraService.StopGrabbing();
                    cameraService.Close();
                    (cameraService as IDisposable)?.Dispose();
                }
            }
            catch { }

            base.OnClosing(e);
        }

        /// <summary>
        /// 窗口关闭后强制退出应用程序
        /// 确保所有后台线程都被终止
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }
    }
}