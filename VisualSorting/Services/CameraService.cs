using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using VisualSorting.Services;

namespace VisualSorting.Services
{
    /// <summary>
    /// 相机服务实现类
    /// 封装Halcon图像采集功能，支持GigE工业相机
    /// 使用后台线程进行连续采集，通过事件机制向UI层传递图像
    /// 
    /// 【HObject所有权规则】
    /// CameraService是事件发布者，负责创建和释放事件中传递的HObject
    /// 订阅者（ViewModel）只读取事件中的HObject，不负责Dispose
    /// 订阅者如需保留图像，必须自行Clone，Clone出的副本由订阅者自行管理生命周期
    /// </summary>
    public class CameraService : ICameraService, IDisposable
    {
        #region 私有字段
        /// <summary>Halcon图像采集句柄，由OpenFramegrabber创建</summary>
        private HTuple hv_AcqHandle;
        /// <summary>取消令牌源，用于停止采集线程</summary>
        private CancellationTokenSource _cts;
        /// <summary>采集线程Task引用</summary>
        private Task _liveTask;
        /// <summary>线程锁，保护Close操作中的资源释放</summary>
        private readonly object _lock = new object();
        /// <summary>是否已释放标志</summary>
        private bool _disposed = false;
        #endregion

        #region 属性实现
        /// <summary>相机是否已打开</summary>
        public bool IsOpen => hv_AcqHandle != null && hv_AcqHandle.Length > 0;
        /// <summary>是否正在采集</summary>
        public bool IsGrabbing { get; private set; }
        /// <summary>当前采集的最新图像，HObject类型</summary>
        public object CurrentImage { get; private set; }
        #endregion

        #region 事件
        /// <summary>图像采集完成事件，每采集一帧图像触发一次，传递HObject给订阅者</summary>
        public event EventHandler<HObject> ImageGrabbed;
        #endregion

        #region 公共方法
        /// <summary>打开GigE工业相机，创建采集句柄</summary>
        public void Open()
        {
            Close();
            HOperatorSet.OpenFramegrabber("GigEVision2", 0, 0, 0, 0, 0, 0, "progressive",
       -1, "default", -1, "false", "default", "c42f90f6ad7a_GEV_MVCA06010GC", 0,
       -1, out hv_AcqHandle);
        }

        /// <summary>关闭相机，释放采集句柄</summary>
        public void Close()
        {
            lock (_lock)
            {
                (CurrentImage as HObject)?.Dispose();//安全转换类型释放图像
                CurrentImage = null;//引用滞空，让GC回收，避免内存泄露

                if (hv_AcqHandle != null)
                {
                    HOperatorSet.CloseFramegrabber(hv_AcqHandle);//停止相机采集
                    hv_AcqHandle = null;
                }
            }
        }

        /// <summary>启动连续采集，后台线程循环抓取图像并通过ImageGrabbed事件通知订阅者</summary>
        public void StartGrabbing()
        {
            if (!IsOpen) return;
            if (_liveTask != null && !_liveTask.IsCompleted) return;

            IsGrabbing = true;
            _cts = new CancellationTokenSource();
            _liveTask = Task.Run(() => LiveLoop(_cts.Token));//启动后台采集任务
        }

        /// <summary>停止连续采集，取消后台线程</summary>
        public void StopGrabbing()
        {
            _cts?.Cancel();

            try
            {
                _liveTask?.Wait(2000);
            }
            catch (AggregateException) { }
            finally
            {
                _cts?.Dispose();
                _cts = null;
                _liveTask = null;
            }
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 采集线程主循环
        /// 
        /// 【HObject所有权与生命周期管理】
        /// 1. ho_Image 由 GrabImage 创建，在 finally 中由本方法释放
        /// 2. imageClone 是为事件传递创建的副本，在 BeginInvoke 回调中所有订阅者处理完毕后释放
        /// 3. 订阅者收到 imageClone 后只读取，不 Dispose；如需保留则自行 Clone
        /// 4. 使用 BeginInvoke 异步投递，避免阻塞采集线程导致帧积压
        /// </summary>
        private void LiveLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                HObject ho_Image = null;
                try
                {
                    HOperatorSet.GrabImage(out ho_Image, hv_AcqHandle);

                    if (ho_Image == null || !ho_Image.IsInitialized())
                    {
                        continue;
                    }

                    lock (_lock)
                    {
                        (CurrentImage as HObject)?.Dispose();
                        CurrentImage = ho_Image.Clone();//克隆新图象
                    }
                    //ho_Image 是临时工（Clone完就释放）
                    //CurrentImage 是门面（只做缓存）
                    //imageClone 才是真正的干活者——它通过事件进入ViewModel，转灰度后一分为二：Clone给CurrentImage做 显示 
                    //Clone给processImage做 检测 ，而模板创建则是直接读CurrentImage（也就是当前屏幕上那帧图）
                    var imageClone = ho_Image.Clone();//创建了一个副本，防止订阅者收到的时候图像被回收了
                    if (System.Windows.Application.Current?.Dispatcher != null)
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>//吧后台线程切换到UI线程异步执行
                    {
                        try
                        {
                            // 事件触发：所有订阅者同步执行
                            // 订阅者只读取imageClone，不Dispose
                            // 如需保留图像，订阅者自行Clone
                            ImageGrabbed?.Invoke(this, imageClone);
                        }
                        catch (Exception)
                        {
                        }
                        finally
                        {
                            imageClone?.Dispose();
                        }
                    }));
                    }
                }
                catch (HalconException)
                {
                    ho_Image?.Dispose();
                    break;
                }
                catch (Exception)
                {
                    ho_Image?.Dispose();
                }
                finally
                {
                    // 释放GrabImage创建的原始图像
                    ho_Image?.Dispose();
                }
            }
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            if (!_disposed)
            {
                Close();
                _disposed = true;
            }
        }
        #endregion
    }
}
