using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VisualSorting.ViewModels;

namespace VisualSorting.Views
{
  
    //登录界面视图
  
    public partial class Login : UserControl
    {
        /// <summary>
        /// 构造函数：初始化组件并绑定ViewModel
        /// </summary>
        public Login()
        {
            InitializeComponent();
            LoginViewModel vm = new LoginViewModel();
            DataContext = vm;
        }

       
        //触发入场动画，提升用户体验
      
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            StartEntryAnimations();
        }

       
        // 启动入场动画
      
        private void StartEntryAnimations()
        {
            // 主卡片动画：从下方滑入并淡入
            var mainCard = this.FindName("MainCard") as Border;
            if (mainCard != null)
            {
                var cardStoryboard = new Storyboard();

                // Y轴位移动画：从80px下方位移到原位
                var translateYAnim = new DoubleAnimationUsingKeyFrames();
                translateYAnim.KeyFrames.Add(new EasingDoubleKeyFrame(80, KeyTime.FromTimeSpan(TimeSpan.Zero)));
                translateYAnim.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.9)))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                });
                Storyboard.SetTarget(translateYAnim, mainCard);
                Storyboard.SetTargetProperty(translateYAnim, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[1].(TranslateTransform.Y)"));
                cardStoryboard.Children.Add(translateYAnim);

                // 透明度动画：从0到1
                var opacityAnim = new DoubleAnimationUsingKeyFrames();
                opacityAnim.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
                opacityAnim.KeyFrames.Add(new EasingDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.7)))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                });
                Storyboard.SetTarget(opacityAnim, mainCard);
                Storyboard.SetTargetProperty(opacityAnim, new PropertyPath(UIElement.OpacityProperty));
                cardStoryboard.Children.Add(opacityAnim);
                cardStoryboard.Begin();
            }

            // Logo缩放动画：从0放大到1，带有弹性效果
            var logoSection = this.FindName("LogoSection") as StackPanel;
            if (logoSection != null)
            {
                var logoStoryboard = new Storyboard();

                // X轴缩放动画
                var scaleXAnim = new DoubleAnimationUsingKeyFrames();
                scaleXAnim.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
                scaleXAnim.KeyFrames.Add(new EasingDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.8)))
                {
                    EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.5 }
                });
                Storyboard.SetTarget(scaleXAnim, logoSection);
                Storyboard.SetTargetProperty(scaleXAnim, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)"));
                logoStoryboard.Children.Add(scaleXAnim);

                // Y轴缩放动画
                var scaleYAnim = new DoubleAnimationUsingKeyFrames();
                scaleYAnim.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
                scaleYAnim.KeyFrames.Add(new EasingDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.8)))
                {
                    EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.5 }
                });
                Storyboard.SetTarget(scaleYAnim, logoSection);
                Storyboard.SetTargetProperty(scaleYAnim, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)"));
                logoStoryboard.Children.Add(scaleYAnim);

                // 透明度动画
                var logoOpacityAnim = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.6));
                Storyboard.SetTarget(logoOpacityAnim, logoSection);
                Storyboard.SetTargetProperty(logoOpacityAnim, new PropertyPath(UIElement.OpacityProperty));
                logoStoryboard.Children.Add(logoOpacityAnim);
                logoStoryboard.Begin();
            }

            // 依次触发各区域的淡入动画（交错效果）
            StartStaggeredFadeIn("UsernameSection", TimeSpan.FromSeconds(0.3));
            StartStaggeredFadeIn("PasswordSection", TimeSpan.FromSeconds(0.5));
            StartStaggeredFadeIn("ButtonSection", TimeSpan.FromSeconds(0.7));
            StartStaggeredFadeIn("FooterSection", TimeSpan.FromSeconds(0.9));
        }

       
        // 启动交错淡入动画
       
        private void StartStaggeredFadeIn(string elementName, TimeSpan delay)
        {
            var element = this.FindName(elementName) as FrameworkElement;
            if (element != null)
            {
                var storyboard = new Storyboard();

                // 透明度动画：延迟后淡入
                var opacityAnim = new DoubleAnimationUsingKeyFrames();
                opacityAnim.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
                opacityAnim.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(delay)));
                opacityAnim.KeyFrames.Add(new EasingDoubleKeyFrame(1, KeyTime.FromTimeSpan(delay + TimeSpan.FromSeconds(0.5)))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                });
                Storyboard.SetTarget(opacityAnim, element);
                Storyboard.SetTargetProperty(opacityAnim, new PropertyPath(UIElement.OpacityProperty));
                storyboard.Children.Add(opacityAnim);

                // Y轴位移动画：延迟后从下方滑入
                var translateYAnim = new DoubleAnimationUsingKeyFrames();
                translateYAnim.KeyFrames.Add(new EasingDoubleKeyFrame(20, KeyTime.FromTimeSpan(TimeSpan.Zero)));
                translateYAnim.KeyFrames.Add(new EasingDoubleKeyFrame(20, KeyTime.FromTimeSpan(delay)));
                translateYAnim.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(delay + TimeSpan.FromSeconds(0.5)))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                });
                Storyboard.SetTarget(translateYAnim, element);
                Storyboard.SetTargetProperty(translateYAnim, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[1].(TranslateTransform.Y)"));
                storyboard.Children.Add(translateYAnim);
                storyboard.Begin();
            }
        }
    }
}
