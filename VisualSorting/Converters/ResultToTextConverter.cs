using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace VisualSorting.Converters
{
    /// <summary>
    /// 检测结果文本转换器
    /// 将QualifiedCount（合格数量）转换为界面显示的"合格"/"不合格"文本
    /// 
    /// 转换规则：
    /// - qualifiedCount == 1 → "合格"（表示该次检测合格）
    /// - qualifiedCount == 0 → "不合格"（表示该次检测不合格）
    /// - 其他值 → "未知"
    /// 
    /// 使用者：XAML中DataGrid列绑定，将ProductInspectionRecord.QualifiedCount转换为可读文本
    /// 注意：此转换器仅支持单向绑定（Convert），不支持ConvertBack
    /// </summary>
    public class ResultToTextConverter : IValueConverter
    {
        /// <summary>
        /// 将QualifiedCount转换为"合格"/"不合格"文本
        /// </summary>
        /// <param name="value">源值，ProductInspectionRecord.QualifiedCount（int类型）</param>
        /// <param name="targetType">目标类型</param>
        /// <param name="parameter">转换参数（未使用）</param>
        /// <param name="culture">区域信息（未使用）</param>
        /// <returns>"合格"/"不合格"/"未知"</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
           if(value is int qualifiedCount)
            {
                return qualifiedCount== 1 ? "合格" : "不合格";
            }
            return "未知";
        }

        /// <summary>
        /// 反向转换（不支持）
        /// 此转换器仅用于显示，不支持从文本反向转换为数值
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
