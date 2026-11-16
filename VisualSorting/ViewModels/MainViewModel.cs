using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HalconDotNet;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VisualSorting.Common;
using VisualSorting.Entities;
using VisualSorting.Models;
using VisualSorting.Repositories;
using VisualSorting.Services;

namespace VisualSorting.ViewModels
{
  
    //主界面视图模型，IC芯片视觉分拣系统的核心控制器
    // 负责管理：运动控制、电磁阀、传感器分拣、相机采集、视觉检测、模板管理、折线图、日志等全部业务逻辑
    // 实现IDisposable接口，确保退出时释放所有资源（定时器、Halcon对象、事件订阅、数据库上下文）
    // 使用CommunityToolkit.Mvvm的ObservableObject实现属性变更通知
   
    public partial class MainViewModel : ObservableObject, IDisposable
    {
        
        // 安全地在UI线程执行操作，防止Application.Current为null时崩溃
        
        private static void InvokeOnUI(Action action)
        {
            if (System.Windows.Application.Current?.Dispatcher != null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(action);
            }
        }

        #region 日志模块
        /// <summary>日志服务，通过WeakReferenceMessenger发送日志消息</summary>
        private readonly ILogService _logService;

        /// <summary>日志条目集合，绑定到界面日志列表，最多保留MaxLogCount条</summary>
        private ObservableCollection<LogEntry> _logEntries = new ObservableCollection<LogEntry>();

        /// <summary>日志条目集合，界面绑定此属性显示日志列表</summary>
        public ObservableCollection<LogEntry> LogEntries
        {
            get => _logEntries;
            set => SetProperty(ref _logEntries, value);
        }

        /// <summary>日志最大保留条数，超出时移除最旧的</summary>
        private const int MaxLogCount = 100;

        /// <summary>添加日志条目，通过LogService发送消息，由WeakReferenceMessenger接收并更新UI</summary>
        public void AddLog(string level, string message)
        {
            _logService.AddLog(level, message);
        }
        #endregion

        #region 构造函数
        public MainViewModel()
        {
            _logService = ServiceLocator.GetService<ILogService>();

            WeakReferenceMessenger.Default.Register<MainViewModel, LogMessage>(this, (r, m) =>
            {
                InvokeOnUI(() =>
                {
                    LogEntries.Insert(0, m.Value);
                    while (LogEntries.Count > MaxLogCount)
                    {
                        LogEntries.RemoveAt(LogEntries.Count - 1);
                    }
                });
            });

            AddLog("INFO", "========================================");
            AddLog("INFO", "IC芯片分拣系统主界面初始化开始");

            #region 运动控制模块初始化
            var motionService = ServiceLocator.GetService<IMotionControlService>();
            _motionController = motionService;
            _move = motionService;
            _stop = motionService;
            _change = motionService;
            _sevon = motionService;

            
            // 运动控制卡操作（Connect/Sevon/Vmove等）涉及硬件通信，耗时不确定，必须异步执行
            ConnectCommand = new AsyncRelayCommand(OnConnectCommandAsync);
            SevonCommand = new AsyncRelayCommand(OnSevonCommandAsync);
            MoveCommand = new AsyncRelayCommand(OnMoveCommandAsync);
            StopCommand = new AsyncRelayCommand(OnStopCommandAsync);
            ChangedSpeedCommand = new AsyncRelayCommand(OnChangeSpeedCommandAsync);
            AddLog("DEBUG", "运动控制命令初始化完成");
            #endregion

            #region 电磁阀模块初始化
            _solenoidValueService = ServiceLocator.GetService<ISolenoidValueService>();
            
            OpenValveCommand = new AsyncRelayCommand(OpenValveAsync);
            CloseValveCommand = new AsyncRelayCommand(CloseValveAsync);
            #endregion

            #region 传感器模块初始化
            _sensorService = ServiceLocator.GetService<ISensorService>();
            InitSensorTimer();
            #endregion

            #region 系统状态模块初始化
            SystemStatus = new SystemStatusViewModel();
            #endregion

            #region 时间显示初始化
            InitTimeUpdateTimer();
            #endregion

            #region 数据库模块初始化
           
            _ = LoadHistoryRecordsAsync();//火并忘记"模式，不阻塞构造函数执行
            #endregion

            #region 视觉模块初始化
            _templateService = ServiceLocator.GetService<ITemplateService>();
            _detectionService = ServiceLocator.GetService<IDetectionService>();
            _cameraService = ServiceLocator.GetService<ICameraService>();
            _cameraService.ImageGrabbed += OnImageGrabbed;

          
            OpenCameraCommand = new AsyncRelayCommand(OpenCameraAsync);
            CloseCameraCommand = new AsyncRelayCommand(CloseCameraAsync);
            // DrawTemplate/DrawCheckRectXid/StartLive/StopLive 仅操作UI状态，不耗时，保持同步
            DrawTemplateCommand = new RelayCommand(DrawTemplate);
            DrawCheckRectXidCommand = new RelayCommand(DrawCheckRectXid);
            DrawCheckRectXid2Command = new RelayCommand(DrawCheckRectXid2);
            StartLiveCommand = new RelayCommand(StartLive);
            StopLiveCommand = new RelayCommand(StopLive);
            SaveTemplateCommand = new RelayCommand(SaveTemplate);
            LoadTemplateCommand = new RelayCommand(LoadTemplate);
            #endregion

            #region 折线图模块初始化
            _recordRepository = new ProductInspectionRecordRepository();
            YAxisFormatter = value => Math.Round(value).ToString();

            InspectionSeries = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "检测总数",
                    Values = new ChartValues<int>(),
                    StrokeThickness = 2,
                    PointGeometrySize = 4,
                    Fill = null,
                    Foreground = System.Windows.Media.Brushes.DimGray
                },
                new LineSeries
                {
                    Title = "不良品数",
                    Values = new ChartValues<int>(),
                    StrokeThickness = 2,
                    PointGeometrySize = 4,
                    Fill = null,
                    Foreground = System.Windows.Media.Brushes.DimGray
                }
            };
            #endregion

