using HalconDotNet;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using VisualSorting.Common;
using VisualSorting.ViewModels;

namespace VisualSorting.Views
{
   
    //YOLO检测界面视图
  
    public partial class YoloDetectionView : UserControl
    {
        /// <summary>ViewModel实例引用，用于事件订阅和取消订阅</summary>
        private YoloDetectionViewModel _vm;

        
        public YoloDetectionView()
        {
            InitializeComponent();
            _vm = new YoloDetectionViewModel();
            DataContext = _vm;
            this.Loaded += YoloDetectionView_Loaded;
            this.Unloaded += YoloDetectionView_Unloaded;
        }

      
        private void YoloDetectionView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_vm != null)
            {
                _vm.PropertyChanged -= OnPropertyChanged;
                _vm.FirstImageReceived -= OnFirstImageReceived;
                _vm.Dispose();
                _vm = null;
            }
        }

      
        private void YoloDetectionView_Loaded(object sender, RoutedEventArgs e)
        {
            if (_vm != null)
            {
                _vm.PropertyChanged += OnPropertyChanged;
                _vm.FirstImageReceived += OnFirstImageReceived;
            }

            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                parentWindow.WindowState = WindowState.Maximized;
            }
        }

       
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(YoloDetectionViewModel.CurrentImage) && sender is YoloDetectionViewModel vm)
            {
                if (vm.CurrentImage != null && vm.CurrentImage.IsInitialized())
                {
                    Dispatcher.Invoke(() =>
                    {
                        HOperatorSet.DispObj(vm.CurrentImage, Hsmart.HalconWindow);
                    });
                }
            }
        }

      
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

        #region 导航按钮事件处理
      
        private void BtnMainView_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataContext is YoloDetectionViewModel vm)
                {
                    vm.AddLog("INFO", "导航：切换到主界面");
                }

                AppData.Container.Content = new MainView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"切换到主界面失败：{ex.Message}");
            }
        }

       
        private void BtnYoloDetection_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is YoloDetectionViewModel vm)
            {
                vm.AddLog("INFO", "导航：当前已在YOLO检测界面");
            }
        }

      
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataContext is YoloDetectionViewModel vm)
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
