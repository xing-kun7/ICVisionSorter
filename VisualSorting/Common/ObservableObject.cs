using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VisualSorting.Common
{
    /// <summary>
    /// 可观察对象基类
    /// 实现INotifyPropertyChanged接口，为所有ViewModel提供属性变更通知机制
    /// 当属性值发生变化时，自动通知UI更新（WPF数据绑定的核心）
    /// </summary>
    public class ObservableObject : INotifyPropertyChanged
    {
        /// <summary>
        /// 属性变更事件
        /// WPF绑定引擎监听此事件，当属性变化时自动更新UI
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 触发属性变更通知
        /// </summary>
        /// <param name="propertyName">发生变更的属性名称，由CallerMemberName自动填充</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 通用属性设置方法
        /// 仅当新值与旧值不同时才更新并触发通知，避免不必要的UI刷新
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="field">属性的后台字段引用</param>
        /// <param name="value">新值</param>
        /// <param name="propertyName">属性名称，由CallerMemberName自动填充</param>
        /// <returns>true=值已变更并已通知，false=值未变更</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
