using HalconDotNet;
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
using VisualSorting.ViewModels;

namespace VisualSorting.Views
{
    
    //主界面视图
  
    public partial class MainView : UserControl
    {
        private MainViewModel _vm;

        public MainView()
        {
            InitializeComponent();
            _vm = new MainViewModel();
            DataContext = _vm;
            this.Loaded += MainView_Loaded;
            this.Unloaded += MainView_Unloaded;
        }

        private void MainView_Unloaded(object sender, RoutedEventArgs e)
        {
            // 页面卸载时取消事件订阅，防止ViewModel被View事件引用无法GC
            if (_vm != null)
            {
                _vm.RequestDrawTemplate -= OnRequestDrawTemplate;
                _vm.RequestDrawCheckRectXid -= OnRequestDrawCheckRectXid;
                _vm.RequestDrawCheckRectXid2 -= OnRequestDrawCheckRectXid2;
                _vm.PropertyChanged -= OnPropertyChanged;
                _vm.FirstImageReceived -= OnFirstImageReceived;
                _vm.Dispose();
                _vm = null;
            }
        }

        
        // 页面加载完成事件处理
     
        private void MainView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // 页面加载完成后，再加载数据、开启自动刷新
            if (this.DataContext is MainViewModel vm)
            {
                vm.RefreshChartData();

                #region 视觉模块事件订阅
                // 订阅绘制事件：由ViewModel触发，View执行Halcon绘制操作
                vm.RequestDrawTemplate += OnRequestDrawTemplate;
                vm.RequestDrawCheckRectXid += OnRequestDrawCheckRectXid;
                vm.RequestDrawCheckRectXid2 += OnRequestDrawCheckRectXid2;

                // 订阅属性变化：监听CurrentImage变化，自动更新显示
                vm.PropertyChanged += OnPropertyChanged;

                // 订阅第一次图像接收事件：设置Halcon窗口显示区域
                vm.FirstImageReceived += OnFirstImageReceived;
                #endregion
            }

