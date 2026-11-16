using HalconDotNet;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace VisualSorting.Services
{
    /// <summary>
    /// 文件回放相机：无相机时按顺序循环读取目录内的图片并输出，遵循与 CameraService 相同的 HObject 所有权规则
    /// </summary>
    public class FileCameraService : ICameraService, IDisposable
    {
        private readonly string _imageDir;
        private readonly int _intervalMs;
        private string[] _files = new string[0];
        private CancellationTokenSource _cts;
        private Task _liveTask;
        private readonly object _lock = new object();
        private bool _isOpen;

        public FileCameraService(string imageDir, int intervalMs = 200)
        {
            _imageDir = imageDir;
            _intervalMs = intervalMs;
        }

        public bool IsOpen => _isOpen;
        public bool IsGrabbing { get; private set; }
        public object CurrentImage { get; private set; }
        public event EventHandler<HObject> ImageGrabbed;

        public void Open()
        {
            Close();
            if (!Directory.Exists(_imageDir)) throw new DirectoryNotFoundException("仿真图片目录不存在: " + _imageDir);
            var exts = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".tif", ".tiff" };
            _files = Directory.GetFiles(_imageDir).Where(f => exts.Contains(Path.GetExtension(f).ToLowerInvariant())).OrderBy(f => f).ToArray();
            if (_files.Length == 0) throw new FileNotFoundException("仿真图片目录内没有图片: " + _imageDir);
            _isOpen = true;
        }

        public void Close()
        {
            StopGrabbing();
            lock (_lock)
            {
                (CurrentImage as HObject)?.Dispose();
                CurrentImage = null;
            }
            _isOpen = false;
        }

        public void StartGrabbing()
        {
            if (!_isOpen) return;
            if (_liveTask != null && !_liveTask.IsCompleted) return;
            IsGrabbing = true;
            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            _liveTask = Task.Run(() => LiveLoop(token));
        }

        public void StopGrabbing()
        {
            IsGrabbing = false;
            _cts?.Cancel();
            try { _liveTask?.Wait(2000); }
            catch (AggregateException) { }
            finally
            {
                _cts?.Dispose();
                _cts = null;
                _liveTask = null;
            }
        }

        private void LiveLoop(CancellationToken token)
        {
            int index = 0;
            while (!token.IsCancellationRequested)
            {
                HObject ho_Image = null;
                try
                {
                    HOperatorSet.ReadImage(out ho_Image, _files[index++ % _files.Length]);
                    lock (_lock)
                    {
                        (CurrentImage as HObject)?.Dispose();
                        CurrentImage = ho_Image.Clone();
                    }
                    var imageClone = ho_Image.Clone();
                    var dispatcher = System.Windows.Application.Current?.Dispatcher;
                    if (dispatcher == null) { imageClone.Dispose(); continue; }
                    dispatcher.BeginInvoke(new Action(() =>
                    {
                        try { ImageGrabbed?.Invoke(this, imageClone); }
                        catch (Exception) { }
                        finally { imageClone?.Dispose(); }
                    }));
                    token.WaitHandle.WaitOne(_intervalMs);
                }
                catch (HalconException)
                {
                    break;
                }
                catch (Exception) { }
                finally
                {
                    ho_Image?.Dispose();
                }
            }
        }

        public void Dispose() { Close(); }
    }
}
