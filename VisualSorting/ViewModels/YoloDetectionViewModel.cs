using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HalconDotNet;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VisualSorting.Common;
using VisualSorting.Models;
using VisualSorting.Services;

namespace VisualSorting.ViewModels
{
   
    // YOLO检测视图模型
   
    public partial class YoloDetectionViewModel : ObservableObject, IDisposable
    {
     
        private readonly ILogService _logService;
        private readonly ICameraService _cameraService;
        private ObservableCollection<LogEntry> _logEntries = new ObservableCollection<LogEntry>();
       
        public ObservableCollection<LogEntry> LogEntries
        {
            get => _logEntries;
            set => SetProperty(ref _logEntries, value);
        }

      
        private const int MaxLogCount = 100;
        public void AddLog(string level, string message)
        {
            _logService.AddLog(level, message);
        }

        /// <summary>当前显示图像，绑定到Halcon窗口控件，设置时自动释放旧图像</summary>
        private HObject _currentImage;
       
        public HObject CurrentImage
        {
            get => _currentImage;
            set
            {
                if (_currentImage != value)
                {
                    _currentImage?.Dispose();
                    SetProperty(ref _currentImage, value);
                }
            }
        }

      
        private bool _isCameraOpen;
       
        public bool IsCameraOpen
        {
            get => _isCameraOpen;
            set => SetProperty(ref _isCameraOpen, value);
        }

       
        private string _detectionStatus = "等待检测";
       
        public string DetectionStatus
        {
            get => _detectionStatus;
            set => SetProperty(ref _detectionStatus, value);
        }

     
        private bool _isFirstImage = true;

       
        public event EventHandler<ImageSizeEventArgs> FirstImageReceived;

      
        public ICommand OpenCameraCommand { get; }
    
        public ICommand CloseCameraCommand { get; }
       
        public ICommand StartDetectionCommand { get; }
      
        public ICommand StopDetectionCommand { get; }

      
        public YoloDetectionViewModel()
        {
            _logService = ServiceLocator.GetService<ILogService>();
            _cameraService = ServiceLocator.GetService<ICameraService>();

            WeakReferenceMessenger.Default.Register<YoloDetectionViewModel, LogMessage>(this, (r, m) =>
            {
                if (System.Windows.Application.Current?.Dispatcher != null)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        LogEntries.Insert(0, m.Value);
                        while (LogEntries.Count > MaxLogCount)
                        {
                            LogEntries.RemoveAt(LogEntries.Count - 1);
                        }
                    });
                }
            });

            _cameraService.ImageGrabbed += OnImageGrabbed;

            OpenCameraCommand = new AsyncRelayCommand(OpenCameraAsync);
            CloseCameraCommand = new AsyncRelayCommand(CloseCameraAsync);
            StartDetectionCommand = new RelayCommand(StartDetection);
            StopDetectionCommand = new RelayCommand(StopDetection);

            AddLog("INFO", "========================================");
            AddLog("INFO", "YOLO检测模块初始化完成");
        }

      
        private async Task OpenCameraAsync()
        {
            if (IsCameraOpen)
            {
                AddLog("WARN", "相机已打开，无需重复操作");
                return;
            }
            AddLog("INFO", "正在打开相机...");
            try
            {
                await Task.Run(() => _cameraService.Open());
                AddLog("INFO", "相机设备打开成功");

                _cameraService.StartGrabbing();
                AddLog("INFO", "相机开始采集图像");

                _isFirstImage = true;
                IsCameraOpen = true;
                AddLog("INFO", "相机模块初始化完成，等待图像采集");
            }
            catch (Exception ex)
            {
                AddLog("ERROR", $"打开相机失败: {ex.Message}");
                MessageBox.Show($"打开相机失败：{ex.Message}");
            }
        }

        
        private async Task CloseCameraAsync()
        {
            if (!IsCameraOpen)
            {
                AddLog("WARN", "相机未打开，无需关闭");
                return;
            }
            AddLog("INFO", "正在关闭相机...");
            try
            {
                await Task.Run(() => _cameraService.StopGrabbing());
                await Task.Run(() => _cameraService.Close());
                IsCameraOpen = false;
                AddLog("INFO", "相机关闭成功");
            }
            catch (Exception ex)
            {
                AddLog("ERROR", $"关闭相机失败: {ex.Message}");
                MessageBox.Show($"关闭相机失败：{ex.Message}");
            }
        }

      
        private void StartDetection()
        {
            AddLog("INFO", "开始YOLO检测...");
            DetectionStatus = "正在检测";
        }

        
        private void StopDetection()
        {
            AddLog("INFO", "停止YOLO检测");
            DetectionStatus = "检测停止";
        }

       
        private void OnImageGrabbed(object sender, HObject ho_Image)
        {
            if (ho_Image == null || !ho_Image.IsInitialized())
            {
                return;
            }

            HObject ho_GrayImage = null;
            try
            {
                HOperatorSet.CountChannels(ho_Image, out HTuple channels);
                if (channels.I == 3)
                {
                    HOperatorSet.Rgb1ToGray(ho_Image, out ho_GrayImage);
                }
                else
                {
                    ho_GrayImage = ho_Image.Clone();
                }

                if (ho_GrayImage == null || !ho_GrayImage.IsInitialized())
                {
                    return;
                }

                if (_isFirstImage)
                {
                    _isFirstImage = false;
                    HOperatorSet.GetImageSize(ho_GrayImage, out HTuple width, out HTuple height);
                    FirstImageReceived?.Invoke(this, new ImageSizeEventArgs
                    {
                        Width = width.I,
                        Height = height.I
                    });
                    AddLog("INFO", $"首次接收图像，尺寸: {width.I} x {height.I}，已设置显示区域");
                }

                if (System.Windows.Application.Current?.Dispatcher != null)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        var oldImage = CurrentImage;
                        CurrentImage = ho_GrayImage.Clone();
                        oldImage?.Dispose();
                    });
                }
            }
            catch (Exception ex)
            {
                AddLog("ERROR", $"图像处理流程异常: {ex.Message}");
            }
            finally
            {
                ho_GrayImage?.Dispose();
            }
        }

        #region IDisposable
      
        private bool _disposed = false;

        public void Dispose()
        {
            if (_disposed) return;

            if (_cameraService != null)
            {
                _cameraService.ImageGrabbed -= OnImageGrabbed;
            }

            if (_currentImage != null && _currentImage.IsInitialized())
            {
                _currentImage.Dispose();
                _currentImage = null;
            }

            WeakReferenceMessenger.Default.Unregister<LogMessage>(this);

            _disposed = true;
        }
        #endregion
    }
}
