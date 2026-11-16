using CommunityToolkit.Mvvm.Input;
using NLog;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VisualSorting.Common;
using VisualSorting.Entities;
using VisualSorting.Repositories;
using VisualSorting.Views;

namespace VisualSorting.ViewModels
{
    
    // 登录视图模型
  
    public partial class LoginViewModel : ObservableObject
    {
        /// <summary>NLog日志记录器，用于文件持久化日志</summary>
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

       
        // 当前登录用户信息，绑定到界面用户名和密码输入框
        
        public Member Member { get; set; } = new Member();

        /// <summary>登录命令，XAML绑定到"登录"按钮</summary>
        public ICommand LoginCommand { get; }
        /// <summary>退出命令，XAML绑定到"退出"按钮</summary>
        public ICommand ExitCommand { get; }

      
        // 预设测试账号，初始化登录和退出命令
       
        public LoginViewModel()
        {
            _logger.Info("========================================");
            _logger.Info("登录模块初始化开始");
            _logger.Info("应用程序启动，等待用户登录");

            Member.Name = "半岛冰茶";
            Member.Password = "1816301413";

            _logger.Debug($"预设测试账号已加载 - 用户名: {Member.Name}");

            LoginCommand = new AsyncRelayCommand(OnLoginCommandAsync);
            ExitCommand = new RelayCommand(OnExitCommand);

            _logger.Info("登录模块初始化完成");
        }

      
        // 退出命令处理
       
        private void OnExitCommand()
        {
            _logger.Info("用户点击退出按钮，应用程序即将关闭");
            _logger.Info("========================================");
            Environment.Exit(0);
        }

      
        // 验证流程：
        // 1. 检查用户名和密码是否为空
        // 2. 在数据库Member表中查找匹配的用户名和密码
        // 3. 匹配成功则切换到主界面，失败则提示错误
        // </summary>
        private async Task OnLoginCommandAsync()
        {
            _logger.Info("用户尝试登录");

            if (string.IsNullOrEmpty(Member.Name))
            {
                _logger.Warn("登录失败: 用户名为空");
                MessageBox.Show("用户名不能为空");
                return;
            }

            if (string.IsNullOrEmpty(Member.Password))
            {
                _logger.Warn("登录失败: 密码为空");
                MessageBox.Show("密码不能为空");
                return;
            }

            Member member = null;
            try
            {
                member = await Task.Run(() =>
                {
                    using (var memberRepository = new MemberRepository())
                    {
                        return memberRepository.GetAll()
                            .FirstOrDefault(p => p.Name == Member.Name && p.Password == Member.Password);
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "数据库连接不可用，启用离线单机凭据快速登录");
                if ((Member.Name == "半岛冰茶" && Member.Password == "1816301413") ||
                    (Member.Name == "admin" && Member.Password == "123456"))
                {
                    member = new Member { Name = Member.Name, Password = Member.Password, Role = 1 };
                }
            }

            if (member == null)
            {
                _logger.Warn($"登录失败: 用户名或密码错误 - 用户名: {Member.Name}");
                MessageBox.Show("用户名或者密码错误");
                return;
            }
            else
            {
                _logger.Info($"登录成功 - 用户: {Member.Name}");
                _logger.Debug($"正在切换到主界面，窗口尺寸: 1500x800");

                try
                {
                    AppData.Container.Content = new MainView();
                    AppData.MainWindow.Left = (SystemParameters.WorkArea.Width - AppData.MainWindow.Width) / 2;
                    AppData.MainWindow.Top = (SystemParameters.WorkArea.Height - AppData.MainWindow.Height) / 2;

                    _logger.Info("主界面加载完成");
                    _logger.Info("========================================");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"切换主界面时发生异常 - 用户: {Member.Name}");
                    MessageBox.Show($"界面切换失败: {ex.Message}");
                }
            }
        }
    }
}
