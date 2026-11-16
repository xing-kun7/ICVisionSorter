using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Threading;
using VisualSorting.Common;

namespace VisualSorting.ViewModels
{
    // 系统状态监控视图模型
    // 实时监控CPU使用率、内存使用率、程序运行时间
    // 数据通过DispatcherTimer每秒刷新一次，绑定到主界面设备状态区域
  
    public class SystemStatusViewModel : ObservableObject, IDisposable
    {
       
        [StructLayout(LayoutKind.Sequential)]
        private class MEMORYSTATUSEX
        {
            /// <summary>结构体大小，调用前必须设置为Marshal.SizeOf(typeof(MEMORYSTATUSEX))</summary>
            public uint dwLength;
            /// <summary>物理内存总量（字节）</summary>
            public ulong ullTotalPhys;
            /// <summary>可用物理内存（字节）</summary>
            public ulong ullAvailPhys;
            /// <summary>系统页面文件总量（字节）</summary>
            public ulong ullTotalPageFile;
            /// <summary>可用页面文件（字节）</summary>
            public ulong ullAvailPageFile;
            /// <summary>虚拟内存总量（字节）</summary>
            public ulong ullTotalVirtual;
            /// <summary>可用虚拟内存（字节）</summary>
            public ulong ullAvailVirtual;
            /// <summary>可用扩展虚拟内存（字节）</summary>
            public ulong ullAvailExtendedVirtual;

            /// <summary>构造函数：自动设置dwLength为结构体大小</summary>
            public MEMORYSTATUSEX()
            {
                dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }

        /// <summary>
        /// Win32 API：获取全局内存状态
        /// 返回物理内存、虚拟内存、页面文件的使用情况
        /// </summary>
        /// <param name="lpBuffer">MEMORYSTATUSEX结构体实例</param>
        /// <returns>true=成功获取内存状态</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalMemoryStatusEx(MEMORYSTATUSEX lpBuffer);

        /// <summary>CPU使用率（百分比），绑定到界面CPU进度条和文本</summary>
        private double _cpuUsage;
        /// <summary>内存使用率（百分比），绑定到界面内存进度条和文本</summary>
        private double _memoryUsage;
        /// <summary>程序运行时间文本，格式"HH:mm:ss"，绑定到界面运行时间显示</summary>
        private string _runningTime;
        /// <summary>UI线程定时器，每秒触发一次更新系统状态</summary>
        private readonly DispatcherTimer _updateTimer;
        /// <summary>CPU性能计数器，读取处理器使用率</summary>
        private PerformanceCounter _cpuCounter;
        /// <summary>应用程序启动时间，用于计算运行时长</summary>
        private DateTime _appStartTime;
        /// <summary>是否已释放标志</summary>
        private bool _disposedValue;
        /// <summary>是否已完成后台初始化标志</summary>
        private bool _isInitialized;
        /// <summary>初始化锁，保护_cpuCounter和_appStartTime的线程安全访问</summary>
        private readonly object _initLock = new object();

      
        //CPU使用率（百分比），界面绑定显示
      
        public double CpuUsage
        {
            get => _cpuUsage;
            set => SetProperty(ref _cpuUsage, value);
        }

        
        //内存使用率（百分比），界面绑定显示
       
        public double MemoryUsage
        {
            get => _memoryUsage;
            set => SetProperty(ref _memoryUsage, value);
        }

      
        // 程序运行时间文本，格式"HH:mm:ss"
        
        public string RunningTime
        {
            get => _runningTime;
            set => SetProperty(ref _runningTime, value);
        }

     
        public SystemStatusViewModel()
        {
            _appStartTime = DateTime.Now;
            _runningTime = "00:00:00";

            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _updateTimer.Tick += OnUpdateTimerTick;

            Task.Run(() => InitializeInBackground());
        }

        
        // 后台初始化PerformanceCounter
       
        private void InitializeInBackground()
        {
            try
            {
                var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _ = cpuCounter.NextValue();

                var startTime = Process.GetCurrentProcess().StartTime;

                lock (_initLock)
                {
                    _cpuCounter = cpuCounter;
                    _appStartTime = startTime;
                    _isInitialized = true;
                }

                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    UpdateSystemStatus();
                    _updateTimer.Start();
                }));
            }
            catch (Exception)
            {
            }
        }

       
        // 定时器回调，每秒触发一次
       
        private void OnUpdateTimerTick(object sender, EventArgs e)
        {
            UpdateSystemStatus();
        }

       
        // 更新系统状态数据
        
        private void UpdateSystemStatus()
        {
            if (!_isInitialized) return;

            try
            {
                PerformanceCounter counter;
                DateTime startTime;

                lock (_initLock)
                {
                    counter = _cpuCounter;
                    startTime = _appStartTime;
                }

                if (counter != null)
                {
                    CpuUsage = Math.Round(counter.NextValue(), 1);
                }

                var memInfo = new MEMORYSTATUSEX();
                if (GlobalMemoryStatusEx(memInfo))
                {
                    double memoryRate = (double)(memInfo.ullTotalPhys - memInfo.ullAvailPhys) / memInfo.ullTotalPhys * 100;
                    MemoryUsage = Math.Round(memoryRate, 1);
                }

                TimeSpan runDuration = DateTime.Now - startTime;
                RunningTime = $"{(int)runDuration.TotalHours:D2}:{runDuration.Minutes:D2}:{runDuration.Seconds:D2}";
            }
            catch (Exception)
            {
            }
        }

        //释放资源（公共方法）
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        
        // 释放资源的核心方法
       
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _updateTimer.Stop();
                    _updateTimer.Tick -= OnUpdateTimerTick;
                    _cpuCounter?.Dispose();
                }
                _disposedValue = true;
            }
        }

        // 析构函数，防止用户忘记调用Dispose
        
        ~SystemStatusViewModel()
        {
            Dispose(false);
        }
    }
}
