﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using VisualSorting.Common;
using VisualSorting.Services;
using VisualSorting.ViewModels;

namespace VisualSorting
{
  
// - 创建 ServiceCollection （服务集合）
//- 注册所有服务 （单例/瞬态模式）
//- 构建 IServiceProvider （服务提供者）
//- 初始化 ServiceLocator （把容器交给定位器）
    public partial class App : Application
    {
      
        public App()
        {
            Services = ConfigureServices();
            ServiceLocator.Initialize(Services);
            this.InitializeComponent();
        }

      
        // 当前App实例的强类型访问
       
        public new static App Current => (App)Application.Current;

      
        // 依赖注入服务提供者
       
        public IServiceProvider Services { get; }

      
        //配置依赖注入容器
        //集中注册所有服务和ViewModel，管理它们的生命周期
       
        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // ========== 注册Services（单例模式）==========
            // 单例：整个应用生命周期内只创建一个实例，适合共享状态的服务
            services.AddSingleton<ITemplateService, TemplateService>();
            services.AddSingleton<IDetectionService, DetectionService>();
            bool useSim = string.Equals(System.Configuration.ConfigurationManager.AppSettings["UseSimulation"], "true", StringComparison.OrdinalIgnoreCase);
            if (useSim)
            {
                string imageDir = System.Configuration.ConfigurationManager.AppSettings["SimImageDir"];
                services.AddSingleton<ICameraService>(sp => new FileCameraService(imageDir));
                services.AddSingleton<IMotionControlService, SimMotionControlService>();
                services.AddSingleton<ISolenoidValueService, SimSolenoidValueService>();
                services.AddSingleton<ISensorService, SimSensorService>();
            }
            else
            {
                services.AddSingleton<ICameraService, CameraService>();
                services.AddSingleton<IMotionControlService, MotionControlService>();
                services.AddSingleton<ISolenoidValueService, SolenoidValueService>();
                services.AddSingleton<ISensorService, SensorService>();
            }
            services.AddSingleton<ILogService, LogService>();

            // ========== 注册ViewModels（瞬态模式）==========
            // 瞬态：每次请求创建新实例，适合页面级ViewModel
            // 不是每个ViewModel都必须注册，如果ViewModel不需要DI可以不注册
            services.AddTransient<MainViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<SystemStatusViewModel>();
            services.AddTransient<YoloDetectionViewModel>();

            return services.BuildServiceProvider();
        }
    }
}