            // 设置窗口最大化
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                parentWindow.WindowState = WindowState.Maximized;
            }
        }

        #region 视觉模块事件处理
     
        // 属性变化事件处理
      
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.CurrentImage) && sender is MainViewModel vm)
            {
                if (vm.CurrentImage != null && vm.CurrentImage.IsInitialized())
                {
                    // 图像可能在后台线程处理，需切换到UI线程显示
                    Dispatcher.Invoke(() =>
                    {
                        HOperatorSet.DispObj(vm.CurrentImage, Hsmart.HalconWindow);
                        
                    });
                }
            }
        }

       
        //第一次图像接收事件处理
        //设置Halcon窗口显示区域，确保显示完整图像
        // SetPart参数：行起点、列起点、行终点、列终点
       
        private void OnFirstImageReceived(object sender, ImageSizeEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (Hsmart.HalconWindow != null && Hsmart.HalconWindow.IsInitialized())
                {
                    HOperatorSet.SetPart(Hsmart.HalconWindow, 0, 0, e.Height - 1, e.Width - 1);
                }
            });
        }

        
        // 绘制模板区域事件处理
        // 使用DrawRectangle1绘制轴对齐矩形
        // 绘制结果传递给ViewModel处理
        private void OnRequestDrawTemplate(object sender, EventArgs e)
        {
            if (Hsmart.HalconWindow == null || Hsmart.HalconWindow.IsInitialized() == false)
            {
                MessageBox.Show("Halcon窗口未初始化");
                return;
            }

            Hsmart.Focus();
            Hsmart.UpdateLayout();//这两个的作用是让窗口获得焦点
            //作用：强制让UI线程处理完所有渲染消息，确保窗口已经完全显示，再让用户绘制矩形，避免窗口还没渲染完成导致 DrawRectangle1 卡住
           // 写法原理：用 DispatcherPriority.Render 优先级，让空操作插队执行，强制刷新UI。
            Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Render);

            try
            {
                HOperatorSet.DispCross(Hsmart.HalconWindow, 100, 100, 20, 0);
                MessageBox.Show("请在图像窗口绘制矩形区域作为模板");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"窗口无效：{ex.Message}");
                return;
            }

            try
            {
                // 调用Halcon绘制矩形方法
                // DrawRectangle1：绘制轴对齐矩形，返回左上角和右下角坐标
                HOperatorSet.DrawRectangle1(Hsmart.HalconWindow, out var hv_mRow1, out var hv_mColumn1, out var hv_mRow2, out var hv_mColumn2);

                if (DataContext is MainViewModel vm)
                {
                    vm.SetTemplateRegion(hv_mRow1, hv_mColumn1, hv_mRow2, hv_mColumn2);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"绘制模板失败：{ex.Message}");
            }
        }

        // 绘制第一个检测区域事件处理
        // 使用DrawRectangle2绘制旋转矩形
        // 用于检测芯片针脚区域
        private void OnRequestDrawCheckRectXid(object sender, EventArgs e)
        {
            if (Hsmart.HalconWindow == null || Hsmart.HalconWindow.IsInitialized() == false)
            {
                MessageBox.Show("Halcon窗口未初始化");
                return;
            }

            // 准备绘制环境
            Hsmart.Focus();
            Hsmart.UpdateLayout();
            Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Render);
            HOperatorSet.DispCross(Hsmart.HalconWindow, 100, 100, 20, 0);

            try
            {
                // DrawRectangle2：绘制旋转矩形
                // 返回中心坐标、旋转角度、半长和半宽
                HOperatorSet.DrawRectangle2(Hsmart.HalconWindow, out var hv_r2Row, out var hv_r2Column, out var hv_r2Phi, out var hv_r2Length1, out var hv_r2Length2);

                if (DataContext is MainViewModel vm)
                {
                    vm.SetCheckRectXid(hv_r2Row, hv_r2Column, hv_r2Phi, hv_r2Length1, hv_r2Length2);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"绘制检测区域1失败：{ex.Message}");
            }
        }

        
        //绘制第二个检测区域事件处理
        // 与第一个检测区域配合，检测芯片两侧针脚
       
        private void OnRequestDrawCheckRectXid2(object sender, EventArgs e)
        {
            try
            {
                HOperatorSet.DrawRectangle2(Hsmart.HalconWindow, out var hv_r3Row, out var hv_r3Column, out var hv_r3Phi, out var hv_r3Length1, out var hv_r3Length2);

                if (DataContext is MainViewModel vm)
                {
                    vm.SetCheckRectXid2(hv_r3Row, hv_r3Column, hv_r3Phi, hv_r3Length1, hv_r3Length2);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"绘制检测区域2失败：{ex.Message}");
            }
        }
        #endregion

        #region 导航按钮事件处理
       
        //主界面按钮点击事件
      
        private void BtnMainView_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.AddLog("INFO", "导航：当前已在主界面");
            }
        }

        
       
        // 切换到YOLO检测界面
        
        private void BtnYoloDetection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataContext is MainViewModel vm)
                {
                    vm.AddLog("INFO", "导航：切换到YOLO检测界面");
                }

                AppData.Container.Content = new YoloDetectionView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"切换到YOLO检测界面失败：{ex.Message}");
            }
        }

        
        //退出登录按钮点击事件
      
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataContext is MainViewModel vm)
                {
                    vm.AddLog("INFO", "导航：退出登录，返回登录界面");
                }

                var loginView = new Login();
                AppData.Container.Content = loginView;

                Window parentWindow = Window.GetWindow(this);
                if (parentWindow != null)
                {
                    parentWindow.WindowState = WindowState.Normal;
                    parentWindow.Width = 600;
                    parentWindow.Height = 650;
                    parentWindow.Left = (SystemParameters.WorkArea.Width - parentWindow.Width) / 2;
                    parentWindow.Top = (SystemParameters.WorkArea.Height - parentWindow.Height) / 2;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"退出登录失败：{ex.Message}");
            }
        }
        #endregion
    }
}