            #region 检测配置初始化
            _config = new InspectionConfig();
            #endregion
        }
        #endregion

        #region 传感器轮询定时器
        private System.Timers.Timer _sensorCheckTimer;

        private void InitSensorTimer()
        {
            _sensorCheckTimer?.Stop();
            _sensorCheckTimer?.Dispose();

            _sensorCheckTimer = new System.Timers.Timer(10);//10ms轮询一次
            _sensorCheckTimer.Elapsed += Timer_Elapsed;
            _sensorCheckTimer.AutoReset = true;
            _sensorCheckTimer.Start();
        }
        #endregion

        #region 系统状态模块
        private SystemStatusViewModel _systemStatus;

        public SystemStatusViewModel SystemStatus
        {
            get => _systemStatus;
            set => SetProperty(ref _systemStatus, value);
        }
        #endregion

        #region 时间显示模块
        private DateTime _currentTime = DateTime.Now;

        public DateTime CurrentTime
        {
            get => _currentTime;
            set => SetProperty(ref _currentTime, value);
        }

        private System.Timers.Timer _timeUpdateTimer;

        private void InitTimeUpdateTimer()
        {
            _timeUpdateTimer = new System.Timers.Timer(1000);
            _timeUpdateTimer.Elapsed += (s, e) =>
            {
                InvokeOnUI(() =>
                {
                    CurrentTime = DateTime.Now;
                });
            };
            _timeUpdateTimer.AutoReset = true;
            _timeUpdateTimer.Start();
        }
        #endregion

        #region 运动控制模块
        /// <summary>电机使能服务引用</summary>
        private readonly IMotionControlService _sevon;
        /// <summary>传送带运动服务引用</summary>
        private readonly IMotionControlService _move;
        /// <summary>运动控制卡连接服务引用</summary>
        private readonly IMotionControlService _motionController;
        /// <summary>传送带停止服务引用</summary>
        private readonly IMotionControlService _stop;
        /// <summary>传送带调速服务引用</summary>
        private readonly IMotionControlService _change;

        /// <summary>控制卡连接状态文本，绑定到设备状态LED灯</summary>
        private string _connectStatus = "未连接控制卡";
        /// <summary>电机使能状态文本，绑定到设备状态LED灯</summary>
        private string _sevonStatus = "电机未使能";
        /// <summary>传送带运动状态文本，绑定到设备状态LED灯</summary>
        private string _moveStatus = "传送带停止";
        /// <summary>目标速度值，绑定到速度输入框</summary>
        private double _targetSpeed;

        /// <summary>控制卡连接状态，界面绑定显示</summary>
        public string ConnectStatus
        {
            get => _connectStatus;
            set => SetProperty(ref _connectStatus, value);
        }
        /// <summary>电机使能状态，界面绑定显示</summary>
        public string SevonStatus
        {
            get => _sevonStatus;
            set => SetProperty(ref _sevonStatus, value);
        }
        /// <summary>传送带运动状态，界面绑定显示</summary>
        public string MoveStatus
        {
            get => _moveStatus;
            set => SetProperty(ref _moveStatus, value);
        }
        /// <summary>目标速度值，界面绑定到速度输入框</summary>
        public double TargetSpeed
        {
            get => _targetSpeed;
            set => SetProperty(ref _targetSpeed, value);
        }

        /// <summary>连接控制卡命令，XAML绑定到"连接控制卡"按钮</summary>
        public ICommand ConnectCommand { get; }
        /// <summary>电机使能命令，XAML绑定到"电机使能"按钮</summary>
        public ICommand SevonCommand { get; }
        /// <summary>启动传送带命令，XAML绑定到"开启传送带"按钮</summary>
        public ICommand MoveCommand { get; }
        /// <summary>停止传送带命令，XAML绑定到"关闭传送带"按钮</summary>
        public ICommand StopCommand { get; }
        /// <summary>调速命令，XAML绑定到"执行调速"按钮</summary>
        public ICommand ChangedSpeedCommand { get; }

      
        // 连接运动控制卡命令
        // 异步执行连接操作，防止UI卡死
    
        private async Task OnConnectCommandAsync()
        {
            AddLog("INFO", "尝试连接运动控制卡...");
            try
            {
                bool isSuccess = await Task.Run(() =>
                {
                    try { return _motionController.Connect(); }
                    catch { return false; }
                });
                if (isSuccess)
                {
                    ConnectStatus = "已连接";
                    AddLog("INFO", "运动控制卡连接成功");
                }
                else
                {
                    ConnectStatus = "连接失败";
                    AddLog("ERROR", "运动控制卡连接失败，请检查：1.控制卡电源 2.网线连接 3.IP地址(192.168.5.11)");
                    MessageBox.Show("运动控制卡连接失败！\n\n请检查：\n1. 控制卡是否上电\n2. 网线是否连接\n3. IP地址是否为192.168.5.11\n4. 本机IP是否在同一网段", "连接失败", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                ConnectStatus = "连接异常";
                AddLog("ERROR", $"运动控制卡连接异常: {ex.Message}");
                MessageBox.Show($"运动控制卡连接异常：{ex.Message}", "连接异常", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

       
        //电机使能
       
        private async Task OnSevonCommandAsync()
        {
            AddLog("INFO", "尝试电机使能操作...");
            bool isSuccess = await Task.Run(() => _sevon.Sevon());
            if (isSuccess)
            {
                SevonStatus = "电机使能";
                AddLog("INFO", "电机使能成功，传送带已准备就绪");
            }
            else
            {
                SevonStatus = "电机未使能";
                AddLog("WARN", "电机使能失败，请检查控制卡连接状态");
            }
        }

        
        //启动传送带
        
        private async Task OnMoveCommandAsync()
        {
            AddLog("INFO", "启动传送带运动...");
            bool isSuccess = await Task.Run(() => _move.Vmove());
            MoveStatus = "传送带运动";
            AddLog("INFO", $"传送带已启动运动，执行结果: {isSuccess}");
        }

        
        //停止传送带
       
        private async Task OnStopCommandAsync()
        {
            AddLog("INFO", "停止传送带运动...");
            bool isSuccess = await Task.Run(() => _stop.Vstop());
            MoveStatus = "传送带停止";
            AddLog("INFO", $"传送带已停止，执行结果: {isSuccess}");
        }

       
       //调速
      
        private async Task OnChangeSpeedCommandAsync()
        {
            AddLog("INFO", $"调整传送带速度，目标速度: {TargetSpeed}");
            bool isSuccess = await Task.Run(() => _change.ChangeSpeed(TargetSpeed));
            if (isSuccess)
            {
                AddLog("INFO", $"速度调整成功，当前速度: {TargetSpeed}");
            }
            else
            {
                AddLog("ERROR", $"速度调整失败，错误码: {_change.LastErrorCode}");
                MessageBox.Show($"调速失败,错误码{_change.LastErrorCode}");
            }
        }
        #endregion

        #region 电磁阀模块
        /// <summary>电磁阀服务引用，控制推料机构气缸</summary>
        private readonly ISolenoidValueService _solenoidValueService;

        /// <summary>推料机构状态文本，"推料机构触发"或"推料机构未触发"，绑定到设备状态LED灯</summary>
        private string _solenoid = "推料机构未触发";
        /// <summary>推料机构状态，界面绑定显示</summary>
        public string Solendio
        {
            get => _solenoid;
            set => SetProperty(ref _solenoid, value);
        }

        /// <summary>手动打开电磁阀命令，XAML绑定到"打开电磁阀"按钮</summary>
        public ICommand OpenValveCommand { get; }
        /// <summary>手动关闭电磁阀命令，XAML绑定到"关闭电磁阀"按钮</summary>
        public ICommand CloseValveCommand { get; }

       
        //打开电磁阀
       
        private async Task OpenValveAsync()
        {
            AddLog("INFO", "手动打开电磁阀...");
            bool isSuccess = await Task.Run(() => _solenoidValueService.OpenValue());
            if (isSuccess)
            {
                Solendio = "推料机构触发";
                AddLog("INFO", "电磁阀打开成功，推料机构已触发");
            }
            else
            {
                AddLog("ERROR", "电磁阀打开失败");
            }
        }

       
        //关闭电磁阀
       
        private async Task CloseValveAsync()
        {
            AddLog("INFO", "手动关闭电磁阀...");
            bool isSuccess = await Task.Run(() => _solenoidValueService.CloseValue());
            if (isSuccess)
            {
                Solendio = "推料机构未触发";
                AddLog("INFO", "电磁阀关闭成功，推料机构已复位");
            }
            else
            {
                AddLog("ERROR", "电磁阀关闭失败");
            }
        }
        #endregion

        #region 传感器与分拣模块
        /// <summary>传感器服务引用，读取光电传感器状态</summary>
        private readonly ISensorService _sensorService;

        /// <summary>上一次传感器状态，用于上升沿检测（从低变高时触发分拣）</summary>
        private bool _lastSensorState = false;
        /// <summary>分拣动作执行中标志，防止传感器上升沿重复触发</summary>
        private bool _isActionRunning = false;

        
        // 芯片检测结果队列，存储(是否合格, 入队时间)
        
        private readonly Queue<(bool IsOK, DateTime EnqueueTime)> _chipResultQueue = new Queue<(bool, DateTime)>();
        private readonly object _queueLock = new object();

        /// <summary>队列最大容量，超出时移除最旧的结果</summary>
        private const int MaxQueueSize = 20;

        /// <summary>结果过期时间（秒），超过此时间的结果视为过期，传感器触发时自动清理</summary>
        private const int ResultExpireSeconds = 20;

        /// <summary>传感器状态文本，"芯片经过（触发中断）"或"传感器常态（无芯片）"，绑定到设备状态LED灯</summary>
        private string _sMCinbit0 = "传感器未触发";
        /// <summary>传感器状态，界面绑定显示</summary>
        public string SMCinbit0
        {
            get => _sMCinbit0;
            set => SetProperty(ref _sMCinbit0, value);
        }

      
        // 传感器定时器回调，每10ms执行一次
       
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                bool currentSensorState = _sensorService.ReadSensorState();
                SMCinbit0 = currentSensorState ? "芯片经过（触发中断）" : "传感器常态（无芯片）";

                if (currentSensorState && !_lastSensorState && !_isActionRunning)
                {
                    AddLog("INFO", "传感器检测到芯片经过，触发分拣流程");
                    TriggerValveByVisionResultAsync(currentSensorState);
                }
                _lastSensorState = currentSensorState;
            }
            catch (Exception ex)
            {
                SMCinbit0 = $"读取失败：{ex.Message}";
            }
        }

       
        //分拣流程
       
        private async void TriggerValveByVisionResultAsync(bool SMCinbit0)
        {
            _isActionRunning = true;
            try
            {
                bool isChipQualified = true;
                int queueCount = 0;
                double resultAge = 0;

                lock (_queueLock)
                {
                    // 清理过期结果：超过25秒的结果视为过期，可能是芯片未到达传感器或已掉落
                    while (_chipResultQueue.Count > 0)
                    {
                        var oldest = _chipResultQueue.Peek();
                        resultAge = (DateTime.Now - oldest.EnqueueTime).TotalSeconds;
                        if (resultAge > ResultExpireSeconds)
                        {
                            var expired = _chipResultQueue.Dequeue();
                            AddLog("WARN", $"清理过期结果: {(expired.IsOK ? "合格" : "不合格")}, 已等待: {resultAge:F1}秒 > {ResultExpireSeconds}秒");
                        }
                        else
                        {
                            break;
                        }
                    }

                    // 从队列取出最新的检测结果
                    if (_chipResultQueue.Count > 0)
                    {
                        var item = _chipResultQueue.Dequeue();
                        isChipQualified = item.IsOK;
                        resultAge = (DateTime.Now - item.EnqueueTime).TotalSeconds;
                    }
                    else
                    {
                        // [Fail-Safe 工业核心原则]：传感器检测到工件到位，但视觉队列为空（漏检/通讯断流/帧丢失）
                        // 严禁静默默认合格放行！必须强制判定为 NG 触发电磁阀剔除打入废料盒！
                        isChipQualified = false;
                        AddLog("WARN", "[Fail-Safe 联锁] 传感器触发但结果队列为空(视觉漏检或丢帧)，强制判定为 NG 剔除，严禁静默放行！");
                    }
                    queueCount = _chipResultQueue.Count;
                }

                AddLog("DEBUG", $"分拣流程启动 - 结果等待: {resultAge:F1}秒, 出队后剩余: {queueCount}条, 芯片状态: {(isChipQualified ? "合格" : "不合格")}");

                if (!isChipQualified)
                {
                    AddLog("WARN", "检测到不合格芯片/异常工件，触发电磁阀执行 NG 剔除");
                    await Task.Delay(3300);

                    Solendio = "推料机构触发";
                    await Task.Run(() => _solenoidValueService.OpenValue());
                    AddLog("INFO", "已执行 NG 剔除动作，传送机构继续运行");

                    await Task.Delay(300);

                    await Task.Run(() => _solenoidValueService.CloseValue());
                    Solendio = "推料机构触发完毕";
                }
                else
                {
                    AddLog("INFO", "检测到合格芯片，无需剔除，继续传送");
                }
            }
            finally
            {
                _isActionRunning = false;
            }
        }
        #endregion

        #region 视觉模块
        /// <summary>相机服务引用，控制GigE工业相机采集</summary>
        private readonly ICameraService _cameraService;
        /// <summary>模板服务引用，创建和管理Halcon形状模板</summary>
        private readonly ITemplateService _templateService;
        /// <summary>检测服务引用，执行模板匹配和芯片针脚检测</summary>
        private readonly IDetectionService _detectionService;

        /// <summary>当前显示图像，绑定到Halcon窗口控件</summary>
        private HObject _currentImage;
        /// <summary>检测状态文本，"OK"/"NG"/"No found"等，绑定到设备状态LED灯</summary>
        private string _detectionStatus;
        /// <summary>相机是否已打开，绑定到设备状态LED灯</summary>
        private bool _isCameraOpen;
        /// <summary>模板是否已绘制标志</summary>
        private bool _isTemplateDrawn;
        /// <summary>检测区域1是否已绘制标志</summary>
        private bool _isCheckRectXidDrawn;
        /// <summary>检测区域2是否已绘制标志</summary>
        private bool _isCheckRectXid2Drawn;
        /// <summary>是否正在实时采集检测，绑定到设备状态LED灯</summary>
        private bool _isGrabbing;
        /// <summary>首帧标志，用于获取图像尺寸信息</summary>
        private bool _isFirstImage = true;
        /// <summary>
        /// 检测并发锁标志，防止多个Task.Run同时执行检测导致内存飙升
        /// </summary>
        private bool _isDetecting = false;

        /// <summary>
        /// 模板加载中标志，防止用户重复点击"调用模板"导致多次重建
        /// </summary>
        private bool _isLoadingTemplate = false;
        /// <summary>检测区域1的Halcon XLD轮廓，传给DetectionService用于针脚分割</summary>
        private HObject _checkRectXid;
        /// <summary>检测区域2的Halcon XLD轮廓，传给DetectionService用于针脚分割</summary>
        private HObject _checkRectXid2;

        /// <summary>模板矩形区域坐标：左上角行、列，右下角行、列</summary>
        private double _templateRow1, _templateColumn1, _templateRow2, _templateColumn2;
        /// <summary>检测区域1参数：中心行、中心列、旋转角度(弧度)、半长轴、半短轴</summary>
        private double _checkRect1Row, _checkRect1Column, _checkRect1Phi, _checkRect1Length1, _checkRect1Length2;
        /// <summary>检测区域2参数：中心行、中心列、旋转角度(弧度)、半长轴、半短轴</summary>
        private double _checkRect2Row, _checkRect2Column, _checkRect2Phi, _checkRect2Length1, _checkRect2Length2;

        /// <summary>当前显示图像，绑定到Halcon HSmartWindowControlWPF</summary>
        public HObject CurrentImage
        {
            get => _currentImage;
            set => SetProperty(ref _currentImage, value);
        }

        /// <summary>检测状态文本，界面绑定显示</summary>
        public string DetectionStatus
        {
            get => _detectionStatus;
            set => SetProperty(ref _detectionStatus, value);
        }

        /// <summary>相机是否已打开，界面绑定显示</summary>
        public bool IsCameraOpen
        {
            get => _isCameraOpen;
            private set => SetProperty(ref _isCameraOpen, value);
        }

        /// <summary>是否正在实时采集检测，界面绑定显示</summary>
        public bool IsGrabbing
        {
            get => _isGrabbing;
            private set => SetProperty(ref _isGrabbing, value);
        }

        /// <summary>打开相机命令，XAML绑定到"打开相机"按钮</summary>
        public ICommand OpenCameraCommand { get; }
        /// <summary>绘制芯片模板命令，XAML绑定到"绘制芯片模板"按钮</summary>
        public ICommand DrawTemplateCommand { get; }
        /// <summary>绘制检测区域1命令，XAML绑定到"绘制检测区域一"按钮</summary>
        public ICommand DrawCheckRectXidCommand { get; }
        /// <summary>绘制检测区域2命令，XAML绑定到"绘制检测区域二"按钮</summary>
        public ICommand DrawCheckRectXid2Command { get; }
        /// <summary>开始实时检测命令，XAML绑定到"开始实时"按钮</summary>
        public ICommand StartLiveCommand { get; }
        /// <summary>停止实时检测命令，XAML绑定到"停止实时"按钮</summary>
        public ICommand StopLiveCommand { get; }
        /// <summary>关闭相机命令，XAML绑定到"关闭相机"按钮</summary>
        public ICommand CloseCameraCommand { get; }
        /// <summary>保存模板命令，XAML绑定到"保存模板"按钮</summary>
        public ICommand SaveTemplateCommand { get; }
        /// <summary>调用模板命令，XAML绑定到"调用模板"按钮</summary>
        public ICommand LoadTemplateCommand { get; }

        /// <summary>请求绘制模板区域事件，View层订阅后调用Halcon DrawRectangle1</summary>
        public event EventHandler RequestDrawTemplate;
        /// <summary>请求绘制检测区域1事件，View层订阅后调用Halcon DrawRectangle2</summary>
        public event EventHandler RequestDrawCheckRectXid;
        /// <summary>请求绘制检测区域2事件，View层订阅后调用Halcon DrawRectangle2</summary>
        public event EventHandler RequestDrawCheckRectXid2;
        /// <summary>首帧图像接收事件，传递图像尺寸给View层初始化Halcon窗口</summary>
        public event EventHandler<ImageSizeEventArgs> FirstImageReceived;

        /// <summary>
        /// 打开相机命令
        /// 异步执行相机初始化，防止UI卡死
        /// 连接失败时弹出详细错误提示，包含常见排查步骤
        /// </summary>
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
                await Task.Run(() =>
                {
                    try { _cameraService.Open(); }
                    catch (Exception ex)
                    {
                        throw new Exception($"相机打开失败：{ex.Message}\n\n请检查：\n1. 相机是否上电\n2. 网线是否连接\n3. 相机IP是否可达");
                    }
                });

                if (!_cameraService.IsOpen)
                {
                    AddLog("ERROR", "相机打开失败：设备未响应");
                    MessageBox.Show("相机打开失败！设备未响应\n\n请检查：\n1. 相机是否上电\n2. 网线是否连接\n3. 相机IP是否可达\n4. Halcon是否正确安装", "相机连接失败", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _cameraService.StartGrabbing();
                _isFirstImage = true;
                IsCameraOpen = true;
                AddLog("INFO", "相机模块初始化完成，等待图像采集");
            }
            catch (Exception ex)
            {
                IsCameraOpen = false;
                AddLog("ERROR", $"打开相机失败: {ex.Message}");
                MessageBox.Show(ex.Message, "相机连接失败", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

       
        // 绘制芯片模板命令处理
      
        private void DrawTemplate()
        {
            AddLog("INFO", "开始绘制芯片模板...");
            if (!IsCameraOpen || _cameraService.CurrentImage == null)
            {
                AddLog("ERROR", "绘制模板失败: 相机未打开或无图像");
                MessageBox.Show("请先打开相机,确保画面有图像");
                return;
            }
            if (_isTemplateDrawn)
            {
                AddLog("WARN", "模板已存在，请先关闭相机重新创建");
                MessageBox.Show("模板已绘制，请勿重复绘制");
                return;
            }
            AddLog("INFO", "请在图像窗口绘制模板区域");
            RequestDrawTemplate?.Invoke(this, EventArgs.Empty);
        }

       
        // 绘制检测区域1命令处理
      
        private void DrawCheckRectXid()
        {
            if (!IsCameraOpen || !_isTemplateDrawn)
            {
                AddLog("ERROR", "绘制检测区域1失败: 请先创建模板");
                MessageBox.Show("请先绘制模板");
                return;
            }
            if (_isCheckRectXidDrawn)
            {
                AddLog("WARN", "检测区域1已存在");
                MessageBox.Show("检测区域1已绘制，请勿重复绘制");
                return;
            }
            AddLog("INFO", "请在图像窗口绘制检测区域1");
            RequestDrawCheckRectXid?.Invoke(this, EventArgs.Empty);
        }

       
        // 绘制检测区域2命令处理
      
        private void DrawCheckRectXid2()
        {
            if (!IsCameraOpen || !_isTemplateDrawn || !_isCheckRectXidDrawn)
            {
                AddLog("ERROR", "绘制检测区域2失败: 请先创建模板和检测区域1");
                MessageBox.Show("请先绘制模板和检测区域1");
                return;
            }
            if (_isCheckRectXid2Drawn)
            {
                AddLog("WARN", "检测区域2已存在");
                MessageBox.Show("检测区域2已绘制，请勿重复绘制");
                return;
            }
            AddLog("INFO", "请在图像窗口绘制检测区域2");
            RequestDrawCheckRectXid2?.Invoke(this, EventArgs.Empty);
        }

      
        //启动实时检测
        
        private void StartLive()
        {
            AddLog("INFO", "启动实时检测...");
            StopLive();

            if (_cameraService == null || _templateService == null || _detectionService == null)
            {
                AddLog("ERROR", "启动检测失败: 服务未初始化");
                MessageBox.Show("服务未初始化，无法开始预览");
                return;
            }

            if (!IsCameraOpen || _isGrabbing)
            {
                AddLog("WARN", "启动检测失败: 相机未打开或已在检测中");
                return;
            }

            if (!_templateService.IsTemplateCreated || _templateService.ModelID == null || _templateService.ModelID.Length == 0)
            {
                AddLog("ERROR", "启动检测失败: 模板未创建");
                MessageBox.Show("模板未创建，请先绘制模板");
                return;
            }

            if (!_isCheckRectXidDrawn || !_isCheckRectXid2Drawn || _checkRectXid == null || _checkRectXid2 == null)
            {
                AddLog("ERROR", "启动检测失败: 检测区域未完整绘制");
                MessageBox.Show("请先绘制检测区域");
                return;
            }

            try
            {
                _detectionService.SetTemplate(_templateService.ModelID, _templateService.ModelContours, _checkRectXid, _checkRectXid2);
                _detectionService.SetPinCountRange(_config.PinCountMin, _config.PinCountMax, _config.PinCount2Min, _config.PinCount2Max);
                AddLog("INFO", $"模板和检测区域已加载到检测服务 (针脚范围: {_config.PinCountMin}-{_config.PinCountMax}, {_config.PinCount2Min}-{_config.PinCount2Max})");

                lock (_queueLock)
                {
                    _chipResultQueue.Clear();
                }

                IsGrabbing = true;
                DetectionStatus = "正在采集检测";
                AddLog("INFO", "实时检测已启动，等待芯片图像采集");
            }
            catch (Exception ex)
            {
                AddLog("ERROR", $"启动实时检测失败: {ex.Message}");
                MessageBox.Show($"开始预览失败：{ex.Message}");
                IsGrabbing = false;
                DetectionStatus = "实时失败" + ex.Message;
            }
        }

       
        // 停止实时检测，设置IsGrabbing=false
       
        public  void StopLive()
        {
            if (!_isGrabbing) return;
            IsGrabbing = false;
            DetectionStatus = "已停止检测";
            AddLog("INFO", "实时检测已停止");
        }

        
        //关闭相机
      
        private async Task CloseCameraAsync()
        {
            if (!IsCameraOpen)
            {
                AddLog("WARN", "相机未打开，无需关闭");
                return;
            }
            AddLog("INFO", "正在关闭相机...");

            await Task.Run(() => _cameraService.StopGrabbing());
            AddLog("DEBUG", "相机图像采集已停止");

            await Task.Run(() => _cameraService.Close());
            AddLog("DEBUG", "相机设备已关闭");

            IsCameraOpen = false;
            IsGrabbing = false;
            _isTemplateDrawn = false;

            _templateService.ClearTemplate();
            AddLog("DEBUG", "模板数据已清除");

            if (CurrentImage != null && CurrentImage.IsInitialized())
            {
                CurrentImage.Dispose();
            }
            CurrentImage = null;

            if (_checkRectXid != null && _checkRectXid.IsInitialized())
            {
                _checkRectXid.Dispose();
            }
            _checkRectXid = null;
            _isCheckRectXidDrawn = false;

            if (_checkRectXid2 != null && _checkRectXid2.IsInitialized())
            {
                _checkRectXid2.Dispose();
            }
            _checkRectXid2 = null;
            _isCheckRectXid2Drawn = false;

            AddLog("INFO", "相机关闭完成，所有资源已释放");
            MessageBox.Show("相机已关闭");
        }

        
        // 设置模板区域并创建形状模板
       
        public void SetTemplateRegion(double hv_mRow1, double hv_mColumn1, double hv_mRow2, double hv_mColumn2)
        {
            if (CurrentImage == null || !CurrentImage.IsInitialized())
            {
                AddLog("ERROR", "设置模板失败: 当前图像无效");
                MessageBox.Show("当前图像无效，无法创建模板，请确保相机正常采集图像");
                return;
            }

            try
            {
                _templateRow1 = hv_mRow1;
                _templateColumn1 = hv_mColumn1;
                _templateRow2 = hv_mRow2;
                _templateColumn2 = hv_mColumn2;

                _templateService.SetTemplateRegion(hv_mRow1, hv_mColumn1, hv_mRow2, hv_mColumn2);
                _templateService.CreateTemplate(CurrentImage, hv_mRow1, hv_mColumn1, hv_mRow2, hv_mColumn2);

                if (!_templateService.IsTemplateCreated || _templateService.ModelID == null)
                {
                    AddLog("ERROR", "模板创建失败: ModelID无效");
                    MessageBox.Show("模板创建失败，ModelID无效");
                    _templateService.ClearTemplate();
                    return;
                }

                _isTemplateDrawn = true;
                AddLog("INFO", "芯片模板创建成功");
                MessageBox.Show("模板创建成功");
            }
            catch (Exception ex)
            {
                AddLog("ERROR", $"设置模板区域失败: {ex.Message}");
                MessageBox.Show($"设置模板区域失败：{ex.Message}");
                _templateService.ClearTemplate();
                _isTemplateDrawn = false;
            }
        }

        
        // 设置检测区域1，生成旋转矩形XLD轮廓
       
        public void SetCheckRectXid(double hv_r2Row, double hv_r2Column, double hv_r2Phi, double hv_r2Length1, double hv_r2Length2)
        {
            try
            {
                _checkRect1Row = hv_r2Row;
                _checkRect1Column = hv_r2Column;
                _checkRect1Phi = hv_r2Phi;
                _checkRect1Length1 = hv_r2Length1;
                _checkRect1Length2 = hv_r2Length2;

                HOperatorSet.GenRectangle2ContourXld(out HObject ho_r2Rectangle, hv_r2Row, hv_r2Column, hv_r2Phi, hv_r2Length1, hv_r2Length2);
                if (ho_r2Rectangle == null || !ho_r2Rectangle.IsInitialized())
                {
                    AddLog("ERROR", "生成检测区域1失败: 坐标或尺寸无效");
                    MessageBox.Show("生成检测区域1失败，请检查绘制的坐标和尺寸");
                    return;
                }

                if (_checkRectXid != null && _checkRectXid.IsInitialized())
                {
                    _checkRectXid.Dispose();
                }

                _checkRectXid = ho_r2Rectangle;
                _isCheckRectXidDrawn = true;
                AddLog("INFO", "检测区域1设置成功");
                MessageBox.Show("检测区域1设置成功");
            }
            catch (Exception ex)
            {
                AddLog("ERROR", $"设置检测区域1失败: {ex.Message}");
                MessageBox.Show($"设置检测区域1失败：{ex.Message}");
                if (_checkRectXid != null && _checkRectXid.IsInitialized())
                {
                    _checkRectXid.Dispose();
                }
                _checkRectXid = null;
                _isCheckRectXidDrawn = false;
            }
        }

      
        // 设置检测区域2，生成旋转矩形XLD轮廓
        
        public void SetCheckRectXid2(double hv_r3Row, double hv_r3Column, double hv_r3Phi, double hv_r3Length1, double hv_r3Length2)
        {
            try
            {
                _checkRect2Row = hv_r3Row;
                _checkRect2Column = hv_r3Column;
                _checkRect2Phi = hv_r3Phi;
                _checkRect2Length1 = hv_r3Length1;
                _checkRect2Length2 = hv_r3Length2;

                HOperatorSet.GenRectangle2ContourXld(out HObject ho_r3Rectangle, hv_r3Row, hv_r3Column, hv_r3Phi, hv_r3Length1, hv_r3Length2);
                if (ho_r3Rectangle == null || !ho_r3Rectangle.IsInitialized())
                {
                    AddLog("ERROR", "生成检测区域2失败: 坐标或尺寸无效");
                    MessageBox.Show("生成检测区域2失败，请检查绘制的坐标和尺寸");
                    return;
                }

                if (_checkRectXid2 != null && _checkRectXid2.IsInitialized())
                {
                    _checkRectXid2.Dispose();
                }

                _checkRectXid2 = ho_r3Rectangle;
                _isCheckRectXid2Drawn = true;
                AddLog("INFO", "检测区域2设置成功");
                MessageBox.Show("检测区域2设置成功");
            }
            catch (Exception ex)
            {
                AddLog("ERROR", $"设置检测区域2失败: {ex.Message}");
                MessageBox.Show($"设置检测区域2失败：{ex.Message}");
                if (_checkRectXid2 != null && _checkRectXid2.IsInitialized())
                {
                    _checkRectXid2.Dispose();
                }
                _checkRectXid2 = null;
                _isCheckRectXid2Drawn = false;
            }
        }

      
        // 模板文件存储目录，位于程序根目录下的Templates文件夹
    
        private static readonly string TemplatesDir = System.IO.Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "Templates");

       
        // 保存模板方法
       
        private void SaveTemplate()
        {
            // 前置检查：模板和检测区域必须都已绘制
            if (!_isTemplateDrawn)
            {
                AddLog("WARN", "保存模板失败: 模板未创建");
                MessageBox.Show("请先绘制芯片模板");
                return;
            }
            if (!_isCheckRectXidDrawn || !_isCheckRectXid2Drawn)
            {
                AddLog("WARN", "保存模板失败: 检测区域未完整绘制");
                MessageBox.Show("请先绘制完整的检测区域");
                return;
            }

            // 创建输入名称的对话框
            var inputWindow = new Window
            {
                Title = "保存模板",
                Width = 380,
                Height = 180,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = System.Windows.Application.Current?.MainWindow,
                ResizeMode = ResizeMode.NoResize
            };

            var panel = new StackPanel { Margin = new Thickness(20) };
            panel.Children.Add(new TextBlock { Text = "请输入模板名称：", FontSize = 14, Margin = new Thickness(0, 0, 0, 10) });

            // 默认名称带时间戳，避免重名
            var textBox = new TextBox
            {
                Text = $"模板_{DateTime.Now:yyyyMMdd_HHmmss}",
                FontSize = 14,
                Padding = new Thickness(6, 4, 6, 4)
            };
            textBox.SelectAll();
            panel.Children.Add(textBox);

            // 保存/取消按钮
            var btnPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 15, 0, 0)
            };

            var okBtn = new Button { Content = "保存", Width = 80, Height = 32, Margin = new Thickness(5, 0, 0, 0) };
            var cancelBtn = new Button { Content = "取消", Width = 80, Height = 32, Margin = new Thickness(5, 0, 0, 0) };
            okBtn.Click += (s, e) => { inputWindow.DialogResult = true; inputWindow.Close(); };
            cancelBtn.Click += (s, e) => { inputWindow.DialogResult = false; inputWindow.Close(); };
            btnPanel.Children.Add(okBtn);
            btnPanel.Children.Add(cancelBtn);
            panel.Children.Add(btnPanel);
            inputWindow.Content = panel;

            // 显示对话框，用户点击取消则退出
            textBox.Focus();
            if (inputWindow.ShowDialog() != true)
            {
                AddLog("INFO", "保存模板已取消");
                return;
            }

            // 验证模板名称合法性
            string templateName = textBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(templateName))
            {
                MessageBox.Show("模板名称不能为空");
                return;
            }

            // 检查是否包含文件名非法字符（如 \ / : * ? " < > |）
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                if (templateName.Contains(c))
                {
                    MessageBox.Show("模板名称包含非法字符，请重新输入");
                    return;
                }
            }

            try
            {
                // 确保模板目录存在
                System.IO.Directory.CreateDirectory(TemplatesDir);

                // 将当前模板坐标参数打包为配置对象
                var config = new TemplateConfig
                {
                    TemplateName = templateName,
                    TemplateRow1 = _templateRow1,
                    TemplateColumn1 = _templateColumn1,
                    TemplateRow2 = _templateRow2,
                    TemplateColumn2 = _templateColumn2,
                    CheckRect1Row = _checkRect1Row,
                    CheckRect1Column = _checkRect1Column,
                    CheckRect1Phi = _checkRect1Phi,
                    CheckRect1Length1 = _checkRect1Length1,
                    CheckRect1Length2 = _checkRect1Length2,
                    CheckRect2Row = _checkRect2Row,
                    CheckRect2Column = _checkRect2Column,
                    CheckRect2Phi = _checkRect2Phi,
                    CheckRect2Length1 = _checkRect2Length1,
                    CheckRect2Length2 = _checkRect2Length2
                };

                // 以模板名称作为文件名
                string filePath = System.IO.Path.Combine(TemplatesDir, $"{templateName}.json");

                // 同名模板覆盖确认
                if (System.IO.File.Exists(filePath))
                {
                    var overwrite = MessageBox.Show($"模板 \"{templateName}\" 已存在，是否覆盖？", "确认覆盖", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (overwrite != MessageBoxResult.Yes)
                    {
                        AddLog("INFO", "保存模板已取消（不覆盖）");
                        return;
                    }
                }

                // 序列化为JSON并写入文件
                string json = config.ToJson();
                System.IO.File.WriteAllText(filePath, json, System.Text.Encoding.UTF8);

                AddLog("INFO", $"模板 \"{templateName}\" 已保存到: {filePath}");
                MessageBox.Show($"模板 \"{templateName}\" 保存成功");
            }
            catch (Exception ex)
            {
                AddLog("ERROR", $"保存模板失败: {ex.Message}");
                MessageBox.Show($"保存模板失败：{ex.Message}");
            }
        }

     
        // 调用模板方法
        // 扫描Templates目录下的所有JSON文件，弹出选择对话框让用户选择要加载的模板
      
        private void LoadTemplate()
        {
            // 防重复加载保护
            if (_isLoadingTemplate)
            {
                AddLog("WARN", "模板正在加载中，请勿重复操作");
                return;
            }

            // 检查模板目录是否存在
            if (!System.IO.Directory.Exists(TemplatesDir))
            {
                AddLog("WARN", "调用模板失败: 未找到模板目录");
                MessageBox.Show("未找到已保存的模板，请先保存模板");
                return;
            }

            // 扫描目录下所有JSON模板文件
            var jsonFiles = System.IO.Directory.GetFiles(TemplatesDir, "*.json");
            if (jsonFiles.Length == 0)
            {
                AddLog("WARN", "调用模板失败: 模板目录为空");
                MessageBox.Show("未找到已保存的模板，请先保存模板");
                return;
            }

            // 前置检查：相机必须打开且有当前图像（模板重建需要图像）
            if (!IsCameraOpen)
            {
                AddLog("WARN", "调用模板失败: 相机未打开");
                MessageBox.Show("请先打开相机并采集图像，再调用模板");
                return;
            }

            if (CurrentImage == null || !CurrentImage.IsInitialized())
            {
                AddLog("WARN", "调用模板失败: 当前无有效图像");
                MessageBox.Show("当前无有效图像，请等待相机采集图像后再调用模板");
                return;
            }

            // 读取所有模板文件的名称，用于列表展示
            var templates = new List<(string FilePath, string DisplayName)>();
            foreach (var file in jsonFiles)
            {
                try
                {
                    string json = System.IO.File.ReadAllText(file, System.Text.Encoding.UTF8);
                    var cfg = TemplateConfig.FromJson(json);
                    // 优先使用模板配置中的名称，否则用文件名
                    string displayName = !string.IsNullOrWhiteSpace(cfg?.TemplateName) ? cfg.TemplateName : System.IO.Path.GetFileNameWithoutExtension(file);
                    templates.Add((file, displayName));
                }
                catch
                {
                    // JSON解析失败时用文件名代替
                    templates.Add((file, System.IO.Path.GetFileNameWithoutExtension(file)));
                }
            }

            // 创建模板选择对话框
            var selectWindow = new Window
            {
                Title = "调用模板",
                Width = 400,
                Height = 340,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = System.Windows.Application.Current?.MainWindow,
                ResizeMode = ResizeMode.NoResize
            };

            var panel = new StackPanel { Margin = new Thickness(20) };
            panel.Children.Add(new TextBlock { Text = "请选择要调用的模板：", FontSize = 14, Margin = new Thickness(0, 0, 0, 10) });

            // 列表框展示所有可用模板
            var listBox = new ListBox { FontSize = 13, Height = 170 };
            foreach (var t in templates)
            {
                listBox.Items.Add(t.DisplayName);
            }
            if (listBox.Items.Count > 0) listBox.SelectedIndex = 0;
            panel.Children.Add(listBox);

            // 加载/删除/取消按钮
            var btnPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 15, 0, 0)
            };

            var okBtn = new Button { Content = "加载", Width = 70, Height = 32, Margin = new Thickness(5, 0, 0, 0) };
            var deleteBtn = new Button { Content = "删除", Width = 70, Height = 32, Margin = new Thickness(5, 0, 0, 0), Background = System.Windows.Media.Brushes.DarkRed, Foreground = System.Windows.Media.Brushes.White };
            var cancelBtn = new Button { Content = "取消", Width = 70, Height = 32, Margin = new Thickness(5, 0, 0, 0) };

            okBtn.Click += (s, e) =>
            {
                if (listBox.SelectedIndex < 0)
                {
                    MessageBox.Show("请选择一个模板");
                    return;
                }
                selectWindow.DialogResult = true;
                selectWindow.Close();
            };

            // 删除按钮：删除选中的模板文件
            deleteBtn.Click += (s, e) =>
            {
                if (listBox.SelectedIndex < 0)
                {
                    MessageBox.Show("请先选择要删除的模板");
                    return;
                }

                int idx = listBox.SelectedIndex;
                string delName = templates[idx].DisplayName;
                string delPath = templates[idx].FilePath;

                var confirm = MessageBox.Show($"确定要删除模板 \"{delName}\" 吗？\n\n此操作不可恢复！", "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (confirm != MessageBoxResult.Yes) return;

                try
                {
                    System.IO.File.Delete(delPath);
                    AddLog("INFO", $"模板 \"{delName}\" 已删除");
                    templates.RemoveAt(idx);
                    listBox.Items.RemoveAt(idx);

                    if (templates.Count == 0)
                    {
                        MessageBox.Show("所有模板已删除，无可用模板");
                        selectWindow.DialogResult = false;
                        selectWindow.Close();
                        return;
                    }

                    if (listBox.SelectedIndex < 0 && listBox.Items.Count > 0)
                    {
                        listBox.SelectedIndex = 0;
                    }
                }
                catch (Exception ex)
                {
                    AddLog("ERROR", $"删除模板失败: {ex.Message}");
                    MessageBox.Show($"删除失败：{ex.Message}");
                }
            };

            cancelBtn.Click += (s, e) => { selectWindow.DialogResult = false; selectWindow.Close(); };
            btnPanel.Children.Add(okBtn);
            btnPanel.Children.Add(deleteBtn);
            btnPanel.Children.Add(cancelBtn);
            panel.Children.Add(btnPanel);
            selectWindow.Content = panel;

            // 显示选择对话框，用户点击取消则退出
            if (selectWindow.ShowDialog() != true || listBox.SelectedIndex < 0)
            {
                AddLog("INFO", "调用模板已取消");
                return;
            }

            // 获取用户选择的模板文件路径和名称
            string selectedFile = templates[listBox.SelectedIndex].FilePath;
            string selectedName = templates[listBox.SelectedIndex].DisplayName;

            // 设置加载标志，防止重复触发
            _isLoadingTemplate = true;
            try
            {
                // 读取并反序列化模板配置
                string json = System.IO.File.ReadAllText(selectedFile, System.Text.Encoding.UTF8);
                var config = TemplateConfig.FromJson(json);

                if (config == null)
                {
                    AddLog("ERROR", "调用模板失败: 配置文件内容无效");
                    MessageBox.Show("模板配置文件内容无效");
                    return;
                }

                // 如果正在实时检测，先停止
                if (_isGrabbing)
                {
                    StopLive();
                }

                // 重置绘制状态标志，允许重新设置模板和检测区域
                _isTemplateDrawn = false;
                _isCheckRectXidDrawn = false;
                _isCheckRectXid2Drawn = false;

                // 根据配置文件中的坐标重建模板区域和形状模板
                SetTemplateRegion(config.TemplateRow1, config.TemplateColumn1, config.TemplateRow2, config.TemplateColumn2);

                // 验证模板是否重建成功
                if (!_templateService.IsTemplateCreated)
                {
                    AddLog("ERROR", "调用模板失败: 模板重建失败");
                    MessageBox.Show("模板重建失败，请检查图像是否正常");
                    return;
                }

                // 重建两个检测区域的旋转矩形
                SetCheckRectXid(config.CheckRect1Row, config.CheckRect1Column, config.CheckRect1Phi, config.CheckRect1Length1, config.CheckRect1Length2);
                SetCheckRectXid2(config.CheckRect2Row, config.CheckRect2Column, config.CheckRect2Phi, config.CheckRect2Length1, config.CheckRect2Length2);

                AddLog("INFO", $"模板 \"{selectedName}\" 已成功加载");
                MessageBox.Show($"模板 \"{selectedName}\" 调用成功，模板和检测区域已重建");
            }
            catch (Exception ex)
            {
                AddLog("ERROR", $"调用模板失败: {ex.Message}");
                MessageBox.Show($"调用模板失败：{ex.Message}");
            }
            finally
            {
                // 无论成功失败，都重置加载标志
                _isLoadingTemplate = false;
            }
        }

       
        // 图像采集回调
        // - ho_Image 由 CameraService 事件传递，CameraService 负责释放，本方法不 Dispose
        // - ho_GrayImage 由本方法创建，在 finally 中释放
        // - 需要保留的图像通过 Clone 复制，Clone 出的副本独立管理生命周期
        // - 检测模式下只更新一次 CurrentImage（来自检测结果），避免重复 Clone
        // - 使用 _isDetecting 标志限制并发检测任务，防止 Task.Run 积压导致内存飙升
        // </summary>
        private void OnImageGrabbed(object sender, HObject ho_Image)
        {
            if (ho_Image == null || !ho_Image.IsInitialized())//安全检查
            {
                return;
            }

            HObject ho_GrayImage = null;
            try
            {
                HOperatorSet.CountChannels(ho_Image, out HTuple channels);//统计图像的通道数
                if (channels.I == 3)//RGB三通道
                {
                    HOperatorSet.Rgb1ToGray(ho_Image, out ho_GrayImage);//转灰度图
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

                if (_isGrabbing)
                {
                    if (_isDetecting)//模式 A：检测模式 + 上一帧检测未完成
                    {
                        InvokeOnUI(() =>
                        {
                            var oldImage = CurrentImage;
                            CurrentImage = ho_GrayImage.Clone();
                            oldImage?.Dispose();
                        });
                    }
                    else//检测模式 + 可以启动新检测
                    {
                        _isDetecting = true;
                        var processImage = ho_GrayImage.Clone();

                        Task.Run(() =>
                        {
                            DetectionResult result = null;
                            try
                            {
                                result = _detectionService.Process(processImage);

                                if (result.IsRisingEdge)
                                {
                                    AddLog("INFO", "★★★ 检测到上升沿（识别到芯片）★★★");
                                }

                                if (result.IsFallingEdge)
                                {
                                    AddLog("INFO", "检测到下降沿（2秒超时）- 状态已重置，准备下一次检测");
                                }

                                if (result.ShouldTrigger && !result.IsError && result.ResultText != "No found")
                                {
                                    lock (_queueLock)
                                    {
                                        // 队列水位监控：当队列水位达到 80% (16/20) 时触发机台降速/排队预警
                                        if (_chipResultQueue.Count >= (int)(MaxQueueSize * 0.8))
                                        {
                                            AddLog("WARN", $"[有界并发监控] 结果队列高水位预警 ({_chipResultQueue.Count}/{MaxQueueSize} >= 80%)，建议降低皮带送料速度以防溢出！");
                                        }

                                        // 队列容量限制：超出时移除最旧的结果，防止堆积
                                        while (_chipResultQueue.Count >= MaxQueueSize)
                                        {
                                            var removed = _chipResultQueue.Dequeue();
                                            AddLog("WARN", $"[Fail-Safe 溢出保护] 队列已达上限 ({MaxQueueSize})，强制移除最旧结果: {(removed.IsOK ? "合格" : "不合格")}, 已等待: {(DateTime.Now - removed.EnqueueTime).TotalSeconds:F1}秒");
                                        }

                                        _chipResultQueue.Enqueue((result.IsOK, DateTime.Now));
                                        AddLog("DEBUG", $"检测结果入队 - 队列长度: {_chipResultQueue.Count}, 结果: {(result.IsOK ? "合格" : "不合格")}{(result.IsOK ? "" : $" [{result.DefectDescription}]")}");
                                    }

                                    SaveInspectionResult(result.IsOK, result.DefectDescription);//存入数据库
                                }
                                
                                InvokeOnUI(() =>
                                {
                                    DetectionStatus = result.ResultText;
                                    var oldImage = CurrentImage;
                                    if (result.DisplayImage != null && result.DisplayImage.IsInitialized())
                                    {
                                        CurrentImage = result.DisplayImage;
                                        result.DisplayImage = null;
                                    }
                                    else
                                    {
                                        CurrentImage = processImage.Clone(); 
                                    }
                                    oldImage?.Dispose();
                                });
                            }
                            catch (Exception ex)
                            {
                                AddLog("ERROR", $"视觉检测处理异常: {ex.Message}");
                                result?.DisplayImage?.Dispose();
                                InvokeOnUI(() =>
                                {
                                    DetectionStatus = $"检测出错:{ex.Message}";
                                });
                            }
                            finally
                            {
                                processImage?.Dispose();
                                _isDetecting = false;
                            }
                        });
                    }
                }
                else//非检测模式（预览）
                {
                    InvokeOnUI(() =>
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
        #endregion

        #region 数据记录模块
        private const int MaxInspectionRecords = 300;

        public ObservableCollection<ProductInspectionRecord> InspectionRecords { get; set; } = new ObservableCollection<ProductInspectionRecord>();
       // 加载历史检测记录
      
        private async Task LoadHistoryRecordsAsync()
        {
            AddLog("INFO", "正在加载历史检测记录...");
            try
            {
                var records = await Task.Run(() =>
                {
                    using (var repo = new ProductInspectionRecordRepository())
                    {
                        return repo.GetAll().OrderByDescending(x => x.InspectionTime).Take(MaxInspectionRecords).ToList();
                    }
                });

                InspectionRecords.Clear();
                foreach (var record in records)
                {
                    InspectionRecords.Add(record);
                }
                AddLog("INFO", $"历史记录加载完成，共加载 {records.Count} 条记录");
            }
            catch (Exception ex)
            {
                AddLog("WARN", $"加载历史记录（数据库离线，以单机内存模式运行）: {ex.Message}");
            }
        }

        #region 检测全局配置
        private readonly InspectionConfig _config;
        #endregion

        public void SaveInspectionResult(bool isOK, string defectReason = null)
        {
            ProductInspectionRecord newRecord = null;
            try
            {
                newRecord = new ProductInspectionRecord
                {
                    ProductModel = _config.CurrentChipModel,
                    BatchNo = _config.CurrentBatchNo,
                    InspectionTime = DateTime.Now,
                    TotalInspectedCount = 1,
                    QualifiedCount = isOK ? 1 : 0,
                    DefectiveCount = isOK ? 0 : 1,
                    DefectReasons = isOK ? "合格" : (string.IsNullOrEmpty(defectReason) ? _config.DefaultDefectReason : defectReason),
                    OperatorId = 1,
                    DeviceNo = _config.DeviceNo,
                    Remark = string.Empty
                };

                int saveResult;
                using (var recordRepository = new ProductInspectionRecordRepository())
                {
                    saveResult = recordRepository.Insert(newRecord);
                }

                if (saveResult > 0)
                {
                    AddLog("INFO", $"检测结果入库成功 - {(isOK ? "合格" : "不合格")} | 芯片型号: {_config.CurrentChipModel}");
                    InvokeOnUI(() =>
                    {
                        InspectionRecords.Insert(0, newRecord);
                        while (InspectionRecords.Count > MaxInspectionRecords)
                        {
                            InspectionRecords.RemoveAt(InspectionRecords.Count - 1);
                        }
                        RefreshChartData();
                    });
                }
                else
                {
                    AddLog("WARN", "检测记录写入数据库失败（以内存记录更新）");
                    InvokeOnUI(() =>
                    {
                        InspectionRecords.Insert(0, newRecord);
                        while (InspectionRecords.Count > MaxInspectionRecords)
                        {
                            InspectionRecords.RemoveAt(InspectionRecords.Count - 1);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                AddLog("WARN", $"数据持久化已切换至单机模式: {ex.Message}");
                InvokeOnUI(() =>
                {
                    InspectionRecords.Insert(0, newRecord);
                    while (InspectionRecords.Count > MaxInspectionRecords)
                    {
                        InspectionRecords.RemoveAt(InspectionRecords.Count - 1);
                    }
                });
            }
        }
        #endregion

        #region 折线图模块
        private readonly ProductInspectionRecordRepository _recordRepository;

        public SeriesCollection InspectionSeries { get; private set; }

        private string[] _timeLabels = Array.Empty<string>();
        public string[] TimeLabels
        {
            get => _timeLabels;
            set
            {
                _timeLabels = value;
                OnPropertyChanged();
            }
        }

        public Func<double, string> YAxisFormatter { get; }

      
        // 刷新折线图数据
     
        public void RefreshChartData()
        {
            try
            {
                DateTime startTime = DateTime.Now.AddDays(-1);
                var allRecords = _recordRepository.GetAll();

                if (allRecords == null || !allRecords.Any())
                {
                    ClearChartData();
                    return;
                }

                var filterRecords = allRecords.Where(x => x.InspectionTime >= startTime).ToList();

                if (!filterRecords.Any())
                {
                    ClearChartData();
                    return;
                }

                var minuteGroup = filterRecords
                    .GroupBy(x => x.InspectionTime.ToString("HH:mm"))
                    .OrderBy(g => g.Key)
                    .Select(g => new
                    {
                        TimeLabel = g.Key,
                        TotalCount = g.Count(),
                        DefectiveCount = g.Sum(x => x.DefectiveCount)
                    })
                    .ToList();

                TimeLabels = minuteGroup.Select(g => g.TimeLabel).ToArray();
                var totalCount = minuteGroup.Select(g => g.TotalCount).ToArray();
                var defectiveCount = minuteGroup.Select(g => g.DefectiveCount).ToArray();

                if (InspectionSeries == null || InspectionSeries.Count < 2) return;

                var totalSeries = (ChartValues<int>)InspectionSeries[0].Values;
                var defectiveSeries = (ChartValues<int>)InspectionSeries[1].Values;
                totalSeries.Clear();
                totalSeries.AddRange(totalCount);
                defectiveSeries.Clear();
                defectiveSeries.AddRange(defectiveCount);

                OnPropertyChanged(nameof(InspectionSeries));
                OnPropertyChanged(nameof(TimeLabels));
            }
            catch (Exception ex)
            {
                ClearChartData();
            }
        }

        private void ClearChartData()
        {
            TimeLabels = Array.Empty<string>();

            if (InspectionSeries == null || InspectionSeries.Count < 2) return;

            ((ChartValues<int>)InspectionSeries[0].Values).Clear();
            ((ChartValues<int>)InspectionSeries[1].Values).Clear();

            OnPropertyChanged(nameof(TimeLabels));
        }
        #endregion

        #region 自动刷新定时器
        private System.Timers.Timer _refreshTimer;

        public void StartAutoRefresh()
        {
            if (_refreshTimer != null) return;

            _refreshTimer = new System.Timers.Timer(60000);
            _refreshTimer.Elapsed += (s, e) =>
            {
                InvokeOnUI(() =>
                {
                    RefreshChartData();
                });
            };
            _refreshTimer.Start();
        }

        public void StopAutoRefresh()
        {
            _refreshTimer?.Stop();
            _refreshTimer?.Dispose();
            _refreshTimer = null;

            lock (_queueLock)
            {
                _chipResultQueue.Clear();
            }
        }
        #endregion

        #region IDisposable - 资源释放
        private bool _disposed = false;

       
        // 释放所有资源，防止内存泄漏和退出卡死
       
        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                // 先停止实时检测，防止后台线程继续访问已释放的资源
                if (_isGrabbing)
                {
                    StopLive();
                }

                // 取消相机事件订阅并关闭相机
                if (_cameraService != null)
                {
                    _cameraService.ImageGrabbed -= OnImageGrabbed;
                    _cameraService.StopGrabbing();
                    _cameraService.Close();
                }
            }
            catch { }

            // 释放所有定时器，防止回调继续执行
            _sensorCheckTimer?.Stop();
            _sensorCheckTimer?.Dispose();
            _sensorCheckTimer = null;

            _timeUpdateTimer?.Stop();
            _timeUpdateTimer?.Dispose();
            _timeUpdateTimer = null;

            _refreshTimer?.Stop();
            _refreshTimer?.Dispose();
            _refreshTimer = null;

            // 释放所有Halcon非托管对象
            if (_currentImage != null && _currentImage.IsInitialized())
            {
                _currentImage.Dispose();
                _currentImage = null;
            }

            if (_checkRectXid != null && _checkRectXid.IsInitialized())
            {
                _checkRectXid.Dispose();
                _checkRectXid = null;
            }

            if (_checkRectXid2 != null && _checkRectXid2.IsInitialized())
            {
                _checkRectXid2.Dispose();
                _checkRectXid2 = null;
            }

            // 释放系统状态监控
            _systemStatus?.Dispose();

            // 取消弱引用消息注册，防止收到消息后访问已释放的对象
            WeakReferenceMessenger.Default.Unregister<LogMessage>(this);

            // 释放数据库上下文
            _recordRepository?.Dispose();

            _disposed = true;
        }
        #endregion
    }
}
